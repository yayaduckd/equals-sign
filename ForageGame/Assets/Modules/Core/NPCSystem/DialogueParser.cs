using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using TDK.ItemSystem;

namespace NPC
{
    public class DialogueParser: MonoBehaviour
    {
        //Stateful reading of lines
        private class LineReader
        {
            private readonly string[] _lines;
            private int _index;

            public LineReader(string[] lines) => _lines = lines;
            public bool HasLines => _index < _lines.Length;
            public string Peek() => _lines[_index].Trim();

            public string Consume()
            {
                string line = _lines[_index].Trim();
                _index++;
                return line;
            }
            public void SkipEmpty()
            {
                while (HasLines && string.IsNullOrEmpty(Peek())) Consume();
            }
        }

        private LineReader reader;
        private Dictionary<string, NpcLocation> _locations;
        private Dictionary<string, UnityEvent> _actions;
        private Dictionary<string, ItemData> _items;
        private Dictionary<string, StoryFlag> _flags;
        public DialogueDatabase Parse(string fileContents, 
                                            Dictionary<string, StoryFlag> flags, 
                                            Dictionary<string, ItemData> items,
                                            Dictionary<string, NpcLocation> locations, 
                                            Dictionary<string, UnityEvent> dialogueActions)
        {
            _locations = locations;
            _actions = dialogueActions;
            _items = items;
            _flags = flags;

            DialogueDatabase db = new DialogueDatabase();

            reader = new LineReader(fileContents.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None));
            while (reader.HasLines)
            {
                reader.SkipEmpty();
                if (!reader.HasLines) break; //end of file safegaurd

                var currentLine = reader.Consume();
                //Debug.Log(currentLine);

                if (currentLine.StartsWith("---")) //StoryStage marker
                    db.storyStages.Add(ParseStoryStage());
                else //anything else is incorrect input
                {
                    Debug.LogWarning($"[DialogueParser] Unexpected line in DialogueDatabase: '{currentLine}'");
                }
            }
            return db;
        }

