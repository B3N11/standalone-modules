AROUND CAMERA INFORMATION
*************************

Description:
AroundCamera (AC) is a C# script with a class that inherits from Unity's MonoBehaviour. It can be attached to GameObjects in Unity to use it. AroundCamera is a Third-person camera that can be rotated around a target. It has features such as collision detection (avoid wall-clipping) and lock-on (follows another target and keeps original target in sight).

-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

USAGE:
AC is not static, you will need a reference for it in your script where you want to feed the camera the input data.
AC does not do the input handling for you. It requires two float values in order to operate.
Call

	SetValues(float x, float y)

to set the input values. It is important to call this function in the Update() method! An example for usage:

	private void Update()
	{
		float x = Input.GetAxis("Horizontal");
		float y = Input.GetAxis("Vertical");

		myAroundCamera.SetValues(x, y);
	}

This will make your camera rotate around the target.

---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

PROPERTIES:

float maxDistance: Limits how far the camera can go behind the target.

float minDistance: Limits how close the camera can get to the target. Usually better be set to 0.

float height: A vertical offset for the target. If it is not zero, it will be added to target's vertical position.

float horizontalSpeed: Sets the camera rotation speed horizontally.

float verticalSpeed: Sets the camera rotation speed vertically.

Vector3 startRotation: Sets the rotation of the camera on the Start time.

---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
