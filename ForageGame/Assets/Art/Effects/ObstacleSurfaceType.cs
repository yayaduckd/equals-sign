using UnityEngine;

/// <summary>
/// This JUST holds a reference to a Surface Type for footstep audio purposes.
/// The Detector defaults to Wood, so only non-wood obstacles need this script
/// ~Lars
/// </summary>
public class ObstacleSurfaceType : MonoBehaviour
{
    [SerializeField] private SurfaceType surfaceType = SurfaceType.Wood;
    public SurfaceType SurfaceType => surfaceType;
}
