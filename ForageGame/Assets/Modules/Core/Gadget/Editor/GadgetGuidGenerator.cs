using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace TDK.Gadgets
{
    public static class GadgetGuidGenerator
    {
        [MenuItem("Tools/Gadgets/Generate All GUIDs")]
        public static void GenerateAllGuids()
        {
            Scene scene = SceneManager.GetActiveScene();

            if (!scene.isLoaded)
            {
                Debug.LogWarning("No active scene is loaded.");
                return;
            }

            int count = 0;

            foreach (GameObject root in scene.GetRootGameObjects())
            {
                Gadget[] gadgets = root.GetComponentsInChildren<Gadget>(true);

                foreach (Gadget gadget in gadgets)
                {
                    gadget.GenerateGuid();
                    EditorUtility.SetDirty(gadget);
                    count++;
                }
            }

            EditorSceneManager.MarkSceneDirty(scene);

            Debug.Log(
                $"Generated GUIDs for {count} Gadget(s) in scene '{scene.name}'."
            );
        }
    }
}