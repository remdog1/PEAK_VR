# NOT COMPATIBLE ANYMORE - NOT WORKING WITH UPDATES PAST 1.44.a !!

## **Manual Installation:**
- Extract the PEAK_VR zip file in the game root folder (use steam, on the game, manage, browse game files).
- This mod uses a virtual gamepad emulation: **YOU MUST INSTALL VigemBus_1.22.0_x64_x86_arm64.exe from the folder bepinex/redist**

## **Mod managers:**
If you are already using a mod manager, try to make a copy of the game folder and extract the vr mod zip in this copy if steam allows launching from another folder.
Unfortunately you will have to put any other mod you wanna use in the game folder in BepInEx/plugins.
That might break the game or conflict with the mod manager.

If you want to stop using the VR mod and use mods from a mod manager again, rename the file in the game root folder "winhttp.dll" to something like "winhttp2.dll".


## **VR Runtime and Graphics API:**
- The game can be run using Vulkan or D3D12. AND the VR mod supports OpenVR by default but can also support OpenXR. Some combinations will crash the game, or not show anything in the VR headset. 
**There are four possibilites:**
	- OpenVR+Vulkan **(RECOMMENDED)**
	- OpenVR+D3D12 (no image in the headset for me)
	- OpenXR+Vulkan (crashes for me)
	- OpenXR+D3D12 (works for me, no image on the flatscreen)
	
You need to find which combination works for you if OpenVR+Vulkan does not work.
You can turn on an additional camera if you need to stream or show the spectator view (see UnityVR_Bepinex.cfg below) or use an OpenVR plugin to capture the view from the headset directly.

## **Controls:**
- VR controllers are mapped as a xbox controller (A,B,X,Y on same buttons, LeftGrip is LB, RightGrip is RB, left trigger is LT, right trigger is RT) BUT for convenience, I swapped some of the controls : X and B are swapped, Left Trigger and Right trigger are swapped. 
ONSCREEN icons are corrected accordingly.
- "hotkey gesture" : put your left hand close to the left side of your head (in real life), the controller will vibrate. While it vibrates, the left joystick becomes the D-Pad, left joystick click becomes the "back" button, right joystick click becomes the "start" button.
- **click both joystick to recenter the VR view** 
- **THERE ARE TWO types of laser pointers:**
    - WHITE : used for interactions with kiosk, items, characters, throwing items. It follows the position of your real hand at all times.
    - RED : used for shootable items, it helps aiming and are attached to the items itself. Shootable items will shoot where they are pointing at.

## **Features:**
- 3D stereoscopic VR view
- 6DOF head tracking
- 6DOF hand tracking
- bHaptics support
- Left handed mode : After the first launch with the mod installed, a config file will be created in bepinex/config/ called "PEAK_VR.cfg". Edit this file and change "leftHanded" to "true"


## WHAT THE MOD DOES NOT DO:
- There is no "hand movement climbing" unfortunately, that would be a huge work to add this if even possible.
- You cannot see other VR players hands moving in real time because that is not something the game synchronizes and I am not gonna implement it because in VR you know what people do with their hands: Peak is played by a lot of kids and I will not allow this feature to be added to the original game. 

## **Config files:**
In BepInEx/config you will find different files.

**=> PEAK_VR.cfg** :

~~~
## Settings file was created by plugin PEAK_VR v1.0.0
## Plugin GUID: PEAK_VR

[General]

## UI mode
# Setting type: String
# Default value: frontOfCam
# Acceptable values: frontOfGameCam, headFixed
UIMode = frontOfGameCam

## AttachUIToHand only works with UI mode frontOfGameCam
# Setting type: String
# Default value: false
# Acceptable values: true, false
AttachUIToHand = false

## Fog and other overlay effects can be a little weird in headset, disable if you don't like it
# Setting type: String
# Default value: false
# Acceptable values: false, true
disableFog = false

## Aim with hand or head
# Setting type: String
# Default value: hand
# Acceptable values: hand, head
aimingMode = hand

## left Handed mode
# Setting type: String
# Default value: false
# Acceptable values: false, true
leftHanded = false
~~~

- UIMode : three UI modes available
	- UIMode = frontOfGameCam + AttachUIToHand = false: flat in front of the game camera, follows game camera rotation, NOT the head rotations.
	- UIMode = frontOfGameCam + AttachUIToHand = true: attaches the UI (stamina and inventory) to the right hand, the rest is in front of the head.
	- UIMode = headFixed: everything is fixed relative to VR head and follows head movements.

You can change those values to create your own prefered UI mode. Find the UI that suits you the best.

