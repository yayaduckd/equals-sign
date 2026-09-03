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

        private void ClearTriggers()
        {
            foreach (var param in animator.parameters)
                if (param.type == AnimatorControllerParameterType.Trigger)
                    animator.ResetTrigger(param.name);
        }

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
            ClearTriggers();
            animator.SetTrigger("Interact");
        }
        public override void OnPopUp()
        {
            ClearTriggers();
            animator.SetTrigger("Pop-Up");
        }
        public override void OnShrinkAway()
        {
            ClearTriggers();
            animator.SetTrigger("Shrink Away");
        }

        public void TriggerCustomAnimation(int id, bool playIdleNext) // 1, 2, 3, 4 (4 animations supported)
        {
            ClearTriggers();
            animator.SetTrigger("Custom " + id.ToString());
            if (playIdleNext) animator.SetTrigger("Idle");
        }

        //super sucks to do hehe but I don't want to do animation detection
        public void ShrinkAwayFinished()
        {
            transform.parent.gameObject.SetActive(false);
        }

        public override void FaceTowardPlayer()
        {
            //Debug.Log($"facing towards player, own x value: {transform.position.x}, player x value: {GameObject.FindWithTag("Player").transform.position.x}. Thus, flipping: {transform.position.x > GameObject.FindWithTag("Player").transform.position.x} ");
            spriteRenderer.flipX = transform.position.x > GameObject.FindWithTag("Player").transform.position.x;
        }

        public override void FaceAwayFromPlayer()
        {
            //Debug.Log($"facing away from player, own x value: {transform.position.x}, player x value: {GameObject.FindWithTag("Player").transform.position.x}. Thus, flipping: {!(transform.position.x > GameObject.FindWithTag("Player").transform.position.x)} ");
            spriteRenderer.flipX = !(transform.position.x > GameObject.FindWithTag("Player").transform.position.x);
        }

        public override void FaceLeft()
        {
            spriteRenderer.flipX = true;
        }

        public override void FaceRight()
        {
            spriteRenderer.flipX = false;
        }
    }
}