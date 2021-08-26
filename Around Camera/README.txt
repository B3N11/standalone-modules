AROUND CAMERA INFORMATION
*************************

Description:
AroundCamera (AC) is a C# script with a class that inherits from Unity's MonoBehaviour. It can be attached to GameObjects in Unity to use it. AroundCamera is a Third-person camera that can be rotated around a target. It has features such as collision detection (avoid wall-clipping) and lock-on (follows another target and keeps original target in sight).

Setup:
Create a camera in the scene and attach this script to it. Setup the properties as you like. If you only want to use it for player following, then add a reference to it in the `target` Transform. Done!
