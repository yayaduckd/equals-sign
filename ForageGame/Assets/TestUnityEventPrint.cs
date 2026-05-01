using UnityEngine;

[CreateAssetMenu(fileName = "TestUnityEventPrint", menuName = "TestUnityEventPrint")]
public class TestUnityEventPrint : ScriptableObject
{
    public static void Print(string msg)
    {
        Debug.Log(msg);
    }
}
