using UnityEngine;

namespace PeakVR;

internal class VRNetSync : MonoBehaviour
{
    private float sinceSend;

    private void Update()
    {
        VRFingers.SampleLocal();

        sinceSend += Time.deltaTime;
        var rate = Plugin.Config != null ? Plugin.Config.HandSyncRate.Value : 20f;
        if (sinceSend < 1f / Mathf.Max(5f, rate))
            return;

        sinceSend = 0f;

        var c = Character.localCharacter;
        if (c == null || c.refs == null || c.refs.head == null
            || c.refs.IKHandTargetLeft == null || c.refs.IKHandTargetRight == null)
            return;

        VRNetworking.SendPose(c);
    }
}
