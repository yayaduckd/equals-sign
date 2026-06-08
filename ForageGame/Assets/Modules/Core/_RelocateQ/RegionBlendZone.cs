using UnityEngine;

namespace AudioIntegration
{

    [RequireComponent(typeof(BoxCollider))]
    public class RegionBlendZone : MonoBehaviour
    {
        private BoxCollider col;

        [SerializeField] private Region regionA;
        
        [SerializeField] private Region regionB;

        void Start()
        {
            col = GetComponent<BoxCollider>();
            if (!col.isTrigger)
                Debug.LogWarning("Collider should be a trigger!");
        }

        void OnTriggerEnter(Collider other)
        {
            if(other.CompareTag("Player"))
            {
                //check from which side the player came from, start the appropriate event
                float relZ = transform.InverseTransformPoint(other.transform.position).z; //local position, so we can also rotate the box
                if(relZ > 0)
                {
                    //towards negative Z, A side
                    AmbienceManager.Instance.StartEvent(regionA);
                }
                else
                {
                    AmbienceManager.Instance.StartEvent(regionB);
                }
            }
        }

        void OnTriggerExit(Collider other)
        {
            if(other.CompareTag("Player"))
            {
                //check from which side the player left to, stop the appropriate event
                float relZ = transform.InverseTransformPoint(other.transform.position).z; //local position, so we can also rotate the box
                if(relZ > 0)
                {
                    //towards positive Z, B side
                    AmbienceManager.Instance.StopEvent(regionA);
                }
                else
                {
                    AmbienceManager.Instance.StopEvent(regionB);
                }
            }
        }

    }
}