        public ReadableDialogueDatabase ParseReadable(string fileContents, 
                                            Dictionary<string, StoryFlag> flags, 
                                            Dictionary<string, ItemData> items,
                                            Dictionary<string, UnityEvent> dialogueActions)
        {
            _actions = dialogueActions;
            _items = items;
            _flags = flags;

            ReadableDialogueDatabase db = new ReadableDialogueDatabase();

            reader = new LineReader(fileContents.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None));
            while (reader.HasLines)
            {
                reader.SkipEmpty();
                if (!reader.HasLines) break; //end of file safegaurd

                var currentLine = reader.Consume();
                //Debug.Log(currentLine);

                if (currentLine.StartsWith("---")) //StoryStage marker
                    db.storyStages.Add(ParseReadableStage());
                else //anything else is incorrect input
                {
                    Debug.LogWarning($"[DialogueParser] Unexpected line in DialogueDatabase: '{currentLine}'");
                }
            }
            return db;
        }

        private StoryStage ParseStoryStage()
        {
            StoryStage stage = new StoryStage();

            //Debug.Log("Parsing a StoryStage!");

            while (reader.HasLines)
            {
                reader.SkipEmpty();
                string line = reader.Peek();
                if (line.StartsWith("---")) break; // next stage
                else if (line.StartsWith("Flags:", StringComparison.OrdinalIgnoreCase))
                {
                    stage.RequiredFlags = ParseReferenceList(reader.Consume(), _flags, "Flags");
                }
                else if (line.StartsWith("Required-Items:")) //Items
                {
                    if(stage.requiredItems.Count > 0) Debug.LogError("Duplicate \"Required-Items:\" attribute in StoryStage!");
                    stage.requiredItems = ParseReferenceList(reader.Consume(), _items, "Items");
                }
                else if (line.StartsWith("<RequireTimePassing>")) //Time passing requirement
                {
                    stage.requiresTimePassing = true;
                    reader.Consume();
                }
                else if (line.StartsWith("Location:")) //LocationDialogue
                {
                    var locations = ParseReferenceList(reader.Consume(), _locations, "Locations");
                    if (locations.Count > 1) //safegaurd for multiple entered locations
                        Debug.LogError("[DialogueParser] NpcLocation: expects exactly one location ID, expect duplicate dialogue (or problems)");
                    var locDialog = ParseLocationDialogue();
                    foreach (NpcLocation loc in locations)
                    {
                        stage.locationDialogues.Add(loc, locDialog);
                    }
                }
                else //everything else is WRONG
                {
                    Debug.LogWarning($"[DialogueParser] Unexpected line in StoryStage: '{reader.Consume()}'");
                }
            }

            //Debug.Log($"StoryStage Parsed: Flags: {stage.RequiredFlags}, Items: {stage.requiredItems}, LocDialogueCount: {stage.locationDialogues.Count}");
            return stage;
        }

        private ReadableStage ParseReadableStage()
        {
            ReadableStage stage = new ReadableStage();

            //Debug.Log("Parsing a ReadableStage!");

            while (reader.HasLines)
            {
                reader.SkipEmpty();
                string line = reader.Peek();
                if (line.StartsWith("---")) break; // next stage
                else if (line.StartsWith("Flags:", StringComparison.OrdinalIgnoreCase))
                {
                    stage.RequiredFlags = ParseReferenceList(reader.Consume(), _flags, "Flags");
                }
                else if (line.StartsWith("Required-Items:")) //Items
                {
                    if(stage.requiredItems.Count > 0) Debug.LogError("Duplicate \"Required-Items:\" attribute in StoryStage!");
                    stage.requiredItems = ParseReferenceList(reader.Consume(), _items, "Items");
                }
                else if (line.StartsWith("<RequireTimePassing>")) //Time passing requirement
                {
                    stage.requiresTimePassing = true;
                    reader.Consume();
                }
                else if (line.StartsWith("<main>") || line.StartsWith("Stage:")) //dialogue
                {
                    stage.locationDialogue = ParseLocationDialogue();
                }
                else //everything else is WRONG
                {
                    Debug.LogWarning($"[DialogueParser] Unexpected line in StoryStage: '{reader.Consume()}'");
                }
            }

            //Debug.Log($"StoryStage Parsed: Flags: {stage.RequiredFlags}, Items: {stage.requiredItems}, LocDialogueCount: {stage.locationDialogues.Count}");
            return stage;
        }

        /// <summary>
        /// This one is a mess
        /// Don't feel like fixing it
        /// </summary>
        private LocationDialogue ParseLocationDialogue()
        {
            var ld = new LocationDialogue();

            while (reader.HasLines)
            {
                reader.SkipEmpty();
                if (!reader.HasLines) break; //end of file

                string line = reader.Peek();

                // exit conditions - do not consume, parent owns these
                if (line.StartsWith("---") || line.StartsWith("Location:")) break;

                if (line.StartsWith("<")) //main or flavor text marker
                {
                    ld.isMainDialogue = reader.Consume().Equals("<main>", StringComparison.OrdinalIgnoreCase);
                }
                else if (line.StartsWith("BaseEmotion:"))
                    ld.baseEmotion = ParseValue(reader.Consume());
                else if (line.StartsWith("Stage:"))
                    ld.Lines.Add(ParseDialogueLine());

                else
                    Debug.LogWarning($"[DialogueParser] Unexpected line in LocationDialogue: '{reader.Consume()}'");
            }

            // Debug.Log($"LocationDialogue Parsed: isMain: {ld.isMainDialogue}, LineCount: {ld.Lines.Count()}, StandardLineCount: {ld.StandardLines.Count()}");
            return ld;
        }

        private DialogueLine ParseDialogueLine()
        {
            //Debug.Log($"Parsing DialogueLine!");
            var dl = new DialogueLine();
            dl.StageID = ParseValue(reader.Consume()); // consume "Stage: x"

            while (reader.HasLines)
            {
                reader.SkipEmpty();
                if (!reader.HasLines) break; //end of file

                string line = reader.Peek();

                // exit conditions - parent owns these
                if (line.StartsWith("Stage:")    ||
                    line.StartsWith("Location:") ||
                    line.StartsWith("---")) break;

                if (line.StartsWith("Emotion:"))
                    dl.emotion = ParseValue(reader.Consume()); //this stays a string

                else if (line.StartsWith("Actions:"))
                    dl.dialogueActions = ParseReferenceList(reader.Consume(), _actions, "Dialogue Actions");

                else if (line.StartsWith("<CloseAfter>"))
                {
                    dl.closeAfter = true;
                    reader.Consume();
                }
                else
                {
                    // plain text - accumulate multiline
                    if (!string.IsNullOrEmpty(dl.Text)) dl.Text += "\n";
                    dl.Text += reader.Consume();
                }
            }

            if (string.IsNullOrEmpty(dl.Text))
                Debug.LogWarning($"[DialogueParser] Stage '{dl.StageID}' has no text.");

            //Debug.Log($"DialogueLine Parsed: Stage: {dl.StageID}, Emotion: {dl.emotion}, Actions: {dl.dialogueActions}, text: {dl.Text}");
            return dl;
        }

        #region Utility
        //trim off the pre-value stuffs
        private string ParseValue(string line)
        {
            int idx = line.IndexOf(':');
            return idx == -1 ? "" : line[(idx + 1)..].Trim();
        }

        private List<string> ParseValueList(string line)
        {
            string val = ParseValue(line);
            if (string.IsNullOrEmpty(val)) return new List<string>();
            return val.Split(',').Select(s => s.Trim())
                    .Where(s => !string.IsNullOrEmpty(s))
                    .ToList();
        }

        private List<T> ParseReferenceList<T>(string line, Dictionary<string, T> lookup, string context) //context is what we were trying to parse, i.e., StoryFlag
        {
            return ParseValueList(line).Select(key =>
            {
                if (lookup.TryGetValue(key, out var value)) return (true, value);
                Debug.LogError($"[DialogueParser] Unknown key '{key}' in {context}"); //DOES NOT WORK!
                return (false, default);
            })
            .Where(r => r.Item1)
            .Select(r => r.Item2)
            .ToList();
        }
        #endregion
    }
}