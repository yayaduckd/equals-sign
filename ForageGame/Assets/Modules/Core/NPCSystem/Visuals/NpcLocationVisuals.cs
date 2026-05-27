using UnityEngine;

namespace NPC
{
    /// <summary>
    /// This is an incredibly awkward thing that I have to put in because Readable NPCs will have 3d models, not sprites
    /// thus the visuals must be split... somehow
    /// 
    /// ~Lars
    /// </summary>
    public abstract class NpcLocationVisuals: MonoBehaviour
    {
        /// <summary>
        /// This is left unused for Readable NPCs
        /// Since they do not have emotions
        /// </summary>
        public abstract void SetEmotion(string emotion);

        /// <summary>
        /// This is left unused for Readable NPCs
        /// or maybe not? who knows
        /// </summary>
        public abstract void OnInteract();

        /// <summary>
        /// These two only for characters
        /// </summary>
        public abstract void OnPopUp();
        public abstract void OnShrinkAway();

        public abstract void FaceTowardPlayer();

        public abstract void FaceAwayFromPlayer();
    }
}
