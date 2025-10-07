using TMPro;
using UnityEngine;

public class InteractablePrompt : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI promptText;
    [SerializeField] Collider col; // Used to position the prompt above the object
    [SerializeField] Vector3 offset = new Vector3(0, 0, 0);
    [SerializeField] bool initializeOnStart = true;
    [SerializeField] string text;
    [SerializeField] bool faceCamera = true;
    bool initialized = false;

    private void Awake()
    {
        gameObject.SetActive(false);
        if (initializeOnStart) { Initialize(); }
    }

    private void Update()
    {
        if(faceCamera) 
        { 
            transform.forward = -(Camera.main.transform.position - transform.position); // Look towards the camera
            transform.rotation = Quaternion.Euler(new Vector3(transform.rotation.eulerAngles.x, Camera.main.transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z));
        }

    }

    public void Activate()
    {
        if(!initialized) { Initialize(); }
        gameObject.SetActive(true);
    }

    public void Deactivate()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Sets up the prompt. Position is updated to be above the supplied collider (+offset), or at the offset if no collider is given.
    /// </summary>
    [ContextMenu("Initialize Prompt")]
    private void Initialize()
    {
        promptText.text = text;

        float halfTextHeight = promptText.GetPreferredValues().y * promptText.transform.lossyScale.y / 2; 

        Ray ray = new Ray(transform.position + Vector3.up * 10f * col.transform.lossyScale.y, Vector3.down);
        RaycastHit hit;

        if(col != null && col.Raycast(ray, out hit, 20f * col.transform.lossyScale.y))
        {
            transform.position = hit.point + Vector3.up * halfTextHeight + offset;
        }
        else
        {
            transform.localPosition = Vector3.up * halfTextHeight + offset;
        }

        gameObject.SetActive(true);
        initialized = true;
    }
}
