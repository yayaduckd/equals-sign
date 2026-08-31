using TDK.ItemSystem.Inventory;
using UnityEngine;

namespace TDK.PlayerSystem
{
    [System.Serializable]
    public class PlayerSaveData
    {
        public Vector3 spawnPosition = new Vector3(1.9f, 14f, 36.6f);
        public float damageAmount = 90;

        public int wingLevel = 0;
        public int pouchLevel = 0;
        public bool attackUnlocked = false;
        public bool sprintUnlocked = false;
        public bool lanternUnlocked = false;

        // The "hasUsed" referes to the fact you have used this ability ever (for the InGameHints system)
        public bool hasUsedJump = false;
        public bool hasUsedSprint = false;
        public bool hasUsedFly = false;
        public bool hasUsedAttack = false;

        public bool hasOpenedRecipeBook = false;
    }
}