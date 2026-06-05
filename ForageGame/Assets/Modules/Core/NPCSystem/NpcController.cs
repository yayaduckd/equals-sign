using System.Collections.Generic;
using System.Linq;
using TDK.ItemSystem;
using TDK.ItemSystem.Inventory;
using UnityEngine;
using UnityEngine.Events;
using TDK.ItemSystem.Types;

namespace NPC
{
    public enum DialogueSpeakerType { Bracken, Mosswick, Grimble, Lyria, WizardRock };

    //What API calls return, to return this control to the NpcController instead
    public struct DialogueResult
    {
        public DialogueLine Line;
        public bool CloseAfter;

        public DialogueResult(DialogueLine line, bool CloseAfter = false)
        {
            this.Line = line;
            this.CloseAfter = CloseAfter;
        }
    }

    public class NpcController : MonoBehaviour
    {

        [Header("Dialogue Data")]
        [SerializeField] public DialogueSpeakerType character;
        [SerializeField] private TextAsset _sourceFile;
        [SerializeField] private DialogueParser parser;

        [SerializeField] private DialogueDatabase _database;

        [Header("References")]
        [SerializeField] private DialogueReferences dialogueReferences;
        [SerializeField] private List<NpcLocation> locations;

        [Header("Current State")]
        [SerializeField] private StoryStage _activeStage;
        [SerializeField] private Dictionary<NpcLocation, int> _lineIndices = new();
        [SerializeField] private HashSet<int> _completedStageIndices = new();

        [SerializeField] private NpcLocation _lastActiveLocation;

        [Header("Dialogue display settings")]
        [Tooltip("character count -> syllable count. Clamped between 1 and 10")]
        [SerializeField] private AnimationCurve syllableCountCurve;

        void Awake()
        {
            locations = GetComponentsInChildren<NpcLocation>().ToList();
        }

        //Changed to Start() from Awake() since it gave inconsistent behavior in terms of timing ~Lars
        private void Start()
        {
            StoryFlagManager.onFlagAdded += OnNewStoryFlag;
            StoryFlagManager.onTimePassing += OnTimePassing;
            InventoryController.onNewItemSeen += OnNewItemSeen;
            _database = parser.Parse(_sourceFile.text,
                                    StoryFlagManager.Instance.flagDatabase.AsDictionary(),
                                    dialogueReferences.GetItemDataMap(),
                                    dialogueReferences.GetNpcLocationsMap(),
                                    dialogueReferences.GetDialogueActionMap());
            EvaluateActiveStage();

            //this sucks but I have to since only this object knows how long a given line is
            foreach (DialogueBox box in GetComponentsInChildren<DialogueBox>())
            {
                box.syllableCountCurve = syllableCountCurve;
            }
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
            if (_completedStageIndices.Contains(GetActiveStageIndex())) EvaluateActiveStage(); //do this only if current stage is done
        }
        private void EvaluateActiveStage(bool timePassed = false)
        {
            Debug.Log($"[NpcController: {character}] Re-evaluating active stage, current stage index is {GetActiveStageIndex()}");

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
                Debug.Log($"[NpcController: {character}] No new Active StoryStage detected");
                return; //if makes no difference nothing changes!
            }

            StartNewStoryStage(next);
        }

        private void StartNewStoryStage(StoryStage stage)
        {
            _activeStage = stage;
            Debug.Log($"[NpcController: {character}] New active StoryStage set with index {GetActiveStageIndex()}");

            //update location indices
            _lineIndices.Clear();
            foreach (var loc in _activeStage.locationDialogues.Keys)
            {
                _lineIndices.Add(loc, 0);
            }

            UpdateActiveLocations();

            //if the current stage has no main dialogue to display, auto-complete it
            if (ActiveStageEmpty())
            {
                Debug.Log($"[NpcController: {character}] Active StoryStage {GetActiveStageIndex()} has no main dialogue to display, auto-completing!");
                _completedStageIndices.Add(GetActiveStageIndex());
                //NOTE: I do not re-check for new active stage since an empty storystage is a deliberate choice, to have a break in the story.
                //Thus, this will only be done when picking up a new flag or item.
            }
        }

        //turn off NpcLocations that have no dialogue set in the active StoryStage
        private void UpdateActiveLocations()
        {
            if (_activeStage == null)
            {
                Debug.LogError($"[NpcController: {character}] No active StoryStage");
                return;
            }

            foreach (var loc in locations)
                if (_activeStage.locationDialogues.ContainsKey(loc))
                {
                    loc.gameObject.SetActive(true); //will play the popup animation if not already active
                }
                else
                {
                    loc.ShrinkAway(); //play the shrink away animation before auto-deactivating
                }

            //set init emotion
            foreach (var loc in _activeStage.locationDialogues.Keys)
            {
                if (!string.IsNullOrEmpty(_activeStage.locationDialogues[loc].baseEmotion)) loc.SetEmotion(_activeStage.locationDialogues[loc].baseEmotion);
            }
        }

        //mostly used for returning to base emotion after dialogue is closed
        public string GetBaseEmotion(NpcLocation loc)
        {
            //Error handling
            if (_activeStage == null)
            {
                Debug.LogError($"[NpcController: {character}] No active StoryStage");
                return null;
            }
            if (!_activeStage.locationDialogues.TryGetValue(loc, out var dialogue))
            {
                Debug.LogError($"[NpcController: {character}] Active StoryStage has no dialogue for location: {loc}");
                return null;
            }
            return dialogue.baseEmotion;
        }

