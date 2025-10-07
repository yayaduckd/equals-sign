using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "List Of Tmp Sprite Assets", menuName = "DynamicInputSprites/List Of Tmp Sprite Assets")]
public class ListOfTmpSpriteAssets : ScriptableObject
{
    [System.Serializable]
    public class SpriteAssetWrapper
    {   
        public TMP_SpriteAsset spriteAsset;
        public string inputControlScheme;
    }

    public List<SpriteAssetWrapper> SpriteAssets;
}
