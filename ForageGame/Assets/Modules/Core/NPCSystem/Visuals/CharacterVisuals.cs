using UnityEngine;
using UnityEngine.U2D.Animation;

namespace NPC
{
    [RequireComponent(typeof(SpriteResolver))]
    public class CharacterVisuals : NpcLocationVisuals
    {
        [SerializeField] private Animator animator;
        [SerializeField] private SpriteResolver spriteResolver;

        /// <summary>
        /// This is left unused for Readable NPCs
        /// </summary>
        public override void SetEmotion(string emotion)
        {
            if (!spriteResolver.SetCategoryAndLabel("Emotions", emotion)) 
            {
                Debug.LogError($"[NpcLocation: {transform.parent.gameObject.name}] emotion not present in SpriteLibrary: {emotion}");
            }
        }

        /// <summary>
        /// This is left unused for Readable NPCs
        /// </summary>
        public override void OnInteract()
        {
            animator.Play("InteractBounce");
        }
    }
}