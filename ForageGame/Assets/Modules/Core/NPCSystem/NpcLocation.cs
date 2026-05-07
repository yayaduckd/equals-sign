using UnityEngine;
using Assets.Modules.Interaction;
using UnityEngine.Events;
using System;
using System.Threading;
using System.Threading.Tasks;
using TDK.ItemSystem;
using TDK.ItemSystem.Inventory;
using UnityEngine.U2D.Animation;

namespace NPC
{
    public class NpcLocation : MonoBehaviour, IInteractable
    {
        //InteractablePrompt PopupPrompt; // UI element to prompt the player to interact

        [Header("Interaction Callbacks")]
        public UnityEvent onInteract; // Event to invoke when interacting
        public UnityEvent onFocus;
        public UnityEvent OnUnfocus;

        [Header("References")]
        [SerializeField] private DialogueBox dialogueBox;

        [SerializeField] private NpcController npcController;

        [SerializeField] private Character character;
        [SerializeField] private Animator animator;
        [SerializeField] private SpriteResolver spriteResolver;

        [Header("Dialogue Display Settings")]
        [SerializeField] private float shortMessageDuration = 2000f;

        // State Tracking
        private bool isDialogueActive = false;
        private bool isTyping = false;
        private CancellationTokenSource textCtxSource;
        private Task currentTypingTask;

        // Public getter, TODO: unused publicly?
        public bool MessageRead { get; private set; } = false;

        private void Start()
        {
            //PopupPrompt = GetComponentInChildren<InteractablePrompt>(true);
            textCtxSource = new CancellationTokenSource();
            character = npcController.character;
        }

        private void OnDestroy()
        {
            CancelCurrentToken();
        }

        #region Interaction
        /// <summary>
        /// These are mostly unused now, they just call Next() and WalkAway()
        /// </summary>

        public virtual void Interact()
        {
            // print("Interacting with " + gameObject.name);

            onInteract?.Invoke();
            //GetComponentInChildren<Renderer>().material.color = Color.cyan; //Yeah lets not
            Next();
        }

        public virtual void Focus()
        {
            // print("Focused on " + gameObject.name);
            onFocus?.Invoke();

            //PopupPrompt?.Activate();
        }

        public virtual void Unfocus()
        {
            // print("Unfocused from " + gameObject.name);

            OnUnfocus?.Invoke();
            WalkAway();

            //PopupPrompt?.Deactivate();
        }
        #endregion
        #region Dialogue

        public void SetDialogue(string[] text)
        {
            EndDialogue();
            MessageRead = false;
        }

        public void SetEmotion(string emotion)
        {
            if (!spriteResolver.SetCategoryAndLabel("Emotions", emotion)) Debug.LogError($"[NpcLocation: {gameObject.name}] emotion not present in SpriteLibrary: {emotion}");
        }

        [ContextMenu("Next Message")]
        public async void Next()
        {
            if (isTyping)
            {
                CancelCurrentToken();
                return;
            }


            if (isDialogueActive && MessageRead)
            {
                EndDialogue();
                return;
            }

            ResetToken();


            //DialogueLine line = npcController.GetNextDialogue(this);
            DialogueResult result = npcController.GetNextDialogue(this);
            MessageRead = result.CloseAfter; //TODO: if take_item, do not close the box to make the dialogue seem continuous!
            DialogueLine line = result.Line;

            if (line == null)
            {
                EndDialogue();
                return;
            }

            //Visual stuffs
            animator.Play("InteractBounce"); //Me no likey but me also no likey to make an entire state machine for this
            if (!string.IsNullOrEmpty(line.emotion)) SetEmotion(line.emotion);

            // if (line.StageID == "repeat") {
            //     MessageRead = true;
            // }

            // Dialogue Actions
            foreach (UnityEvent action in line.dialogueActions)
            {
                action.Invoke();
            }

            // 5. Open Dialogue Box if it's currently closed
            if (!isDialogueActive)
            {
                dialogueBox.OpenDialogue();
                isDialogueActive = true;
            }

            try
            {
                isTyping = true;

                string[] messageLines = line.Text.Split('\n');
                currentTypingTask = dialogueBox.SetText(messageLines, character, textCtxSource.Token);

                await currentTypingTask;
            }
            catch (OperationCanceledException)
            {
                // Task cancelled
            }
            finally
            {
                isTyping = false;
            }
        }

        public void WalkAway()
        {
            if (npcController == null) return;

            ResetToken();

            DialogueLine textToDisplay = null; //very useful assignment of null value to uninitialized local variable, this one is new to me ~Lars

            if (isDialogueActive)
            {
                // Rude: Left while box was open
                textToDisplay = npcController.GetLeaveRudeDialogue(this);
            }
            else
            {
                // Polite: Left after closing the box
                // now only actually gets a message if the regular stages are done ~Lars
                textToDisplay = npcController.GetLeavePoliteDialogue(this);
            }

            if (textToDisplay != null)
            {
                //Visual stuffs
                animator.Play("InteractBounce"); //Me no likey but me also no likey to make an entire state machine for this
                if (!string.IsNullOrEmpty(textToDisplay.emotion))
                {
                    SetEmotion(textToDisplay.emotion);
                }
                _ = ShowShortMessage(textToDisplay.Text, character);
            }
            else
            {
                CancelCurrentToken();
                EndDialogue();
            }
        }

        private async Task ShowShortMessage(string message, Character character)
        {
            try
            {
                if (!isDialogueActive)
                {
                    dialogueBox.OpenDialogue();
                    isDialogueActive = true;
                }

                isTyping = true;
                await dialogueBox.SetText(message, character, textCtxSource.Token);
                isTyping = false;

                await Task.Delay((int)shortMessageDuration, textCtxSource.Token);
            }
            catch (OperationCanceledException) { }
            finally
            {
                EndDialogue();
            }
        }

        private void EndDialogue()
        {
            dialogueBox.CloseDialogue();
            isDialogueActive = false;
            isTyping = false;

            //reset emotion after ending dialogue (i.e., close mouth)
            animator.Play("InteractBounce");
            if(!string.IsNullOrEmpty(npcController.GetBaseEmotion(this))) SetEmotion(npcController.GetBaseEmotion(this));

            CancelCurrentToken();
        }
        #endregion

        #region DialogueActions

        public void TryTakeItem(ItemTakeActionsArgs args)
        {
            Debug.Log($"[NpcLocation: {gameObject.name}] Trying to take item {args.item} from player inventory");
            if (InventoryController.Instance.TryRemoveItemAtAny(args.item))
            {
                StoryFlagManager.Instance.AddFlag(args.OnSuccess);
                MessageRead = false; //IMPORTANT: this hack is what makes it seem like dialogue is continuous in our item taking instead of closing and re-opening
            }
        }

        #endregion
        // --- Helpers ---
        #region Cancellation Tokens
        private void CancelCurrentToken()
        {
            if (textCtxSource != null && !textCtxSource.IsCancellationRequested)
            {
                textCtxSource.Cancel();
                textCtxSource.Dispose();
            }
        }

        private void ResetToken()
        {
            CancelCurrentToken();
            textCtxSource = new CancellationTokenSource();
        }
        #endregion
    }
}