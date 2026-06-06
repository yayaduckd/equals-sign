using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace Assets.Modules.Dialogue.Typewriter_effect
{
    public static class TypewriterEffect
    {
        private const string clearText = "<alpha=#00>";

        public static async Task TypewriteText(this TextMeshProUGUI textbox, string message, CancellationToken ctx, bool underscore = false, float typeDelay = 0.03f, float clickSpeedMultiplication = 2f)
        {
            textbox.text = "";
            var newText = new System.Text.StringBuilder();

            for (int i = 1; i < message.Length + 1; ++i)
            {
                if (ctx.IsCancellationRequested)
                {
                    textbox.text = message;
                    return;
                }
                newText.Clear();
                newText.Append(message.Substring(0, i));
                if(underscore && i < message.Length) newText.Append("_");
                newText.Append(clearText);
                newText.Append(message.Substring(i));
            
                textbox.text = newText.ToString();

                float delay = Input.anyKey ? typeDelay / clickSpeedMultiplication : typeDelay;
                await Task.Delay((int)(delay * 1000));
            }
        }
    }
}