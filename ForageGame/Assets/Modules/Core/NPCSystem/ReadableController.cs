using System.Collections.Generic;
using System.Linq;
using TDK.ItemSystem;
using TDK.ItemSystem.Inventory;
using UnityEngine;
using UnityEngine.Events;

using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.U2D.Animation;
using TDK.PlayerSystem;
using TDK.ItemSystem.Types;
using TDK.SaveSystem;
using TDK.InteractionSystem;

namespace NPC
{
    /// <summary>
    /// A Readable is a special type of NPC that is 'attached' to something that is already interactable.
    /// Mostly used for things like signs, flavor dialogue stuff
    /// But importantly, also Grimble's wall.
    /// 
    /// A Readable is a singular component on a single location, rather than NpcController which holds mutliple NpcLocations.
    /// So its basically an NpcController and NpcLocation combined into one
    /// </summary>

    public class ReadableController : MonoBehaviour, ISaveable, ILoadable
    {

        [Header("Dialogue Data")]
        [SerializeField] public DialogueSpeakerType character;
        [SerializeField] private TextAsset _sourceFile;
        [SerializeField] private DialogueParser parser;

        [SerializeField] private ReadableDialogueDatabase _database;

        [Header("References")]
        [SerializeField] private DialogueReferences dialogueReferences;

        [Header("Current State")]
        [SerializeField] private bool isEnabled = true;
        [SerializeField] private ReadableStage _activeStage;
        [SerializeField] private int _lineIndex = 0;
        [SerializeField] private HashSet<int> _completedStageIndices = new();

        private bool isDialogueActive = false;
        private bool isTyping = false;
        private CancellationTokenSource textCtxSource;
        private Task currentTypingTask;

        [Header("Dialogue display settings")]
        [Tooltip("character count -> syllable count. Clamped between 1 and 10")]
        [SerializeField] private AnimationCurve syllableCountCurve;

        [SerializeField] private float shortMessageDuration = 2000f;

        //Public getter, TODO: unused publicly?
        public bool MessageRead { get; private set; } = false;

        [SerializeField] private Interactable _interactable;

        [SerializeField] private StoryFlag FlagToSetAfterDialogue = null;

        //Changed to Start() from Awake() since it gave inconsistent behavior in terms of timing ~Lars
        private void Start()
        {
            StoryFlagManager.onFlagAdded += OnNewStoryFlag;
            StoryFlagManager.onTimePassing += OnTimePassing;
            InventoryController.onNewItemSeen += OnNewItemSeen;
            _database = parser.ParseReadable(_sourceFile.text,
                                    StoryFlagManager.Instance.flagDatabase.AsDictionary(),
                                    dialogueReferences.GetItemDataMap(),
                                    dialogueReferences.GetDialogueActionMap());
            EvaluateActiveStage();

            //Player.Instance.thinkingBox.syllableCountCurve = syllableCountCurve;
            textCtxSource = new CancellationTokenSource();
        }

        public void DisableInteractable()
        {
            if (_interactable != null)
            {
                _interactable.SetInteractibility(false);
                Debug.Log($"[ReadableController: {transform.parent.gameObject.name}] Interactable {_interactable.name} Disabled!");
            }
        }

        public void EnableInteractable()
        {
            if (_interactable != null)
            {
                _interactable.SetInteractibility(true);
                Debug.Log($"[ReadableController: {transform.parent.gameObject.name}] Interactable {_interactable.name} Enabled!");
            }
        }

        private void OnDestroy()
        {
            CancelCurrentToken();
        }

        #region Stage Management

        private int GetActiveStageIndex() => _activeStage == null ? -1 : _database.storyStages.IndexOf(_activeStage);

        private void OnNewStoryFlag(StoryFlag flag)
        {
            if (_completedStageIndices.Contains(GetActiveStageIndex())) EvaluateActiveStage(true); //do this only if current stage is done
        }
        private void OnTimePassing()
        {
            if (_completedStageIndices.Contains(GetActiveStageIndex())) EvaluateActiveStage(true); //do this only if current stage is done
        }

