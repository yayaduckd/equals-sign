using UnityEngine;

namespace Modules.Outlines
{
    [System.Serializable]
    public class OutlineInfo
    {
        public float outlineWidth = 10f;
        public Color outlineColor = Color.white;
        public bool doWobble = false;
        public float wobbleScale = 0.2f;
        public float wobbleSpeed = 1f;
        public float wobblePersistence = 0.5f;
        public float wobbleLacunarity = 2f;
        public int wobbleOctaves = 5;
        [Range(0, 1)]public float wobbleMaxIndentFactor = 0.5f;
    }
}