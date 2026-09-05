using UnityEngine;
using TMPro;
using System.Threading.Tasks;

public class TypewriterTextbox : MonoBehaviour
{
    public TextMeshProUGUI textbox;
    private string _message;
    [SerializeField] float typeDelay = 0.05f;
    [SerializeField] float clickSpeedMultiplication = 3f;
    [Tooltip("If  true, displays an underscore after the text to show that it's being typed")][SerializeField] bool underscore;
    [SerializeField] AudioSource typingSound;
    [SerializeField] bool playOnStart = true;

    private const string clearText = "<alpha=#00>";

    public void SetMessage(string message)
    {
        _message = message;
    }

    private void Start()
    {
        _message = textbox.text;
        textbox.text = "";
        if (playOnStart) _ = TypeText();
    }

    [ContextMenu("TypeText")]
    public async Task TypeText()
    {
        textbox.text = "";
        var newText = new System.Text.StringBuilder();

        for (int i = 1; i < _message.Length + 1; ++i)
        {
            newText.Clear();
            newText.Append(_message.Substring(0, i));
            if (underscore && i < _message.Length) newText.Append("_");
            newText.Append(clearText);
            newText.Append(_message.Substring(i));

            textbox.text = newText.ToString();

            if (typingSound != null)
            {
                typingSound.Play();
            }

            float delay = Input.anyKey ? typeDelay / clickSpeedMultiplication : typeDelay;
            await System.Threading.Tasks.Task.Delay((int)(delay * 1000));
        }
    }

}
