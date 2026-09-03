using System;
using UnityEngine;

namespace InGameCutscenes
{
    public class InGameCutsceneState : StateMachineBehaviour
    {
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
        }

        override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
        }

        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            InGameCutsceneManager.Instance.OnStateExit();
        }
    }
}