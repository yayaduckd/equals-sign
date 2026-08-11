using UnityEngine;
using TDK.PlayerSystem;
using TDK.ItemSystem.Inventory;
using TDK.ItemSystem;
using System.Collections.Generic;
using TDK.EnemySystem;
using System;
using TDK.Gadgets;
using NPC;

namespace TDK.SaveSystem
{
    [System.Serializable]
    public class WorldSaveData
    {
        public int playtimeSeconds = 0;
        public PlayerSaveData Player = new();
        public InventorySaveData Inventory = new();
        public List<ItemSaveData> Items = new();
        public List<EnemySaveData> Enemies = new();
        public List<GadgetSaveData> Gadgets = new(); // string is GUID
        public List<string> StoryFlagSaveData = new();
        public List<NpcSaveData> NPCs = new();
    }
}