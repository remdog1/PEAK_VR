using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PeakVR;

/// <summary>
/// Curls the character's finger bones from the VR controllers' analog grip and trigger,
/// for the local VR player and for remote VR players (their values arrive with the hand pose).
/// Grip closes the middle, ring and pinky fingers, trigger closes the index finger, and the
/// thumb follows whichever is pressed harder. Pressing both makes a fist; releasing both opens the hand.
/// </summary>
[DefaultExecutionOrder(2002)]
internal class VRFingers : MonoBehaviour
{
    public struct Curl
    {
        public float grip;
        public float trigger;
    }

    private const float SmoothSpeed = 20f;
    private const float LocalTimeout = 0.5f;

    // PEAK's rig names finger bones Finger_<finger>_<segment>_<L|R>; finger 1 is the thumb.
    private const int FingerCount = 5;
    private const int SegmentCount = 3;

    private static InputAction leftGrip, rightGrip, leftTrigger, rightTrigger;

    // Written by VRNetSync while a VR session is running.
    public static Curl LocalLeft, LocalRight;
    private static float localStamp = -1f;

    private class Bone
    {
        public Transform t;
        public int finger;
        public int segment;
        public bool left;
        public Quaternion written;
        public Quaternion rest;
        public bool hasWritten;
    }

    private class HandState
    {
        public readonly List<Bone> bones = new();
        public Curl left, right;
        public bool found;
    }

    private static readonly Dictionary<Character, HandState> States = new();
    private readonly List<Character> dead = new();

    public static void InitInput()
    {
        if (leftGrip != null)
            return;
        leftGrip = Axis("VR LeftGrip Axis", "LeftHand", "grip");
        rightGrip = Axis("VR RightGrip Axis", "RightHand", "grip");
        leftTrigger = Axis("VR LeftTrigger Axis", "LeftHand", "trigger");
        rightTrigger = Axis("VR RightTrigger Axis", "RightHand", "trigger");
    }

    private static InputAction Axis(string name, string hand, string control)
    {
        var a = new InputAction(name, InputActionType.Value, expectedControlType: "Axis");
        VRControls.Bind(a, hand, control);
        a.Enable();
        return a;
    }

    /// <summary>Called from VRNetSync every frame while in VR.</summary>
    public static void SampleLocal()
    {
        InitInput();
        LocalLeft = new Curl { grip = Read(leftGrip), trigger = Read(leftTrigger) };
        LocalRight = new Curl { grip = Read(rightGrip), trigger = Read(rightTrigger) };
        localStamp = Time.unscaledTime;
    }

    private static float Read(InputAction a)
    {
        try { return Mathf.Clamp01(a.ReadValue<float>()); }
        catch { return 0f; }
    }

    public static bool LocalActive => localStamp >= 0f && Time.unscaledTime - localStamp < LocalTimeout;

    public static byte[] PackLocal() => new[]
    {
        ToByte(LocalLeft.grip), ToByte(LocalLeft.trigger),
        ToByte(LocalRight.grip), ToByte(LocalRight.trigger),
    };

    public static void Unpack(byte[] b, out Curl left, out Curl right)
    {
        left = new Curl { grip = b[0] / 255f, trigger = b[1] / 255f };
        right = new Curl { grip = b[2] / 255f, trigger = b[3] / 255f };
    }

    private static byte ToByte(float v) => (byte)Mathf.RoundToInt(Mathf.Clamp01(v) * 255f);

    public static void Forget(Character c)
    {
        if (c == null || !States.TryGetValue(c, out var s))
            return;
        // Put the bones back so a player who left VR doesn't keep a frozen fist.
        foreach (var b in s.bones)
            if (b.t != null && b.hasWritten && b.t.localRotation == b.written)
                b.t.localRotation = b.rest;
        States.Remove(c);
    }

