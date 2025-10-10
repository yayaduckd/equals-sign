using UnityEngine;
using TMPro;

public class TypewriterTextbox : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI textbox;
    [SerializeField] string message;
    [SerializeField] float typeDelay = 0.05f;
    [SerializeField] float startDelay = 1f;
    [SerializeField] float clickSpeedMultiplication = 3f;
    [Tooltip("If  true, displays an underscore after the text to show that it's being typed")][SerializeField] bool underscore;
    [SerializeField] AudioSource typingSound;

    private void Start()
    {
        TypeText();
    }

    public async void TypeText()
    {
        textbox.text = "";
        string text = "";

        for (int i = 0; i < message.Length; ++i)
        {
            text += message[i];
            string append = (underscore && i<message.Length-1) ? "_" : "";
            textbox.text = text + append;

            if (typingSound != null)
            {
                typingSound.Play();
            }

            float delay = Input.anyKey ? typeDelay / clickSpeedMultiplication : typeDelay;
            await System.Threading.Tasks.Task.Delay((int)(delay * 1000));
        }
    }

}