        private void OnNewItemSeen(ItemData item)
        {
            if (_completedStageIndices.Contains(GetActiveStageIndex())) EvaluateActiveStage(); //do this only if current stage is done
        }
        public void OnDialogueClosed()
        {
            if (FlagToSetAfterDialogue != null)
            {
                Debug.Log($"[ReadableController: {transform.parent.gameObject.name}] Setting storyflag {FlagToSetAfterDialogue.id} after dialogue as planned");
                StoryFlagManager.Instance.AddFlag(FlagToSetAfterDialogue);
                FlagToSetAfterDialogue = null;
            }
            else if (_completedStageIndices.Contains(GetActiveStageIndex())) EvaluateActiveStage(); //do this only if current stage is done
        }
        private void EvaluateActiveStage(bool timePassed = false)
        {
            Debug.Log($"[ReadableController: {transform.parent.gameObject.name}] Re-evaluating active stage, current stage index is {GetActiveStageIndex()}");
            foreach (var i in _completedStageIndices)
            {
                Debug.Log($"[ReadableController: {transform.parent.gameObject.name}] ... accounting for completed stage index : {i}");
            }

            int startIndex = GetActiveStageIndex();

            //this gets a default value if no target is found which will be null
            var next = _database.storyStages
                .Skip(startIndex)
                .FirstOrDefault(s =>
                    !_completedStageIndices.Contains(_database.storyStages.IndexOf(s)) &&
                    StoryFlagManager.Instance.FlagListActive(s.RequiredFlags) &&
                    s.requiredItems.All(item => InventoryController.Instance.seenItems.Contains(item)) &&
                    (!s.requiresTimePassing || timePassed));

            if (next == _activeStage || next == null)
            {
                Debug.Log($"[Read: {character}] No new Active StoryStage detected");
                return; //if makes no difference nothing changes!
            }

            StartNewStoryStage(next);
        }

        private void StartNewStoryStage(ReadableStage stage)
        {
            _activeStage = stage;
            Debug.Log($"[ReadableController: {transform.parent.gameObject.name}] New active StoryStage set with index {GetActiveStageIndex()}");
            if (_activeStage == null)
            {
                Debug.LogError($"[ReadableController: {transform.parent.gameObject.name}] No active StoryStage");
                return;
            }

            //update location indices
            _lineIndex = 0;

            //check if the new stage has a location assigned
            //It should just be a single one
            //IMPORTANT: if it has none, the readable will disable itself (use for tutorial stuff)
            var ld = _activeStage.locationDialogue;
            if (ld == null || ld.StandardLines.Count == 0)
            {
                Debug.LogWarning($"[ReadableController: {transform.parent.gameObject.name}] Active StoryStage has no locationDialogue, Readable will be disabled!");
                isEnabled = false;
                _completedStageIndices.Add(GetActiveStageIndex());
                DisableInteractable();

                // if(InteractableObj != null)
                // {
                //     Debug.LogWarning($"[ReadableController]: 'diableQueued' is set: {InteractableObj} will be disabled");
                //     InteractableObj.DisableOutline();
                //     InteractableObj.enabled = false;
                //     // if(InteractableObj.TryGetComponent<OutlineObject>(out var outline)) Destroy(outline); //also disable outline if there is one, to prevent lingering outlines after disabling interactable
                //     //InteractableObj = null;
                // }
            }
            else
            {
                //re-enable if was disabled by previous empty stage, to allow for auto-re-enabling
                EnableInteractable();
                // if(InteractableObj != null)
                // {
                //     Debug.LogWarning($"[ReadableController]: 'diableQueued' was set: {InteractableObj} will be re-enabled");
                //     InteractableObj.enabled = true;
                //     InteractableObj.EnableOutline();
                //     InteractableObj = null;
                // }   
                if (!ld.isMainDialogue)
                {
                    Debug.Log($"[ReadableController: {transform.parent.gameObject.name}] Active StoryStage {GetActiveStageIndex()} has no main dialogue to display, auto-completing!");
                    _completedStageIndices.Add(GetActiveStageIndex());
                }
            }
        }

        /// <summary>
        /// Checks if the current active stage has a <main> LocationDialogue with 'normal' stages
        /// These are required for the StoryStage to be marked as 'completed' normally
        /// </summary>
        /// <returns></returns>
        private bool ActiveStageEmpty()
        {
            var ld = _activeStage.locationDialogue;

            if (ld == null) return true; //if no active stage, it can't be empty! (also prevents null ref errors)
            else if (ld.isMainDialogue && ld.StandardLines.Count > 0)
            {
                return false;
            }
            return true;
        }

        #endregion

        #region Story Stage API

