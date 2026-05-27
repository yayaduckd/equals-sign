using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TDK.ItemSystem.Inventory;
using UnityEngine;
using UnityEngine.Events;

namespace TDK.SaveSystem
{
    public class SaveManager : MonoBehaviour
    {
        [Header("Auto Saving")]
        [SerializeField] private float _autoSaveTimeSeconds = 120f;

        public string CurrentWorldId { get; private set; } = "";
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
        }

        public void SelectWorld(string worldId)
        {
            CurrentWorldId = worldId;
            CurrentWorldSaveData = SaveServices.GetWorld(CurrentWorldId);
            PlayerPrefs.SetString("lastWorldUsed", CurrentWorldId);
        }

        public void SaveWorld(Action callback = null)
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

            callback?.Invoke();
        }

        public void LoadWorld(Action callback = null)
        {
            CurrentWorldSaveData = SaveServices.GetWorld(CurrentWorldId);
            PlayerPrefs.SetString("lastWorldUsed", CurrentWorldId);
            List<ILoadable> loadables = FindAllLoadables();
            foreach (ILoadable loadable in loadables)
                loadable.LoadData(CurrentWorldSaveData);

            callback?.Invoke();
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
                SaveWorld();
            }
        }
    }
}