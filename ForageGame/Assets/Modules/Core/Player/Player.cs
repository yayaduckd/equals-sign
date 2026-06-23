using TDK.ItemSystem.Inventory;
using UnityEngine;
using NPC;
using TDK.SaveSystem;

namespace TDK.PlayerSystem
{
    public enum PlayerUpgradeType { Attack, Lantern, Pouch, Wing, Sprint }

    [RequireComponent(typeof(Energy))]
    [RequireComponent(typeof(PlayerController))]
    [RequireComponent(typeof(PlayerInteract))]
    [RequireComponent(typeof(Animator))]
    public class Player : MonoBehaviour, ISaveable, ILoadable
    {
        public static Player Instance { get; private set; }

        [Header("Components")]
        public Energy energy { get; private set; }
        public PlayerInteract playerInteract { get; private set; }
        public PlayerController playerController { get; private set; }
        public Animator animator { get; private set; }

        [SerializeField] public Hitbox hitbox;
        [SerializeField] public TrailRenderer trailRenderer;
        [SerializeField] private ParticleSystem hitParticleRenderer;
        [SerializeField] public PlayerVisuals visuals;
        [SerializeField] public DialogueBox thinkingBox;

        [Header("Player Data")]
        [SerializeField] public PlayerSaveData playerData;
        [Header("Energy Requirements")]
        [SerializeField] public float runEnergy = 10f; // this is energy per second
        [SerializeField] public float dashEnergy = 10f;
        [SerializeField] public float hopEnergy = 10f;
        [SerializeField] public float flutterEnergy = 10f; // this is energy per second
        [SerializeField] public float attackEnergy = 10f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // Get components on this GameObject
            energy = GetComponent<Energy>();
            playerInteract = GetComponent<PlayerInteract>();
            playerController = GetComponent<PlayerController>();
            animator = GetComponent<Animator>();

            ExitStateReset();
        }

        private void OnValidate()
        {
            visuals.UpdateWingVisuals(playerData.wingLevel);
        }

        #region Save & Load

        public void SaveData(ref WorldSaveData data)
        {
            playerData.spawnPosition = transform.position;
            data.Player = playerData;
        }

        public void LoadData(WorldSaveData data)
        {
            playerData = data.Player;

            playerController.TeleportTo(playerData.spawnPosition, true);
            visuals.UpdateWingVisuals(playerData.wingLevel);
            ExitStateReset();
        }

        #endregion

        #region Upgrades
        public void Upgrade(PlayerUpgradeType upgradeType)
        {
            switch (upgradeType)
            {
                case PlayerUpgradeType.Attack:
                    playerData.attackUnlocked = true;
                    //TODO: spawn dummy
                    break;
                case PlayerUpgradeType.Lantern:
                    playerData.lanternUnlocked = true;
                    //TODO: activate light
                    break;
                case PlayerUpgradeType.Pouch:
                    playerData.pouchLevel += 1;
                    InventoryController.Instance.AddSlots(1);
                    break;
                case PlayerUpgradeType.Wing:
                    playerData.wingLevel += 1;
                    visuals.UpdateWingVisuals(playerData.wingLevel);
                    break;
                case PlayerUpgradeType.Sprint:
                    playerData.sprintUnlocked = true;
                    break;
            }
        }

        #endregion

        // ------------ ANIMATOR CLUNK ------------

        public void ExitStateReset()
        {
            if (hitParticleRenderer != null)
            {
                hitParticleRenderer.Stop();
                hitParticleRenderer.Clear();
            }

            hitbox.gameObject.SetActive(false);
            trailRenderer.emitting = false;

            playerController.Reset();
        }
    }
}