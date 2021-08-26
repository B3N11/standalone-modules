using System;
using UnityEngine;

namespace Environment
{
    namespace NaturalEffects
    {
        public class WindAgent : MonoBehaviour
        {
            #region Fields
            //Properties
            [Header("Properties")]
            [Tooltip("Represents the distance from the dynamic target. It shouldn't be given a too high or a too low value.")]
            public float distanceFromTarget = 10f;

            //References
            [Header("References")]
            public Transform windController;
            public GameObject target;
            private Transform targetTransform;
            private WindTarget windTarget;

            //Effects
            [Header("Effects, particles")]
            public GameObject windParticle;

            //Editor specials
            [Header("Editor Specials")]
            [Tooltip("Draws a ray that shows the line of the airflow until it hits something.")]
            public bool showRay = true;

            [Tooltip("Debugs the name of the hit objects to the console. It will write the name of the parent of the hit collider.")]
            public bool debugHitObject = true;
            #endregion

            #region Functions
            private void Start()
            {
                //Set references
                targetTransform = target.transform;
                windTarget = target.GetComponent<WindTarget>();
            }

            private void FixedUpdate()
            {
                CheckWind();
                UpdateInformations();
            }

            private void CheckWind()
            {
                if (windController.GetComponent<WindController>().enableWind) { ShootRayCast(); }
                else { windParticle.SetActive(false); }
            }

            private void UpdateInformations()
            {
                transform.rotation = windController.rotation;
                transform.position = targetTransform.position - transform.forward * distanceFromTarget;
            }

            private void ShootRayCast()
            {
                RaycastHit hit;
                if (Physics.Raycast(transform.position, transform.forward, out hit, Mathf.Infinity))
                {
                    //Apply effect if hit
                    try
                    {
                        if (hit.collider.gameObject.transform.parent.transform.parent.gameObject.name == targetTransform.gameObject.name)
                        {
                            windTarget.HandleWindEffects();
                            if (windParticle) { windParticle.SetActive(true); }
                        }
                        else { if (windParticle) { windParticle.SetActive(false); } }
                    }
                    catch (NullReferenceException) { }

                    //Display informations if it is allowed
                    if (showRay) { Debug.DrawRay(transform.position, transform.forward * hit.distance, Color.yellow); }
                    if (debugHitObject)
                    {
                        try { Debug.Log(hit.collider.gameObject.transform.parent.gameObject.name); }
                        catch (NullReferenceException) { }
                    }
                }
            }
            #endregion  
        }
    }
}