using System;
using UnityEngine;

namespace Environment
{
    namespace NaturalEffects
    {
        [RequireComponent(typeof(Rigidbody))]
        public class WindTarget : MonoBehaviour
        {
            #region Fields
            //Main Properties
            [Header("Properties")]
            [Tooltip("Represents the type of the Wind Target. Set it to STATIC, if you want your object to be continously effected by the wind. Set it to DYNAMIC, if your object will move (for example: your player) or you don't want it to be effected by the wind over time. Wind Agent required if you use DYNAMIC Target.")]
            public TargetType targetType;

            [Tooltip("Defines how the wind pushes the target.\n\nWIND SPEED: Pushes the plane by the actual wind speed, independently from the angle between its forward vector and the wind.\n\nSIDE FORCE: Pushes the target with the force, that is calculated from the angle between the forward vector and the wind.")]
            public MovingMethods moveBy;

            [Tooltip("Defines on which axis the target should be rotated towards the wind.")]
            public Vector3 rotationAxis = Vector3.zero;

            //Wind Effect Properties
            [Header("Wind Effect Properties")]
            [Tooltip("Enables for the wind to push the target object forward (to the wind direction).")]
            public bool enableMove = true;

            [Tooltip("Enables for the wind to rotate the target object by adding some torque to it. It rotates the target until it is facing to the direction of the wind.")]
            public bool enableRotate = true;

            //Components
            protected Rigidbody rb;

            //Output
            [Header("Output")]
            [Tooltip("Defines the method of the Side Force calculation. SF is a force that is calculated based on the angle between the target and the wind direcion (target.transform.forward, wind.direction). NOTE, that this is not the force, that is applied to the target. This force is used for rotating towards the wind direction, if it is enabled. Of yourse, you can use it as you want.\n\nPERPENDICULAR: The closer the angle to 90°, the bigger the force. (Example for use: Airplane is rotating towards the wind by this Side Force. If the plane is perpendicular to the wind, the wind will rotate it faster.\n\nPARALLEL: The more parallel the two vectors are, the bigger the force. (Example for use: Sail of the ship (its forward vector) is parallel to the wind. The ship will go faster.)")]
            public SideForceType sideForceCalculationMethod;

            [Tooltip("Stores the angle in degrees between the target and the wind direction. In the case of Perpendicular SF calculation, it goes from -180° to 180°. In the case of Parallel SF calculation, it goes from 0° to 180°.")]
            public float angleBetweenTargetAndWind = 0f;

            [Tooltip("Stores a value that is calculated from the angle between the target and the wind direction.\nThis value can be used as you wish, for example to controll a ship if the sail is perfectly parallel to the wind.")]
            public float sideForcePercentage = 0f;

            [Tooltip("The Side Force itself. Its calculation is based on the method you chose, the maximum wind speed and the angle between the target and the wind.")]
            public float sideForce = 0f;

            //References
            [Header("References")]
            public WindController windController;

            [Tooltip("The SF is calculated from the angle between this GameObject (forward vector) and the wind direction.\n\nFor example: If you are controlling a ship with the wind, this GameObject should be the sail of the ship. The angle between the sail and the wind will define the force that will be applied to the target.\n\nAnother example: If you are flying an airplane, this GameObject should be the same as the GameObject of this script (the airplane), because the force will be calculated from the angle between the plane and the wind.")]
            public Transform referenceForAngleCalculation;
            #endregion

            #region Functions
            private void Start()
            {
                rb = GetComponent<Rigidbody>();
            }

            private void FixedUpdate()
            {
                CheckType();
            }

            private void CheckType()
            {
                if (targetType == TargetType.Static && windController.enableWind) { HandleWindEffects(); }
            }

            public void HandleWindEffects()
            {
                HandleSideForceMethod();
                if (enableMove) { HandleMoveTypes(); }
                if (enableRotate) { RotateByForce(); }
            }

            private void HandleSideForceMethod()
            {
                switch (sideForceCalculationMethod)
                {
                    case SideForceType.Perpendicular:
                        CalculatePerpendicular();
                        break;

                    case SideForceType.Parallel:
                        CalculateParallel();
                        break;
                }
            }

            private void CalculatePerpendicular()
            {
                //Getting the angle between the wind direction and transform.forward
                float angle = Vector3.SignedAngle(referenceForAngleCalculation.forward, windController.direction, Vector3.up);
                float rad = angle * Mathf.Deg2Rad;

                //Converting the angle to a value between -1 and 1 by getting its SIN
                //Defines the windspeed in percentage and the direction of the rotation
                float percentage = Mathf.Sin(rad);

                //Setting the values
                angleBetweenTargetAndWind = angle;
                sideForcePercentage = percentage;
                sideForce = windController.windSpeed * percentage;
            }

            private void CalculateParallel()
            {
                //Getting the angle between the wind direction and transform.forward
                float angle = Vector3.Angle(referenceForAngleCalculation.forward, windController.direction);
                float rad = angle * Mathf.Deg2Rad;

                //Converting the angle to a value between -1 and 1 by getting its SIN
                //Defines the windspeed in percentage and the direction of the rotation
                float percentage = Mathf.Cos(rad);

                //Setting the values
                angleBetweenTargetAndWind = angle;
                sideForcePercentage = percentage;
                sideForce = windController.windSpeed * percentage;
            }

            private void HandleMoveTypes()
            {
                switch (moveBy)
                {
                    case MovingMethods.WindSpeed:
                        MoveByWindSpeed();
                        break;

                    case MovingMethods.SideForce:
                        MoveBySideForce();
                        break;
                }
            }

            private void MoveByWindSpeed()
            {
                rb.AddForce(windController.direction * windController.windSpeed);
            }

            private void MoveBySideForce()
            {
                rb.AddForce(windController.direction * sideForce);
            }

            private void RotateByForce()
            {
                rb.AddTorque(rotationAxis * sideForce);
            }
            #endregion
        }

        public enum TargetType
        {
            Static,
            Dynamic
        }

        public enum SideForceType
        {
            Perpendicular,
            Parallel
        }

        public enum MovingMethods
        {
            WindSpeed,
            SideForce
        }
    }
}