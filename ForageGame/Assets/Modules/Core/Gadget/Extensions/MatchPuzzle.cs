using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace TDK.Gadgets
{
    public class MatchPuzzle : Gadget
    {
        [Header("Puzzle Options")]
        [SerializeField] private SolutionEntry[] _solution;

        [Serializable]
        private struct SolutionEntry
        {
            public Gadget gadget1;
            public Gadget gadget2;
        }

        public void TrySolve()
        {
            if (Locked) return;

            foreach (SolutionEntry entry in _solution)
                if (entry.gadget1.State != entry.gadget2.State) return;

            PuzzleSolved();
        }

        private void PuzzleSolved()
        {
            SetState(true);
            // Lock everything
            foreach (SolutionEntry entry in _solution)
            {
                entry.gadget1.SetLocked(true);
                entry.gadget2.SetLocked(true);
            }
            SetLocked(true);
        }
    }
}