using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;
using System.Collections;
using Eflatun.SceneReference;

namespace TDK.SceneSystem
{
    public static class SceneServices
    {
        public static event Action<string> OnSceneLoaded = delegate { };
        public static event Action<string> OnSceneUnloaded = delegate { };
        public static event Action OnSceneGroupLoaded = delegate { };

        public static async Task LoadScene(SceneReference scene, bool allowMultipleSceneInstances = false)
        {
            Debug.Log($"[SceneServices] Loading Scene: {scene.Name}");

            if (allowMultipleSceneInstances == false && scene.LoadedScene.isLoaded) return;

            AsyncOperation operation = SceneManager.LoadSceneAsync(scene.Path, LoadSceneMode.Additive);
            while (!operation.isDone)
                await Task.Delay(100);

            OnSceneLoaded.Invoke(scene.Name);

            Debug.Log($"[SceneServices] Loaded Scene: {scene.Name}");
        }

        public static async Task LoadScenes(List<SceneReference> scenes, bool allowMultipleSceneInstances = false)
        {
            List<SceneReference> scenesToLoad = new();

            foreach (SceneReference scene in scenes)
                if (allowMultipleSceneInstances || !scene.LoadedScene.isLoaded)
                    scenesToLoad.Add(scene);

            List<AsyncOperation> asyncOperations = new();

            foreach (SceneReference scene in scenesToLoad)
            {
                Debug.Log($"[SceneServices] Loading Scene: {scene.Name}");
                AsyncOperation operation = SceneManager.LoadSceneAsync(scene.Path, LoadSceneMode.Additive);
                operation.allowSceneActivation = false;
                asyncOperations.Add(operation);
            }

            while (asyncOperations.All(o => o.progress < 0.9f)) // yes this should be 0.9f (check docs)
                await Task.Delay(100);

            foreach (AsyncOperation asyncOperation in asyncOperations)
                asyncOperation.allowSceneActivation = true;

            foreach (SceneReference scene in scenesToLoad)
            {
                OnSceneLoaded.Invoke(scene.Name);
                Debug.Log($"[SceneServices] Loaded Scene: {scene.Name}");
            }

            OnSceneGroupLoaded.Invoke();
        }

        public static async Task UnloadScene(Scene scene)
        {
            if (!scene.isLoaded) return;
            if (scene == SceneManager.GetActiveScene()) return;
            // if (scene.name == "Bootstrapper") return;
            string sceneName = scene.name; // inaccesible after the scene unloads
            Debug.Log($"[SceneServices] Unloading Scene: {sceneName}");

            var operation = SceneManager.UnloadSceneAsync(scene);
            if (operation == null) return; // ? (originally continue)

            while (!operation.isDone)
                await Task.Delay(100); // delay to avoid tight loop

            OnSceneUnloaded.Invoke(sceneName);

            await Resources.UnloadUnusedAssets();   // Optional: UnloadUnusedAssets - unloads all unused assets from memory

            Debug.Log($"[SceneServices] Unloaded Scene: {sceneName}");
        }

        public static async Task UnloadScenes(List<Scene> scenes)
        {
            foreach (Scene scene in scenes)
                await UnloadScene(scene);

            await Resources.UnloadUnusedAssets();
        }

        public static async Task UnloadScene(SceneReference scene) => await UnloadScene(scene.LoadedScene);

        public static async Task UnloadScenes(List<SceneReference> scenes)
        {
            foreach (SceneReference scene in scenes)
                await UnloadScene(scene);
        }

        public static async Task UnloadAllScenes() => await UnloadScenes(GetLoadedScenes().ToList());

        public static Scene[] GetLoadedScenes()
        {
            int countLoaded = SceneManager.sceneCount;
            Scene[] loadedScenes = new Scene[countLoaded];
            for (int i = 0; i < countLoaded; i++)
                loadedScenes[i] = SceneManager.GetSceneAt(i);
            return loadedScenes;
        }

        public static string[] GetSceneNames(SceneReference[] scenes)
        {
            string[] names = new string[scenes.Length];
            for (int i = 0; i < scenes.Length; i++)
                names[i] = scenes[i].Name;
            return names;
        }

        public static int[] GetSceneBuildIndecies(SceneReference[] scenes)
        {
            int[] names = new int[scenes.Length];
            for (int i = 0; i < scenes.Length; i++)
                names[i] = scenes[i].BuildIndex;
            return names;
        }

        public static string[] GetSceneAddresses(SceneReference[] scenes)
        {
            string[] addresses = new string[scenes.Length];
            for (int i = 0; i < scenes.Length; i++)
                addresses[i] = scenes[i].Address;
            return addresses;
        }

        public static string[] GetScenePaths(SceneReference[] scenes)
        {
            string[] paths = new string[scenes.Length];
            for (int i = 0; i < scenes.Length; i++)
                paths[i] = scenes[i].Path;
            return paths;
        }

        public static Scene[] GetSceneLoads(SceneReference[] scenes)
        {
            Scene[] loadedScenes = new Scene[scenes.Length];
            for (int i = 0; i < scenes.Length; i++)
                loadedScenes[i] = scenes[i].LoadedScene;
            return loadedScenes;
        }

        public static string[] GetSceneGuids(SceneReference[] scenes)
        {
            string[] guids = new string[scenes.Length];
            for (int i = 0; i < scenes.Length; i++)
                guids[i] = scenes[i].Guid;
            return guids;
        }
    }
}