using UnityEngine;
using UnityEngine.U2D.Animation;

namespace NPC
{
    [RequireComponent(typeof(SpriteResolver))]
    public class CharacterVisuals : NpcLocationVisuals
    {
        [SerializeField] private Animator animator;
        [SerializeField] private SpriteResolver spriteResolver;
        [SerializeField] private SpriteRenderer spriteRenderer;

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
            Debug.Log("Bruh");
            animator.Play("InteractBounce");
        } 
        public override void OnPopUp()
        {
            animator.Play("Pop-Up");
        }
        public override void OnShrinkAway()
        {
            animator.Play("Shrink Away");
        }

        //super sucks to do hehe but I don't want to do animation detection
        public void ShrinkAwayFinished()
        {
            transform.parent.gameObject.SetActive(false);
        }

        public override void FaceTowardPlayer()
        {
            Debug.Log($"facing towards player, own x value: {transform.position.x}, player x value: {GameObject.FindWithTag("Player").transform.position.x}. Thus, flipping: {transform.position.x > GameObject.FindWithTag("Player").transform.position.x} ");
            spriteRenderer.flipX = transform.position.x > GameObject.FindWithTag("Player").transform.position.x;
        }

        public override void FaceAwayFromPlayer()
        {
            Debug.Log($"facing away from player, own x value: {transform.position.x}, player x value: {GameObject.FindWithTag("Player").transform.position.x}. Thus, flipping: {!(transform.position.x > GameObject.FindWithTag("Player").transform.position.x)} ");
            spriteRenderer.flipX = !(transform.position.x > GameObject.FindWithTag("Player").transform.position.x);
        }
    }
}