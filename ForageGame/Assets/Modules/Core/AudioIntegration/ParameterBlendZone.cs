using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AudioIntegration
{
    [System.Serializable] //here's me hoping this is the case lol
    public struct ParameterCurve
    {
        public string parameterName;
        public AnimationCurve curve;
    }


    [RequireComponent(typeof(BoxCollider))]
    public class ParameterBlendZone : MonoBehaviour
    {
        [Header("FMOD Target Parameters.\nA is on the relative -Z axis and should start at 1,\nB on the +Z axis and should start at 0")]

        [Tooltip("")]
        [SerializeField] private List<ParameterCurve> aParams = new List<ParameterCurve>();

        //this separation is not strictly necessary, but is easier for clarity
        [SerializeField] private List<ParameterCurve> bParams = new List<ParameterCurve>();

        private BoxCollider col;

        private float progress01 = 0f;

        [Header("Gizmo settings")]
        [SerializeField] private Color aColor;
        [SerializeField] private Color bColor;

        void Start()
        {
            col = GetComponent<BoxCollider>();
            if (!col.isTrigger)
                Debug.LogWarning("Collider should be a trigger!");
        }

        //basically an update() while there is a collision
        void OnTriggerStay(Collider other)
        {
            if(other.CompareTag("Player"))
            {
                Vector3 localPos = transform.InverseTransformPoint(other.transform.position); //local position, so we can also rotate the box

                float halfLength = col.size.z * 0.5f;
                progress01 = Mathf.InverseLerp(-halfLength, halfLength, localPos.z); //how far are we in the length

                foreach (ParameterCurve param in aParams)
                {
                    AmbienceManager.Instance.SetParameter(param.parameterName, param.curve.Evaluate(progress01));
                }

                foreach (ParameterCurve param in bParams)
                {
                    AmbienceManager.Instance.SetParameter(param.parameterName, param.curve.Evaluate(progress01));
                }
            }
        }

        //some nice gizmos cooked up by Mr. GPT ~Lars
        private void OnDrawGizmos()
        {
            if (col == null) return;
            Gizmos.color = Color.Lerp(aColor, bColor, progress01);
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(col.center, col.size);

            //Handles is editor only, for some reason
            #if UNITY_EDITOR
                // Compute ends in local space
                Vector3 halfSize = col.size * 0.5f;
                Vector3 startLocal = col.center - new Vector3(0, 0, halfSize.z);
                Vector3 endLocal = col.center + new Vector3(0, 0, halfSize.z);

                // Transform to world space
                Vector3 startWorld = transform.TransformPoint(startLocal);
                Vector3 endWorld = transform.TransformPoint(endLocal);

                string aParamString = "";
                foreach (ParameterCurve param in aParams)
                {
                    aParamString += param.parameterName  + ",\n";
                }
                string bParamString = "";
                foreach (ParameterCurve param in bParams)
                {
                    bParamString += param.parameterName + ",\n";
                }

                // Draw labels
                Handles.Label(startWorld, "A: " + aParamString);
                Handles.Label(endWorld, "B: " + bParamString);
            #endif
        }
    }
}