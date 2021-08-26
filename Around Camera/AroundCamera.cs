using System;
using System.Collections.Generic;
using UnityEngine;

public class AroundCamera : MonoBehaviour
{
    #region Fields
    //General Data
    [Header("General Data")]
    public bool active = true;
    [SerializeField] private float maxDistance = 5f;
    [SerializeField] private float minDistance = 0f;        //Usually should be left on 0
    [SerializeField] private float height = 0f;
    private float finalDistance = 0f;

    //Movement Data
    [Header("Movement Data")]
    [SerializeField] private float horizontalSpeed = 200f;
    [SerializeField] private float verticalSpeed = 100f;
    [SerializeField] private Vector3 startRotation;
    private Quaternion rotation;
    private Vector3 faceDirection;
    private float speedX = 0f;
    private float speedY = 0f;

    //Lock-On Data
    [Header("Lock-On Data")]
    public bool lockedOn = false;
    [SerializeField] private string lockOnTag = "Enemy";
    [SerializeField] private float lockOnRange = 30f;
    [SerializeField] private float lockOnHeight = 0f;
    [SerializeField] private float lockOnAngleMin = -20f;
    [SerializeField] private float lockOnAngleMax = 20f;
    [SerializeField] private float lockOnSearchRayDensity = 1f;     //Defines the angle that is rotated before the next raycast. Example: If the lockOnAngle goes from -20 to 20 and the lockOnSearchRayDensity is 1, then there will be 41 rays cast. (From -20° to 0° to 20°)
    private List<Transform> lockOnTargets = new List<Transform>();
    [SerializeField] private float lockOnPointHeight = 1f;

    //Input Data
    [Header("Input Data")]
    public string horizontalAxis;
    public string verticalAxis;
    public string lockOnButton;

    //Debug Data
    [Header("Debug Data")]
    [SerializeField] private bool enableDebug = true;

    //References
    [Header("References")]
    [SerializeField] private Transform target;
    [SerializeField] private Transform lockOnRayStartPosition;
    [SerializeField] private GameObject lockOnPoint;
    private Transform lockOnTarget;
    #endregion

    #region Unity Functions
    private void Start()
    {
        SetupStartingRotation();
    }

    private void LateUpdate()
    {
        if (!active)
            return;

        SetRotation();
        CalculateFinalPosition();
        if (enableDebug) { DoDebug(); DrawLockOnField(); }
    }
    #endregion

    #region Access Functions
    /// <summary>
    /// Sets the rotation input values. Needs to be called via code to pass the values.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    public void SetValues(float x, float y)
    {
        speedX = x;
        speedY = y;
    }

    /// <summary>
    /// Get the direction the camera is facing horizontally. (Same as the camera.transform.forward, but you dont get the height value.)
    /// </summary>
    /// <returns></returns>
    public Vector3 GetFaceDirection()
    {
        faceDirection = new Vector3(transform.forward.x, 0, transform.forward.z);
        return faceDirection;
    }

    ///<summary>
    /// Tries to lock on to a target in front of the camera.
    ///</summary>
    ///<returns>Returns the result of the operation.</returns>
    public bool ToggleLockOn(bool enable)
    {
        bool result = false;
        if (enable) { result = TryLockOn(); }
        else { result = FreeCamera(); }
        //lockOnPoint.SetActive(result);
        return result;
    }
    #endregion

    #region Lock-On
    private bool TryLockOn()
    {
        lockedOn = FindTargets();
        if (lockedOn) { SetTarget(); }
        return lockedOn;
    }

    private bool FindTargets()
    {
        bool result = false;
        float angle = lockOnAngleMin;

        //Calculate number of rays to cast
        float numberOfRays = (-lockOnAngleMin + lockOnAngleMax) + 1;
        numberOfRays = numberOfRays / lockOnSearchRayDensity;
        numberOfRays = Mathf.Round(numberOfRays);

        //Search for targets
        for (int i = 0; i < numberOfRays; i++)
        {
            RaycastHit hit;
            Vector3 direction = (Quaternion.Euler(0, angle, 0) * faceDirection).normalized;
            if (Physics.Raycast(lockOnRayStartPosition.position, direction, out hit, lockOnRange))
            {
                if (hit.transform.root.gameObject.tag == lockOnTag)
                {
                    result = true;
                    lockOnTargets.Add(hit.transform.root);
                }
            }
            angle += lockOnSearchRayDensity;
        }

        //Return
        return result;
    }

