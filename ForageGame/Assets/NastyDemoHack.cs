using UnityEngine;
using AudioIntegration;

public class NastyDemoHack : MonoBehaviour
{

    [SerializeField] private Region startRegion;
    [SerializeField] private string startParam;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Update()
    {
        AmbienceManager.Instance.StartEvent(startRegion);
        AmbienceManager.Instance.SetParameter(startParam, 1.0f);
        Destroy(gameObject); //Hell yeah, need this to happen in Start but Unity does things in the wrong order :(
    }
}
