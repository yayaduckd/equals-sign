using UnityEngine;

namespace NPC
{
    public class ReadableVisuals : NpcLocationVisuals
    {
        /// <summary>
        /// This is left unused for Readable NPCs
        /// </summary>
        public override void SetEmotion(string emotion)
        {
            Debug.LogError($"[NpcLocation: {transform.parent.gameObject.name}] Readable NPC has no emotions");
        }

        /// <summary>
        /// This is left unused for Readable NPCs
        /// </summary>
        public override void OnInteract()
        {
            //nothing
            //Debug.LogError($"[NpcLocation: {transform.parent.gameObject.name}] Readable NPC has no OnInteract behavior");
        }
        public override void OnPopUp()
        {
            //nothing
        }
        public override void OnShrinkAway()
        {
            Debug.LogError($"[NpcLocation: {transform.parent.gameObject.name}] Readable NPC should never shrink away");
            transform.parent.gameObject.SetActive(false);
        }

        public override void FaceTowardPlayer()
        {
            Debug.LogError($"[NpcLocation: {transform.parent.gameObject.name}] Readable NPC does not 'face towards player'");
        }

        public override void FaceAwayFromPlayer()
        {
            Debug.LogError($"[NpcLocation: {transform.parent.gameObject.name}] Readable NPC does not 'face away from player'");
        }
        
        public override void FaceLeft()
        {
            Debug.LogError($"[NpcLocation: {transform.parent.gameObject.name}] Readable NPC does not 'face left'");
        }
        public override void FaceRight()
        {
            Debug.LogError($"[NpcLocation: {transform.parent.gameObject.name}] Readable NPC does not 'face right'");
        }
    }
}
