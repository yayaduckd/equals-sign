using System.IO;
using UnityEngine;
using DG.Tweening;
using System.Reflection.Emit;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System;
// using UnityEditor.Overlays;

namespace TDK.SaveSystem
{
    public static class SaveServices
    {
        private static string _dirPath = Path.Combine(Application.persistentDataPath, "WorldSaveData");
        private const string _fileExtension = ".json";
        private const string _backupExtension = ".bak";

        public static void DeleteWorld(string worldId)
        {
            if (worldId == null) return;
            Delete(worldId);
        }

        public static void CreateWorld(string worldId)
        {
            if (worldId == null) return;
            WorldSaveData worldData = new();
            Write(worldData, worldId);
        }

        public static void SetWorld(string worldId, WorldSaveData worldData)
        {
            if (worldId == null) return;
            if (worldData == null) return;
            Write(worldData, worldId);
        }

        public static WorldSaveData GetWorld(string worldId)
        {
            if (worldId == null) return new();
            WorldSaveData worldData = Read(worldId);
            worldData ??= new();
            return worldData;
        }

        public static bool ExistsWorld(string worldId)
        {
            if (worldId == null) return false;
            return File.Exists(GetFilePath(worldId));
        }

        public static string GetFreeWorldId(IEnumerable<string> worldIds)
        {
            foreach (string worldId in worldIds)
            {
                if (!ExistsWorld(worldId))
                    return worldId;
            }
            return null;
        }

        #region Private File Helpers

        private static string GetFilePath(string worldId, bool isBackup = false)
        {
            if (!Directory.Exists(_dirPath))
                Directory.CreateDirectory(_dirPath);
            string path = Path.Combine(_dirPath, worldId);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            if (isBackup) return Path.Combine(path, "SaveData" + _backupExtension);
            else return Path.Combine(path, "SaveData" + _fileExtension);
        }

        private static WorldSaveData Read(string worldId, bool allowRestoreFromBackup = true)
        {
            if (worldId == null) return null;

            string filePath = GetFilePath(worldId);
            WorldSaveData readData = null;
            if (File.Exists(filePath))
            {
                try
                {
                    // load the serialized data from the file
                    string dataToLoad = "";
                    using (FileStream stream = new(filePath, FileMode.Open))
                    using (StreamReader reader = new(stream))
                        dataToLoad = reader.ReadToEnd();

                    // deserialize the data from Json back into the C# object
                    readData = JsonUtility.FromJson<WorldSaveData>(dataToLoad);
                }
                catch (Exception e)
                {
                    // since we're calling Load(..) recursively, we need to account for the case where
                    // the rollback succeeds, but data is still failing to load for some other reason,
                    // which without this check may cause an infinite recursion loop.
                    if (allowRestoreFromBackup)
                    {
                        Debug.LogWarning($"SAVE: Failed to load data file. Attempting to roll back. \n {e}");
                        bool rollbackSuccess = AttemptRollback(worldId);
                        if (rollbackSuccess)
                            readData = Read(worldId, false); // try to load again recursively
                    }
                    // if we hit this else block, one possibility is that the backup file is also corrupt
                    else
                        Debug.LogError($"SAVE: Failed to load file at path {filePath} and backup did not work.\n {e}");
                }
            }
            return readData;
        }

        private static void Write(WorldSaveData data, string worldId)
        {
            if (worldId == null) return;

            string filePath = GetFilePath(worldId);
            try
            {
                // create the directory the file will be written to if it doesn't already exist
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                // serialize the C# game data object into Json
                string dataToStore = JsonUtility.ToJson(data, true);

                // write the serialized data to the file
                using (FileStream stream = new(filePath, FileMode.Create))
                using (StreamWriter writer = new(stream))
                    writer.Write(dataToStore);

                // verify the newly saved file can be loaded successfully
                WorldSaveData verifiedGameData = Read(worldId);
                // if the data can be verified, back it up
                if (verifiedGameData != null)
                    File.Copy(filePath, GetFilePath(worldId, isBackup: true), true);

                // otherwise, something went wrong and we should throw an exception
                else
                    throw new Exception("File could not be verified and backup could not be created.");

            }
            catch (Exception e)
            {
                Debug.LogError($"SAVE: Failed to save data to file: {filePath} \n {e}");
            }
        }

        private static void Delete(string worldId)
        {
            // base case - if the profileId is null, return right away
            if (worldId == null) return;

            string filePath = GetFilePath(worldId);
            try
            {
                // ensure the data file exists at this path before deleting the directory
                if (File.Exists(filePath))
                {
                    // delete the profile folder and everything within it
                    Directory.Delete(Path.GetDirectoryName(filePath), true);
                }
                else
                    Debug.LogWarning("Tried to delete profile data, but data was not found at path: " + filePath);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to delete profile data for profileId: {worldId} at path: {filePath} \n {e}");
            }
        }

        private static bool AttemptRollback(string worldId)
        {
            bool success = false;
            string filePath = GetFilePath(worldId, isBackup: false);
            string backupFilePath = GetFilePath(worldId, isBackup: true);
            try
            {
                // if the file exists, attempt to roll back to it by overwriting the original file
                if (File.Exists(backupFilePath))
                {
                    File.Copy(backupFilePath, filePath, true);
                    success = true;
                    Debug.LogWarning($"SAVE: Had to roll back to backup file at {backupFilePath}");
                }
                // otherwise, we don't yet have a backup file - so there's nothing to roll back to
                else
                    throw new Exception($"SAVE: Tried to roll back, but no backup file exists to roll back to.");
            }
            catch (Exception e)
            {
                Debug.LogError($"SAVE: Failed roll back to backup file at {backupFilePath} \n {e}");
            }
            return success;
        }

        #endregion
    }
}