        /// <summary>
        /// This function will only ever continue the active StoryStage, making it simpler
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        public DialogueResult GetNextDialogue()
        {
            //Error handling
            if (_activeStage == null)
            {
                Debug.LogError($"[ReadableController: {transform.parent.gameObject.name}] No active StoryStage");
                return new DialogueResult(GetErrorLine());
            }
            var dialogue = _activeStage.locationDialogue;
            if (dialogue == null)
            {
                Debug.LogError($"[ReadableController: {transform.parent.gameObject.name}] Active StoryStage has no dialogue");
                return new DialogueResult(GetErrorLine());
            }

            //Repeat logic
            if (_lineIndex >= dialogue.StandardLines.Count)
            {
                Debug.Log($"[ReadableController: {transform.parent.gameObject.name}] Regular dialogue stages exhausted...");
                var repeatLine = dialogue.GetSpecialLine("repeat");
                if (repeatLine != null)
                {
                    Debug.Log($"[ReadableController: {transform.parent.gameObject.name}] ...Displaying repeat stage");
                    return new DialogueResult(repeatLine, true);
                }
                else
                {
                    Debug.Log($"[ReadableController: {transform.parent.gameObject.name}] ...But no repeat stage assigned, restarting _locationDialogue");
                    _lineIndex = 0;
                }
            }

            //regular line
            var res = new DialogueResult();
            res.Line = dialogue.StandardLines[_lineIndex];
            res.CloseAfter = res.Line.closeAfter; //when a stage is manually marked with it
            _lineIndex++;

            //check if _locDialogue is complete
            if (_lineIndex >= dialogue.StandardLines.Count)
            {
                Debug.Log($"[ReadableController: {transform.parent.gameObject.name}] Finished locationDialogue");
                res.CloseAfter = true;
                if (dialogue.isMainDialogue)
                {
                    Debug.Log($"[ReadableController: {transform.parent.gameObject.name}] Finished MAIN locationDialogue");
                    _completedStageIndices.Add(GetActiveStageIndex());
                }
            }
            return res;
        }

        /// <summary>
        /// Returns null if no line in active StoryStage
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        public DialogueLine GetLeaveRudeDialogue()
        {
            //Error handling
            if (_activeStage == null)
            {
                Debug.LogError($"[ReadableController: {transform.parent.gameObject.name}] No active StoryStage");
                return null;
            }
            var dialogue = _activeStage.locationDialogue;
            if (dialogue == null)
            {
                Debug.LogError($"[ReadableController: {transform.parent.gameObject.name}] Active StoryStage has no dialogue for location");
                return null;
            }
            return dialogue.GetSpecialLine("leave_rude"); //will be null if none found
        }
        public DialogueLine GetLeavePoliteDialogue()
        {
            //Error handling
            if (_activeStage == null)
            {
                Debug.LogError($"[ReadableController: {transform.parent.gameObject.name}] No active StoryStage");
                return null;
            }
            var dialogue = _activeStage.locationDialogue;
            if (dialogue == null)
            {
                Debug.LogError($"[ReadableController: {transform.parent.gameObject.name}] Active StoryStage has no dialogue for location");
                return null;
            }
            if (!_completedStageIndices.Contains(GetActiveStageIndex()))
            {
                Debug.Log($"[ReadableController]: Leave_polite dialogue requested for non-finished StoryStage, ignored!");
                return null;
            }
            return dialogue.GetSpecialLine("leave_polite"); //will be null anyway if none found
        }

        private DialogueLine GetErrorLine()
        {
            DialogueLine line = new DialogueLine();
            line.StageID = "repeat";
            line.Text = $"This text should not appear! Error!";
            return line;
        }

        #endregion

        #region Dialogue API