    private void SetTarget()
    {
        float maxAngle = lockOnAngleMax;
        Transform currentTrans = null;

        foreach (Transform current in lockOnTargets)
        {
            Vector3 direction = (current.position - lockOnRayStartPosition.position).normalized;
            float angle = Vector3.SignedAngle(direction, faceDirection, Vector3.up);
            if (angle < maxAngle) { maxAngle = angle; currentTrans = current; }
        }

        lockOnTarget = currentTrans;
    }

    private void SetLockOnPoint()
    {
        lockOnPoint.transform.position = lockOnTarget.position + lockOnTarget.up * lockOnPointHeight;
        lockOnPoint.transform.forward = lockOnPoint.transform.position - target.position;
    }

    private bool FreeCamera()
    {
        lockOnTarget = null;
        lockedOn = false;
        return false;
    }
    #endregion

    #region Other Functions
    private void SetupStartingRotation()
    {
        //Set rotation values
        rotation.x = startRotation.x;
        rotation.y = startRotation.y;
        rotation.z = startRotation.z;

        //Apply rotation
        transform.rotation = Quaternion.Euler(rotation.y, rotation.x, rotation.z);
    }

    private void SetRotation()
    {
        if (!lockedOn) { NormalRotation(); }
        else { LockOnRotation(); }
    }

    private void NormalRotation()
    {
        //Set rotation values
        rotation.x += speedX * horizontalSpeed * Time.deltaTime;
        rotation.y += speedY * verticalSpeed * Time.deltaTime;
        rotation.y = Mathf.Clamp(rotation.y, -50, 90);

        //Apply rotation
        transform.rotation = Quaternion.Euler(rotation.y, rotation.x, rotation.z);
    }

    private void LockOnRotation()
    {
        if (lockOnTarget == null) { FreeCamera(); return; }
        Vector3 direction = ((lockOnTarget.position + lockOnTarget.up * lockOnHeight) - target.position).normalized;
        transform.forward = direction;
    }

    private void CalculateFinalPosition()
    {
        //Setup temporary variables
        RaycastHit hit;
        Vector3 targetPosition = target.position + target.up * height;
        Vector3 direction = (transform.position - targetPosition).normalized;
        finalDistance = maxDistance;

        //Check collision
        if (Physics.Raycast(targetPosition, direction, out hit, Mathf.Infinity)) { finalDistance = hit.distance - 0.1f; }

        //Apply position
        finalDistance = Mathf.Clamp(finalDistance, minDistance, maxDistance);
        transform.position = (target.position + target.up * height) - transform.forward * finalDistance;
    }

    #endregion

    #region Debug
    private void DoDebug()
    {
        Vector3 targetPosition = target.position + target.up * height;
        Vector3 direction = (transform.position - targetPosition).normalized;
        faceDirection = new Vector3(transform.forward.x, 0, transform.forward.z);

        Debug.DrawRay(targetPosition, direction * finalDistance, Color.green);
        Debug.DrawRay(targetPosition, faceDirection, Color.blue);
    }

    private void DrawLockOnField()
    {
        if (lockOnRayStartPosition == null)
            return;

        Vector3 leftV = (Quaternion.Euler(0, lockOnAngleMin, 0) * faceDirection).normalized;
        Vector3 rightV = (Quaternion.Euler(0, lockOnAngleMax, 0) * faceDirection).normalized;
        Debug.DrawRay(lockOnRayStartPosition.position, leftV * lockOnRange, Color.yellow);
        Debug.DrawRay(lockOnRayStartPosition.position, rightV * lockOnRange, Color.yellow);
    }
    #endregion
}
