using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;

namespace TDK.Gadgets
{
    public class CodePuzzle : Gadget
    {
        [Header("Puzzle Options")]
        [SerializeField] private SolutionEntry[] _solution;

        [Serializable]
        private struct SolutionEntry
        {
            public Gadget gadget;
            public bool targetState;
        }

        public void TrySolve()
        {
            if (Locked) return;

            foreach (SolutionEntry entry in _solution)
                if (entry.gadget.State != entry.targetState) return;

            PuzzleSolved();
        }

        private void PuzzleSolved()
        {
            SetState(true);
            // Lock everything
            foreach (SolutionEntry entry in _solution)
                entry.gadget.SetLocked(true);
            SetLocked(true);
        }
    }
}