    private void LateUpdate()
    {
        var cfg = Plugin.Config;
        if (cfg == null || !cfg.FingerTracking.Value)
            return;

        var t = Mathf.Clamp01(Time.deltaTime * SmoothSpeed);

        var local = Character.localCharacter;
        if (local != null)
        {
            if (LocalActive)
                Drive(local, LocalLeft, LocalRight, t);
            else if (States.ContainsKey(local))
                Forget(local);
        }

        foreach (var kv in VRNetworking.Remotes)
        {
            var pose = kv.Value;
            if (!pose.hasFingers || pose.sinceReceived > VRNetworking.StaleTime)
                continue;
            if (!PlayerHandler.TryGetCharacter(kv.Key, out var c) || c == null || c == local)
                continue;
            Drive(c, pose.leftCurl, pose.rightCurl, t);
        }

        dead.Clear();
        foreach (var c in States.Keys)
            if (c == null)
                dead.Add(c);
        foreach (var c in dead)
            States.Remove(c);
    }

    private static void Drive(Character c, Curl left, Curl right, float t)
    {
        if (!States.TryGetValue(c, out var s))
        {
            s = new HandState();
            Find(c, s);
            States[c] = s;
        }
        if (!s.found)
            return;

        s.left.grip = Mathf.Lerp(s.left.grip, left.grip, t);
        s.left.trigger = Mathf.Lerp(s.left.trigger, left.trigger, t);
        s.right.grip = Mathf.Lerp(s.right.grip, right.grip, t);
        s.right.trigger = Mathf.Lerp(s.right.trigger, right.trigger, t);

        var cfg = Plugin.Config;
        var axis = cfg.FingerCurlAxis.Value switch
        {
            FingerAxis.X => Vector3.right,
            FingerAxis.Y => Vector3.up,
            _ => Vector3.forward,
        };
        var maxAngle = cfg.FingerCurlAngle.Value;
        var invert = cfg.FingerCurlInvert.Value ? -1f : 1f;

        foreach (var b in s.bones)
        {
            if (b.t == null)
            {
                s.found = false;
                States.Remove(c);
                return;
            }

            // If the animator wrote this bone since our last write, its pose is the new base
            // (keeps the game's own finger animation). Otherwise reuse the cached rest pose so the
            // curl doesn't compound frame after frame.
            var current = b.t.localRotation;
            if (!b.hasWritten || current != b.written)
                b.rest = current;

            var h = b.left ? s.left : s.right;
            float amount = b.finger switch
            {
                1 => Mathf.Max(h.grip, h.trigger) * 0.6f, // thumb
                2 => h.trigger,                           // index
                _ => h.grip,                              // middle, ring, pinky
            };

            // Base segments bend a bit less than the tips, like a real hand.
            float segScale = b.segment == 1 ? 0.8f : 1f;
            float side = b.left ? -1f : 1f;
            var angle = amount * maxAngle * segScale * invert * (cfg.FingerMirrorLeft.Value ? side : 1f);

            b.written = b.rest * Quaternion.AngleAxis(angle, axis);
            b.t.localRotation = b.written;
            b.hasWritten = true;
        }
    }

    private static void Find(Character c, HandState s)
    {
        var all = c.GetComponentsInChildren<Transform>(true);
        var byName = new Dictionary<string, Transform>();
        foreach (var tr in all)
            if (tr.name.StartsWith("Finger_") && !byName.ContainsKey(tr.name))
                byName[tr.name] = tr;

        for (int side = 0; side < 2; side++)
        {
            var suffix = side == 0 ? "L" : "R";
            for (int f = 1; f <= FingerCount; f++)
                for (int seg = 1; seg <= SegmentCount; seg++)
                    if (byName.TryGetValue($"Finger_{f}_{seg}_{suffix}", out var tr))
                        s.bones.Add(new Bone { t = tr, finger = f, segment = seg, left = side == 0 });
        }

        s.found = s.bones.Count > 0;
        if (!s.found)
            Plugin.Log.LogWarning($"[PeakVR][Fingers] no finger bones found on '{c.characterName}'");
        else if (Plugin.Config.EnableVerboseLogging.Value)
            Plugin.Log.LogInfo($"[PeakVR][Fingers] '{c.characterName}': {s.bones.Count} finger bones");
    }
}

public enum FingerAxis
{
    X,
    Y,
    Z,
}
