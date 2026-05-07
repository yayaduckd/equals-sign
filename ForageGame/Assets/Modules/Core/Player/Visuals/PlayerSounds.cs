using UnityEngine;

public class PlayerSounds : MonoBehaviour
{
    [SerializeField] private TerrainTextureDetector terrainTextureDetector;
    [SerializeField] private FMODUnity.EventReference footstepEvent;

    [SerializeField] private FMODUnity.EventReference quackEvent;

    [SerializeField] private FMODUnity.EventReference waterEnterEvent;

    [SerializeField] private FMODUnity.EventReference waterLeaveEvent;

    [SerializeField] private FMODUnity.EventReference waterSplashEvent;

    [SerializeField] private FMODUnity.EventReference swimEvent;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void OnFootstep()
    {
        var instance = FMODUnity.RuntimeManager.CreateInstance(footstepEvent);
        instance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform.position));
        instance.setParameterByName("TerrainType", (float)terrainTextureDetector.GetTerrainType()); //yes IK this sucks, I can't pass a label for a labeled parameter, FMOD is great :)
        instance.start();
        instance.release();
    }

    public void OnSwimStroke() => FMODUnity.RuntimeManager.PlayOneShot(swimEvent, transform.position);
}
