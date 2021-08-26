using System;
using System.Collections.Generic;
using UnityEngine;

namespace Environment
{
    namespace NaturalEffects
    {
        public class WindController : MonoBehaviour
        {
            #region System description
            /* BASIC, ARCADE PHYSICS BASED WIND CODE
             * 
             * HOW DOES IT WORK: The GameObject with this script has an Animation Curve that determines the "airflow", the speed of the wind. The
             * vertical axis of the graph represents the max value (speed) of the wind. As the position of this Speed Curve changes by the time,
             * the speed of the airflow changes too. This airflow represents a force that will be applied to the GameObjects with the Wind Target component.
             * 
             * HOW TO USE IT: Create an empty GameObject (suggested to name it something like "Wind Controller/Handler")! Add this script to the Wind Controller!
             * The position of this GameObject doesn't matter.
             * The field "direction" stands for the (surprisingly) direction of the wind. It is important, because the force will be applied from this direction. 
             * The direction of the GameObject is calculated by its rotation! Set up the Speed Curve by choosing a pre-created curve, or create your own.
             * Set the max speed so as the change speed. Then, attach the Wind Target and (optional) the Wind Agent to the right GameObjects. Choose the
             * appropriate script that you need. Here are their functions:
             * 
             * WORKING OF THE WIND TARGET: Wind Target should be added to all GameObjects you want to be affected by the wind. It has two types:
             * STATIC and DYNAMIC
             * In the case of STATIC, you don't have to do anything after adding the script and choosing its Target Type. This scripts needs a reference to the
             * Wind Controller in the scene and it will get the informations in every FixedUpdate call from it. The following variables are passed:
             * direction and windSpeed. Static Wind Target simply applies a force with the current speed of the wind (windSpeed) continuosly.
             * In the case of DYNAMIC, you will also need a different GameObject with the Wind Agent script attached to it. Only thing you must do is selecting
             * the type as DYNAMIC on the Target. This type of Target is recommended to be used on Player and other kind of moving character.
             * 
             * WORKING OF THE WIND AGENT: If you set your Target as a DYNAMIC Target, you need to create a GameObject called Wind Agent. What this object does
             * is shooting RayCasts forward and checking what it hit. It will only apply the force to the given Target, if the RayCast hits it. This means that,
             * if the Target is behind a wall or other kind of cover, the RayCast will hit the cover, the "airflow breaks" and the Target won't be affected by
             * the wind as long as it doesn't leave the cover and contacts the RayCast again. The distance between the Agent and the Target is also very important.
             * If it is too high, the Target won't be affected on more opened areas (such as cornfields). If it is too low, not many objects will fit in the space
             * between the Target and the Agent. There will be hardly any cover, that the Target can use to avoid wind. Of course, this part is all about personal
             * preference and the type of your game.
             * 
             * Desired structure of a Dynamic WindTarget:
             * 
             * Player
             *  - Colliders
             *      - Body_Collider
             * 
             * The Agent will look for the parent of the parent of the collider it hit. If it hits the Body_Collider GameObject, it will look for the parent
             * (Player) of its parent (Colliders). This can be changed in the WindAgent script in the line of 72 (void ShootRayCast() method).
             * 
             * WIND PARTICLE FEATURE: You have the opportunity to visualise the wind by attaching a "follow" script to a particle system or to its parent.
             * You can determine the distance from the player.
             * 
             * Created: 2020 october
             *          in Unity 2020.1.9f1 version
             * System version: v.1.1.0
             * 
             * Any questions or problems? Maybe want to share your opinion or your suggestion? Contact with me: ********@gmail.com
             */
            #endregion

            #region Fields
            //Properties
            [Header("Wind Properties")]
            [Tooltip("Defines the changing method of the wind speed. Horizontal axis represents the time, vertical sets the current speed in percentage (0 - 100 %).")]
            public AnimationCurve speedCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

            [NonSerialized] public Vector3 direction = Vector3.zero;    //Represents the direction where the airflow is coming from

            [Tooltip("Set it by knowing that this represents the amount of force, that will be applied to the objects affected by wind (the Targets).")]
            public float maxSpeed = 50f;

            [Tooltip("Keep it between 0 and 1. The closer the value to 1, the faster the airflow changes. It also determines the amount of steps the curve position is moving with. The less the value the more precious the output on the curve.")]
            public float changeSpeed = 0.01f;    //Speed of airflow change (KEEP IT BETWEEN 0 AND 1!!)

            [Tooltip("By setting this, you can turn on and off the wind. If you want to change it by code, you can access it like: \n\nmyWindController.enableWind = false;")]
            public bool enableWind = true;

            private float curvePosition;    //Changes over time, represents the X on the horizontal axis of the curve

            //Effects
            [Header("Visual Effects")]
            [Tooltip("Only DIRECTIONAL WindZones will be handled in this list.")]
            public List<WindZone> windZones = new List<WindZone>();

            //Output
            [Header("Output")]
            [Tooltip("Represents the current speed of the wind. It changes over time and is calculated from the Speed Curve.")]
            public float windSpeed = 0f;    //Represents the current value of the Speed Curve, the current "speed of the air"

            public float speedInMph = 0f;

            //Editor specials
            [Header("Editor Specials")]
            [Tooltip("Draws a ray to the direction of the wind with the length of the wind speed.")]
            public bool showWindSpeed = true;
            #endregion

            #region Functions
            private void FixedUpdate()
            {
                UpdateRotation();
                CalculateWindSpeed();
                UpdateOutput();
            }

            private void UpdateRotation()
            {
                direction = transform.forward;
            }

            private void CalculateWindSpeed()
            {
                //Calculating the current speed of the airflow
                windSpeed = speedCurve.Evaluate(curvePosition) * maxSpeed;

                //Moving the position of the curve and keeping it between 0 and 1. Then looping it
                curvePosition += changeSpeed;
                curvePosition = Mathf.Clamp01(curvePosition);
                if (curvePosition == 1) { curvePosition = 0; }

                //Draw the current wind speed it it is allowed
                if (showWindSpeed) { Debug.DrawRay(transform.position, transform.forward * windSpeed, Color.red); }
            }

            private void UpdateOutput()
            {

            }

            private void SetWindZones()
            {
                foreach (WindZone actual in windZones)
                {
                    if (actual.mode == WindZoneMode.Directional)
                    {
                        actual.transform.rotation = Quaternion.Euler(direction);
                    }
                }
            }
            #endregion
        }
    }
}