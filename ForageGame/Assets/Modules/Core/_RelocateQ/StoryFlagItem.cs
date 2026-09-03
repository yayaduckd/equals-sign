using UnityEngine;
using Assets.Modules.Interaction;
using UnityEngine.Events;
using System;
using TDK.ItemSystem;


//TODO: just a testing script, should be integrated with general World Items I think ~Lars
public class StoryFlagItem : ItemController
{
    [SerializeField] private StoryFlag flag;

    override public void AttemptInteract()
    {
        StoryFlagManager.Instance.AddFlag(flag);
        // Unlock Recipies
        // TODO
        // TODO: show discovery
        Destroy(gameObject);
    }
}
