using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public abstract class SODatabaseBase : ScriptableObject
{
    public void RunAutoFillButton()
    {
        RunAutoFill();
    }

    protected abstract void RunAutoFill();
}

public abstract class SODatabase<T> : SODatabaseBase where T : ScriptableObject
{
    [Serializable]
    public class Entry
    {
        public string Id;
        public T Asset;
    }

    [SerializeField] protected List<Entry> _entries = new();

    #region Auto Filler

    protected override void RunAutoFill()
    {
        if (!EditorUtility.DisplayDialog("Auto-Fill",
            $"Clear '{name}' and populate with all '{typeof(T).Name}' assets in the project?",
            "Yes", "Cancel")) return;

        var guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
        var assets = guids
            .Select(g => AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(g)))
            //.Where(a => a != null && a.GetType() == typeof(T))
            //.OrderBy(a => a.name)
            .ToList();

        _entries.Clear();
        foreach (var asset in assets)
            _entries.Add(new() { Id = asset.name, Asset = asset });
        Debug.Log($"[SODatabase] Auto-filled '{this.name}' with {assets.Count} entries.");
    }

    #endregion

    #region Public API

    public T GetAsset(string id)
    {
        foreach (var e in _entries)
            if (e.Id == id) return e.Asset;
        return null;
    }
    public IEnumerable<T> GetAssets(IEnumerable<string> ids)
    {
        foreach (string id in ids)
            yield return GetAsset(id);
    }

    public string GetId(T asset)
    {
        foreach (var e in _entries)
            if (e.Asset == asset) return e.Id;
        return null;
    }
    public IEnumerable<string> GetIds(IEnumerable<T> assets)
    {
        foreach (T asset in assets)
            yield return GetId(asset);
    }

    public IEnumerable<T> GetAllAssets()
    {
        foreach (var e in _entries)
            if (e.Asset != null) yield return e.Asset;
    }

    public Dictionary<string, T> AsDictionary()
    {
        Dictionary<string, T> dictionary = new();
        foreach (Entry entry in _entries)
            dictionary.Add(entry.Id, entry.Asset);
        return dictionary;
    }

    #endregion
}