        [ContextMenu("Next Message")]
        public async void Next()
        {
            // if (!isEnabled) return;
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

            DialogueResult result = GetNextDialogue();
            MessageRead = result.CloseAfter;
            DialogueLine line = result.Line;

            if (line == null)
            {
                EndDialogue();
                return;
            }

            // Dialogue Actions
            foreach (UnityEvent action in line.dialogueActions)
            {
                action.Invoke();
            }

            // 5. Open Dialogue Box if it's currently closed
            if (!isDialogueActive)
            {
                Player.Instance.thinkingBox.OpenDialogue();
                isDialogueActive = true;
            }

            try
            {
                isTyping = true;

                string[] messageLines = line.Text.Split('\n'); //Okay so I absolutely fucking hate this, this means speech HAS to be done by the dialogue box itself ~Lars
                currentTypingTask = Player.Instance.thinkingBox.SetText(messageLines, character, textCtxSource.Token);

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
            // if (!isEnabled) return;

            ResetToken();

            DialogueLine textToDisplay = null;

            if (isDialogueActive)
            {
                // Rude: Left while box was open
                textToDisplay = GetLeaveRudeDialogue();
            }
            else
            {
                // Polite: Left after closing the box
                // now only actually gets a message if the regular stages are done ~Lars
                textToDisplay = GetLeavePoliteDialogue();
            }

            if (textToDisplay != null)
            {
                foreach (UnityEvent action in textToDisplay.dialogueActions)
                {
                    action.Invoke();
                }
                _ = ShowShortMessage(textToDisplay.Text, character);
            }
            else
            {
                CancelCurrentToken();
                EndDialogue();
            }
        }

        private async Task ShowShortMessage(string message, DialogueSpeakerType character)
        {
            try
            {
                if (!isDialogueActive)
                {
                    Player.Instance.thinkingBox.OpenDialogue();
                    isDialogueActive = true;
                }

                isTyping = true;
                await Player.Instance.thinkingBox.SetText(message, character, textCtxSource.Token);
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
            Player.Instance.thinkingBox.CloseDialogue();
            isDialogueActive = false;
            isTyping = false;

            OnDialogueClosed();
            CancelCurrentToken();
        }
        #endregion

        #region DialogueActionJargin

        public void TryTakeItem(ItemTakeActionsArgs args)
        {
            Debug.Log($"[Readable: {transform.parent.gameObject.name}] Trying to take item {args.item} from player inventory");
            if (InventoryController.Instance.TryRemoveItemAtAny(args.item))
            {
                StoryFlagManager.Instance.AddFlag(args.OnSuccess);
                MessageRead = false; //IMPORTANT: this hack is what makes it seem like dialogue is continuous in our item taking instead of closing and re-opening
            }
        }

        public void TryGiveItem(ItemTakeActionsArgs args)
        {
            Debug.Log($"[NpcLocation: {transform.parent.gameObject.name}] Trying to give item {args.item} to player inventory");
            if (InventoryController.Instance.TryAddItemAtAny(args.item))
            {
                StoryFlagManager.Instance.AddFlag(args.OnSuccess);
                MessageRead = false; //IMPORTANT: this hack is what makes it seem like dialogue is continuous in our item giving instead of closing and re-opening
            }
        }

        public void GiveRecipe(RecipeItem item)
        {
            Debug.Log($"[NpcLocation: {transform.parent.gameObject.name}] Trying to give recipe {item} to player recipe book");
            if (RecipeBookController.Instance.TryAddRecipe(item))
            {
                Debug.Log($"[NpcLocation: {transform.parent.gameObject.name}] Successfully gave recipe {item} to player recipe book");
                //StoryFlagManager.Instance.AddFlag(args.OnSuccess);
                //MessageRead = false; //IMPORTANT: this hack is what makes it seem like dialogue is continuous in our item giving instead of closing and re-opening
            }
        }

        public void GiveStoryFlag(StoryFlag flag) => StoryFlagManager.Instance.AddFlag(flag); //required because StoryFlagManager is in a different scene

        public void GiveStoryFlagOnClose(StoryFlag flag)
        {
            if (FlagToSetAfterDialogue != null) Debug.LogWarning($"[ReadableController: {transform.parent.gameObject.name}] there is already a storyflag set to be given after dialogue, overwriting! Previous flag: {FlagToSetAfterDialogue.id}, new flag: {FlagToSetAfterDialogue.id}");
            FlagToSetAfterDialogue = flag;
        }

        public void DisableInteractableOnClose(Interactable interactable)
        {
            _interactable = interactable;
        }

        public void DisableInteractableOnStoryExhausted(Interactable interactable)
        {
            _interactable = interactable;
        }
        #endregion

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

        #region Save & Load

        [Header("Save Options")]
        [SerializeField] private string _guid;
        [ContextMenu("Generate GUID")]
        public void GenerateGuid()
        {
            _guid = Guid.NewGuid().ToString();
        }

        public void SaveData(ref WorldSaveData data)
        {
            data.NPCs.Add(new()
            {
                Guid = _guid,
                currentStageIndex = GetActiveStageIndex(),
                CompletedStageIndices = _completedStageIndices.ToList(),
            });
        }

        public void LoadData(WorldSaveData data)
        {
            foreach (NpcSaveData npcSaveData in data.NPCs)
            {
                if (npcSaveData.Guid == _guid)
                {
                    _completedStageIndices = npcSaveData.CompletedStageIndices.ToHashSet();
                    StartNewStoryStage(_database.storyStages[npcSaveData.currentStageIndex]);
                    break;
                }
            }
            EvaluateActiveStage(true);
        }
        #endregion
    }

}