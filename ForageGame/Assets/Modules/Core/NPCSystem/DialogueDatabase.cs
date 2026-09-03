using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;
using TDK.ItemSystem;

namespace NPC
{
    /// <summary>
    /// top-level container, might host some helper funcs later
    /// </summary>
    [Serializable]
    public class DialogueDatabase
    {
        public List<StoryStage> storyStages = new List<StoryStage>();
    }

    [Serializable]

    public class ReadableDialogueDatabase
    {
        public List<ReadableStage> storyStages = new List<ReadableStage>();
    }

    /// <summary>
    /// The stage of progression in the story at any one point.
    /// Holds the dialogue of multiple active NpcLocations at once, though not all of them may progress the story (only main lines do).
    /// Thus, can be viewed as an 'active state' as such.
    /// </summary>
    [Serializable]
    public class StoryStage
    {
        //Indices are now implicit, by the ordering in the input file!
        public List<StoryFlag> RequiredFlags = new List<StoryFlag>();
        //-> Setting flags is done as a Dialogue Action
        public List<ItemData> requiredItems = new List<ItemData>(); //decide how to actually 'take' items, actions I guess?
        public bool requiresTimePassing = false; //for StoryStages that can't happen immediately after the last one to make sense story-wise.
        public List<UnityEvent> stageActions = new List<UnityEvent>(); //Some StoryStages need to have stuff happen on activation immediately (mostly Lyria regarding Princess' state) that can't be reacted to otherwise.
        public Dictionary<NpcLocation, LocationDialogue> locationDialogues = new Dictionary<NpcLocation, LocationDialogue>(); //not-so serializable anymore lolol

    }

    //variant for Readables, who do not have locations
    [Serializable]
    public class ReadableStage
    {
        //Indices are now implicit, by the ordering in the input file!
        public List<StoryFlag> RequiredFlags = new List<StoryFlag>();
        //-> Setting flags is done as a Dialogue Action
        public List<ItemData> requiredItems = new List<ItemData>(); //decide how to actually 'take' items, actions I guess?
        public bool requiresTimePassing = false; //for StoryStages that can't happen immediately after the last one to make sense story-wise.
        public List<UnityEvent> stageActions = new List<UnityEvent>(); //Some StoryStages need to have stuff happen on activation immediately (mostly Lyria regarding Princess' state) that can't be reacted to otherwise.
        public LocationDialogue locationDialogue;

    }

    /// <summary>
    /// Holds the dialoge for a given NpcLocation
    /// </summary>
    [Serializable]
    public class LocationDialogue
    {
        //Flavor dialogue does not progress StoryStage. Use for puzzle hints or indeed flavor text
        public bool isMainDialogue = false;
        public string baseEmotion;
        public List<DialogueLine> Lines = new List<DialogueLine>();

        // --- Helpers for specific line types ---
        //TODO: Lars no likey

        // The numeric story lines (0, 1, 2...)
        public List<DialogueLine> StandardLines => Lines.Where(l => l.IsStoryStage).ToList();
        public DialogueLine GetSpecialLine(string stageID) //TODO: THIS IS HOW WE GET LEAVE MESSAGES AND STUFF! SUCKS!
        {
            return Lines.FirstOrDefault(l => l.StageID.Equals(stageID, StringComparison.OrdinalIgnoreCase));
        }
    }

    [Serializable]
    public class DialogueLine
    {
        public string StageID;
        public string emotion; //which sprite to use, can be left at null
        public bool closeAfter = false;
        public string Text;
        public List<UnityEvent> dialogueActions = new List<UnityEvent>(); //also contains setting flags!

        // Returns true if the stage is NOT one of our special keywords
        public bool IsStoryStage
        {
            get
            {
                // If it parses as a number, it's a story stage. 
                // Alternatively, just check against known keywords.
                return int.TryParse(StageID, out _);
            }
        }
    }
}