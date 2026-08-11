using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace NPC
{
    public static class NPCGuidGenerator
    {
        [MenuItem("Tools/NPCs/Generate All GUIDs")]
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
                NpcController[] controllers = root.GetComponentsInChildren<NpcController>(true);
                ReadableController[] readables = root.GetComponentsInChildren<ReadableController>(true);


                foreach (NpcController controller in controllers)
                {
                    controller.GenerateGuid();
                    EditorUtility.SetDirty(controller);
                    count++;
                }
                foreach (ReadableController readable in readables)
                {
                    readable.GenerateGuid();
                    EditorUtility.SetDirty(readable);
                    count++;
                }
            }

            EditorSceneManager.MarkSceneDirty(scene);

            Debug.Log(
                $"Generated GUIDs for {count} NPC(s) in scene '{scene.name}'."
            );
        }
    }
}