using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TDK.ItemSystem.Inventory;
using UnityEngine;
using UnityEngine.Events;

namespace TDK.SaveSystem
{
    public class SaveManager : MonoBehaviour
    {
        [Header("Auto Saving")]
        [SerializeField] private bool _useAutoSave = false;
        [SerializeField] private float _autoSaveTimeSeconds = 120f;

        private string CurrentWorldId = "";
        private WorldSaveData CurrentWorldSaveData = new();

        public static SaveManager Instance { get; private set; }
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (_useAutoSave) StartCoroutine(AutoSave());
        }

        public void SelectWorld(string worldId)
        {
            CurrentWorldId = worldId;
            CurrentWorldSaveData = SaveServices.GetWorld(CurrentWorldId);
            PlayerPrefs.SetString("lastWorldUsed", CurrentWorldId);
        }

        public void SaveWorld()
        {
            if (CurrentWorldSaveData == null)
            {
                Debug.LogWarning("No data was found. A New Game needs to be started before data can be saved.");
                return;
            }

            List<ISaveable> saveables = FindAllSaveables();
            foreach (ISaveable saveable in saveables)
                saveable.SaveData(ref CurrentWorldSaveData);

            SaveServices.SetWorld(CurrentWorldId, CurrentWorldSaveData);
        }

        public async Task SaveWorldAsync() => SaveWorld();

        public void LoadWorld()
        {
            CurrentWorldSaveData = SaveServices.GetWorld(CurrentWorldId);
            PlayerPrefs.SetString("lastWorldUsed", CurrentWorldId);
            List<ILoadable> loadables = FindAllLoadables();
            foreach (ILoadable loadable in loadables)
                loadable.LoadData(CurrentWorldSaveData);

            if (CurrentWorldSaveData.playtimeSeconds > 1)
            {
                IEnumerable<DestroyOnWorldReload> destroyItems = FindObjectsByType<DestroyOnWorldReload>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                foreach (DestroyOnWorldReload destroyItem in destroyItems)
                    Destroy(destroyItem.gameObject);
            }
        }

        void OnApplicationQuit()
        {
            SaveWorld();
        }

        private List<ISaveable> FindAllSaveables()
        {
            IEnumerable<ISaveable> saveables = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None).OfType<ISaveable>();
            return new List<ISaveable>(saveables);
        }

        private List<ILoadable> FindAllLoadables()
        {
            IEnumerable<ILoadable> loadables = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None).OfType<ILoadable>();
            return new List<ILoadable>(loadables);
        }

        private IEnumerator AutoSave()
        {
            while (true)
            {
                yield return new WaitForSeconds(_autoSaveTimeSeconds);
                _ = SaveWorldAsync();
            }
        }
    }
}