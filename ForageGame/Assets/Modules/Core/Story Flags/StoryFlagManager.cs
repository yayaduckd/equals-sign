using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using System;
using System.Collections.ObjectModel;


public class StoryFlagManager : MonoBehaviour
{
    public static StoryFlagManager Instance { get; private set; }

    // Database: ID → SO
    public Dictionary<string, StoryFlag> flagDatabase; //I'm not bothering with a ReadOnlyDictionary it is a hassle ~Lars

    // Active flags: SO → active
    private HashSet<StoryFlag> activeFlags;

    public static event Action<StoryFlag> onFlagAdded;
    public static event Action<StoryFlag> onFlagRemoved;
    public static event Action onTimePassing; //for StoryStages that require time to have passed since the last one, to make sense story-wise.

    private void Awake()
    {
        //Singleton management
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadAllFlags(); //construct existing flag database
        activeFlags = new HashSet<StoryFlag>(); //clear existing flags TODO: load from save
    }

    private void LoadAllFlags()
    {
        flagDatabase = new Dictionary<string, StoryFlag>();

        // Load all StoryFlag SOs placed in Resources/StoryFlags
        StoryFlag[] all = Resources.LoadAll<StoryFlag>("StoryFlags");

        foreach (var f in all)
        {
            if (string.IsNullOrEmpty(f.id))
            {
                Debug.LogWarning($"StoryFlag SO '{f.name}' has no ID!");
                continue;
            }

            if (!flagDatabase.ContainsKey(f.id))
            {
                flagDatabase.Add(f.id, f);
                print($"StoryFlag found: {f.id}");
            }
            else
            {
                Debug.LogWarning($"Duplicate StoryFlag ID '{f.id}'!");
            }
        }
    }

    //does this string match an actual StoryFlag
    public bool TryGetStoryFlag(string id, out StoryFlag flag)
    {
        return flagDatabase.TryGetValue(id, out flag);
    }

    public void AddFlag(StoryFlag flag)
    {
        if (flag == null) return;

        if (activeFlags.Add(flag))
        {
            Debug.Log($"StoryFlag activated: {flag.id}");
            onFlagAdded?.Invoke(flag);
        }
    } 

    public void RemoveFlag(StoryFlag flag)
    {
        if (flag == null) return;

        if (activeFlags.Remove(flag))
        {
            Debug.Log($"StoryFlag deactivated: {flag.id}");
            onFlagRemoved?.Invoke(flag);
        }
    }

    //Only called when sleeping, since on flag add does it automatically and we don't want to accidentally call it twice and break things
    public void OnTimePassing()
    {
        Debug.Log("[StoryFlagManager]: Time has passed");
        onTimePassing?.Invoke();
    } 

    //Check if flag active
    public bool FlagActive(StoryFlag flag)
    {
        return activeFlags.Contains(flag);
    }

    //check an entire list at once
    public bool FlagListActive(IEnumerable<StoryFlag> required)
    {
        return activeFlags.IsSupersetOf(required);
    }
}
