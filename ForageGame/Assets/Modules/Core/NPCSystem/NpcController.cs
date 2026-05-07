using System.Collections.Generic;
using System.Linq;
using TDK.ItemSystem;
using TDK.ItemSystem.Inventory;
using UnityEngine;
using UnityEngine.Events;

namespace NPC
{
    public enum Character { Bracken, Mosswick, Grimble, Lyria }; 

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
        [SerializeField] public Character character;
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

        void Awake()
        {
            locations = GetComponentsInChildren<NpcLocation>().ToList();
        }

        //Changed to Start() from Awake() since it gave inconsistent behavior in terms of timing ~Lars
        private void Start()
        {
            StoryFlagManager.onFlagAdded += OnNewStoryFlag;
            InventoryController.onNewItemSeen += OnNewItemSeen;
            _database = parser.Parse(_sourceFile.text, 
                                    StoryFlagManager.Instance.flagDatabase, 
                                    dialogueReferences.GetItemDataMap(), 
                                    dialogueReferences.GetNpcLocationsMap(), 
                                    dialogueReferences.GetDialogueActionMap());
            EvaluateActiveStage();
        }
        #region Stage Management

        private int GetActiveStageIndex() => _activeStage == null ? -1 : _database.storyStages.IndexOf(_activeStage);

        private void OnNewStoryFlag(StoryFlag flag)
        {
            if(_completedStageIndices.Contains(GetActiveStageIndex())) EvaluateActiveStage(); //do this only if current stage is done
        }
        private void OnNewItemSeen(ItemData item)
        {
            if(_completedStageIndices.Contains(GetActiveStageIndex())) EvaluateActiveStage(); //do this only if current stage is done
        }

        private void EvaluateActiveStage()
        {
            Debug.Log($"[NpcController: {character}] Re-evaluating active stage, current stage index is {GetActiveStageIndex()}");

            int startIndex = GetActiveStageIndex();

            //this gets a default value if no target is found which will be null
            var next = _database.storyStages
                .Skip(startIndex)
                .FirstOrDefault(s => 
                    !_completedStageIndices.Contains(_database.storyStages.IndexOf(s)) &&
                    StoryFlagManager.Instance.FlagListActive(s.RequiredFlags) &&
                    s.requiredItems.All(item => InventoryController.Instance.seenItems.Contains(item)));

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
            if(ActiveStageEmpty()) 
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
            foreach (var loc in locations)
                loc.gameObject.SetActive(_activeStage != null && _activeStage.locationDialogues.ContainsKey(loc));
            
            //set init emotion
            foreach (var loc in _activeStage.locationDialogues.Keys)
            {
                if(!string.IsNullOrEmpty(_activeStage.locationDialogues[loc].baseEmotion)) loc.SetEmotion(_activeStage.locationDialogues[loc].baseEmotion);
            }
        }

        //mostly used for returning to base emotion after dialogue is closed
        public string GetBaseEmotion(NpcLocation loc)
        {
            //Error handling
            if(_activeStage == null) 
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
            foreach(LocationDialogue dialogue in _activeStage.locationDialogues.Values)
            {
                if(dialogue.isMainDialogue && dialogue.StandardLines.Count > 0) 
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
            //Error handling
            if(_activeStage == null) 
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
                    return  new DialogueResult(repeatLine, true);
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
            if(_lineIndices[location] >= dialogue.StandardLines.Count)
            {
                Debug.Log($"[NpcController: {character}] Finished locationDialogue");
                res.CloseAfter = true;
                if(dialogue.isMainDialogue)
                {
                    Debug.Log($"[NpcController: {character}] Finished MAIN locationDialogue");
                    _completedStageIndices.Add(GetActiveStageIndex());
                    EvaluateActiveStage();
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
            if(_activeStage == null) 
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
            if(_activeStage == null) 
            {
                Debug.LogError($"[NpcController: {character}] No active StoryStage");
                return null;
            }
            if (!_activeStage.locationDialogues.TryGetValue(location, out var dialogue))
            {
                Debug.LogError($"[NpcController: {character}] Active StoryStage has no dialogue for location: {location}");
                return null;
            } 
            if(!_completedStageIndices.Contains(GetActiveStageIndex()))
            {
                Debug.Log($"[NpcController]: Leave_polite dialogue requested for non-finished StoryStage, ignored!");
                return null;
            }
            return dialogue.GetSpecialLine("leave_polite"); //will be null anyway if none found
        }

        private DialogueLine GetErrorLine()
        {
            DialogueLine line = new DialogueLine();
            line.StageID="repeat";
            line.Text = $"This text should not appear! Error!";
            return line;
        }

        #endregion
    }
}
