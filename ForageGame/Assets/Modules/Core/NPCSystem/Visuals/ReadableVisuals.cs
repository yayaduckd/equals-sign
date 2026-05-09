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

        public override void FaceTowardPlayer()
        {
            Debug.LogError($"[NpcLocation: {transform.parent.gameObject.name}] Readable NPC does not 'face towards player'");
        }

        public override void FaceAwayFromPlayer()
        {
            Debug.LogError($"[NpcLocation: {transform.parent.gameObject.name}] Readable NPC does not 'face away from player'");
        }
    }
}
