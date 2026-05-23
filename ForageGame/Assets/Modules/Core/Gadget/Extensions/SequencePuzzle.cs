using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace TDK.Gadgets
{
    public class SequencePuzzle : Gadget
    {
        [Header("Puzzle Options")]
        [SerializeField] private List<int> _solutionSequence = new();
        public UnityEvent OnFailed;

        private int _sequenceIndex = 0;

        public void LogInput(int number)
        {
            if (Locked) return;

            if (_solutionSequence[_sequenceIndex] != number)
            {
                PuzzleFailed();
                if (_solutionSequence[0] == number) _sequenceIndex = 1;
                else _sequenceIndex = 0;
            }
            else
                _sequenceIndex++;

            if (_sequenceIndex == _solutionSequence.Count) PuzzleSolved();
        }

        private void PuzzleFailed()
        {
            _sequenceIndex = 0;
            OnFailed.Invoke();
        }

        private void PuzzleSolved()
        {
            SetState(true);
            // Lock everything
            // foreach (SolutionEntry entry in _solution)   ! NOT POSSIBLE !
            //     entry.gadget1.SetLocked(true);
            SetLocked(true);
        }
    }
}