- The fog ingame has a visual issue where both eyes don't show exactly the same thing, it can be annoying but is a minor issue, if you find it too annoying you can disable it here.
- Default aiming method is right hand aiming. If you prefer head aiming you can change it here and replace "aimingMode = hand" with "aimingMode = head". (ONLY works with interactions, shootable items will require using your hand)
- When using leftHanded mode, buttons mapping stays the same !! It only affects aiming hand and shooting hand.


**=> UnityVR_Bepinex.cfg**

```
## Use OpenVR or OpenXR
# Setting type: String
# Default value: OpenVR
# Acceptable values: OpenXR, OpenVR
vrApi = OpenVR
```

- the vr mod uses OpenVR. If you want to use OpenXR, edit the file and search for "vrApi" (at the top of the file) and change "vrApi = OpenVR" to "vrApi = OpenXR".

- If you need to turn on the spectator view change this setting to true:
```
## Create custom mirror view for flatscreen, useful for Unity 6
# Setting type: String
# Default value: false
createMirrorView = true
```

- **LEAVE ALL the rest as it is :)**

**=> BepInEx.cfg**

!! the game is known to crash if the logging console is active, at least on first launch. So it is disabled by default. After a successful launch, I highly recommend you change these values, so that the log file can be created.

```
[Logging.Console]

## Enables showing a console for log output.
# Setting type: Boolean
# Default value: false
Enabled = true

[Logging.Disk]

## Include unity log messages in log file output.
# Setting type: Boolean
# Default value: false
WriteUnityLog = true

## Enables writing log messages to disk.
# Setting type: Boolean
# Default value: true
Enabled = true
```

## **Known issues:**
- Depending on your pc + headset configuration, launching the game with OpenVR or OpenXR and Vulkan or D3D12 will produce different results. For me here are the combinations :
OpenVR + Vulkan : VR ok, flatscreen not ok
OpenVR + D3D12: VR no image, flatscreen image ok
OpenXR + Vulkan: crash
OpenXR + D3D12: VR image ok, flatscreen image not ok

- The mod does not allow keyboard and mouse control, the input scheme is LOCKED to gamepad. This is to prevent the game from constantly switching between xbox controller and whatever VR controller it recognizes as a gamepad !

- The fog ingame has a visual issue where both eyes don't show exactly the same thing, as stated in the config section above.

- Binoculars are not working in VR.

## **Troubleshooting:**

- **if you installed the mod manually and want to deactivate the mod without uninstalling it, just rename the file "winhttp.dll" in the game root folder to anything else, like "winhttp2.dll"**

- If using Virtual Desktop, make sure you don't have any gamepad emulation active: in the headset, in "input", nothing should be checked on the right side (Gamepad, Dpad)

- When VR is launched, you should hear a "windows sound" similar to "usb device connected", meaning that Vigem bus driver is working properly. If you don't hear this sound, reinstall Vigem and/or reboot your pc and try again.

- gamepad icons and interactions icons can be messed up, it does happen in the original game too when using a gamepad, after a few item uses, it should fix itself (this is a game original bug, not the mod).

- VR hand use Inverse Kinematics, that "tries to force" the hand object to a target position and articulate the skeleton accordingly but with this skeleton length restrictions. Meaning that the hands won't be at the exact positions of your VR controllers. Every body parts has collision and physics, so there are positions that are not possible.

- if you're having performance issues lower your settings ingame : lower max fps to 100 or under, disable Ambient Occlusion, lower the headset resolution IN STEAMVR, the game resolution does not affect the VR resolution.

- if your hands are a bit weirdly positioned, make sure you are properly centered, using first steamVR recenter function, then click both joysticks, in this order. Hands use physics and have collisions, they will do weird stuff.

- if using Virtual Desktop, in the headset app, in the input section, make sure there is no gamepad emulation checked at all.

- if you have issues with the UI when using the headFixed UI (too small, too big, out of view), change the game resolution in settings. Try using 1920x1080 for a medium scaling.

- if using Vulkan and it crashes the game, run the game in flat mode (rename winhttp.dll) and go to the settings. Turn Ambient Occlusion OFF and reduce the max fps under 120.

## **Other mods compatibility:**
There is NO guarantee that this mod is compatible with other mods !

## **Important**
- DO NOT UNDER ANY CIRCUMSTANCE COMPLAIN ABOUT BUGS TO THE DEVELOPERS WHILE USING MODS. UNINSTALL MODS IF YOU ENCOUNTER BUGS AND THEN REPORT THEM IF THEY ARE STILL PRESENT.

- Please search the PEAK Steam discussions if you face bugs as they likely could be caused by the game and not the mod.