        /// <summary>
        /// Checks if the current active stage has a <main> LocationDialogue with 'normal' stages
        /// These are required for the StoryStage to be marked as 'completed' normally
        /// </summary>
        /// <returns></returns>
        private bool ActiveStageEmpty()
        {
            var res = true;
            foreach (LocationDialogue dialogue in _activeStage.locationDialogues.Values)
            {
                if (dialogue.isMainDialogue && dialogue.StandardLines.Count > 0)
                {
                    res = false;
                    break;
                }
            }
            return res;
        }

        #endregion

        #region API

        /// <summary>
        /// This function will only ever continue the active StoryStage, making it simpler
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        public DialogueResult GetNextDialogue(NpcLocation location)
        {
            _lastActiveLocation = location;
            //Error handling
            if (_activeStage == null)
            {
                Debug.LogError($"[NpcController: {character}] No active StoryStage");
                return new DialogueResult(GetErrorLine());
            }
            if (!_activeStage.locationDialogues.TryGetValue(location, out var dialogue))
            {
                Debug.LogError($"[NpcController: {character}] Active StoryStage has no dialogue for location: {location}");
                return new DialogueResult(GetErrorLine());
            }

            //Repeat logic
            if (_lineIndices[location] >= dialogue.StandardLines.Count)
            {
                Debug.Log($"[NpcController: {character}] Regular dialogue stages exhausted...");
                var repeatLine = dialogue.GetSpecialLine("repeat");
                if (repeatLine != null)
                {
                    Debug.Log($"[NpcController: {character}] ...Displaying repeat stage");
                    return new DialogueResult(repeatLine, true);
                }
                else
                {
                    Debug.Log($"[NpcController: {character}] ...But no repeat stage assigned, restarting LocationDialogue");
                    _lineIndices[location] = 0;
                }
            }

            //regular line
            var res = new DialogueResult();
            res.Line = dialogue.StandardLines[_lineIndices[location]];
            res.CloseAfter = res.Line.closeAfter; //when a stage is manually marked with it
            _lineIndices[location]++;

            //check if LocationDialogue is complete
            if (_lineIndices[location] >= dialogue.StandardLines.Count)
            {
                Debug.Log($"[NpcController: {character}] Finished locationDialogue");
                res.CloseAfter = true;
                if (dialogue.isMainDialogue)
                {
                    Debug.Log($"[NpcController: {character}] Finished MAIN locationDialogue");
                    _completedStageIndices.Add(GetActiveStageIndex());
                    //EvaluateActiveStage(); //TODO: only do after the box is closed
                }
            }
            return res;
        }

        /// <summary>
        /// Returns null if no line in active StoryStage
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        public DialogueLine GetLeaveRudeDialogue(NpcLocation location)
        {
            //Error handling
            if (_activeStage == null)
            {
                Debug.LogError($"[NpcController: {character}] No active StoryStage");
                return null;
            }
            if (!_activeStage.locationDialogues.TryGetValue(location, out var dialogue))
            {
                Debug.LogError($"[NpcController: {character}] Active StoryStage has no dialogue for location: {location}");
                return null;
            }
            return dialogue.GetSpecialLine("leave_rude"); //will be null if none found
        }
        public DialogueLine GetLeavePoliteDialogue(NpcLocation location)
        {
            //Error handling
            if (_activeStage == null)
            {
                Debug.LogError($"[NpcController: {character}] No active StoryStage");
                return null;
            }
            if (!_activeStage.locationDialogues.TryGetValue(location, out var dialogue))
            {
                Debug.LogError($"[NpcController: {character}] Active StoryStage has no dialogue for location: {location}");
                return null;
            }
            if (!_completedStageIndices.Contains(GetActiveStageIndex()))
            {
                Debug.Log($"[NpcController]: Leave_polite dialogue requested for non-finished StoryStage, ignored!");
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

        #region DialogueActionJargin

        public void FaceTowardPlayer() => _lastActiveLocation.FaceTowardPlayer();

        public void FaceAwayFromPlayer() => _lastActiveLocation.FaceAwayFromPlayer();

        public void GiveStoryFlag(StoryFlag flag) => StoryFlagManager.Instance.AddFlag(flag); //required because StoryFlagManager is in a different scene

        public void TryTakeItem(ItemTakeActionsArgs args)
        {
            Debug.Log($"[NpcLocation: {gameObject.name}] Trying to take item {args.item} from player inventory");
            if (InventoryController.Instance.TryRemoveItemAtAny(args.item))
            {
                StoryFlagManager.Instance.AddFlag(args.OnSuccess);
                _lastActiveLocation.MessageRead = false; //IMPORTANT: this hack is what makes it seem like dialogue is continuous in our item taking instead of closing and re-opening
            }
        }

        public void TryGiveItem(ItemTakeActionsArgs args)
        {
            Debug.Log($"[NpcLocation: {gameObject.name}] Trying to give item {args.item} to player inventory");
            if (InventoryController.Instance.TryAddItemAtAny(args.item))
            {
                StoryFlagManager.Instance.AddFlag(args.OnSuccess);
                _lastActiveLocation.MessageRead = false; //IMPORTANT: this hack is what makes it seem like dialogue is continuous in our item giving instead of closing and re-opening
            }
        }

        public void GiveRecipe(RecipeItem item)
        {
            Debug.Log($"[NpcLocation: {gameObject.name}] Trying to give recipe {item} to player recipe book");
            if (RecipeBookController.Instance.TryAddRecipe(item))
            {
                Debug.Log($"[NpcLocation: {gameObject.name}] Successfully gave recipe {item} to player recipe book");
                //StoryFlagManager.Instance.AddFlag(args.OnSuccess);
                //MessageRead = false; //IMPORTANT: this hack is what makes it seem like dialogue is continuous in our item giving instead of closing and re-opening
            }
        }
        #endregion
    }
}
