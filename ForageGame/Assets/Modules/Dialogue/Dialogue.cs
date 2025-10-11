using Assets.Modules.Dialogue;
using TMPro;
using UnityEngine;

public class Dialogue : MonoBehaviour
{
    [SerializeField]string[] messages = {
        "Hello, adventurer!",
        "Welcome to our village.",
        "Feel free to explore and talk to the locals.",
        "Good luck on your journey!"
    };

    int index = 0;

    [SerializeField] DialogueBox dialogueBox;

    [ContextMenu("Start Dialogue")]
    public void StartDialogue()
    {
        dialogueBox.AnimateIn();
        dialogueBox.SetText(messages[index]);
    }

    [ContextMenu("Next Message")]
    public void Next()
    {
        index++;
        if (index >= messages.Length)
        {
            dialogueBox.AnimateOut();
            index = 0;
            return;
        }
        dialogueBox.SetText(messages[index]);
        dialogueBox.AnimateNewMessage();
    }
}
