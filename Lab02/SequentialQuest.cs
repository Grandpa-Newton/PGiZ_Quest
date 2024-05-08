using System;
using System.Collections.Generic;

namespace QuestGame.Logic
{
    internal class SequentialQuest
    {
        public List<InteractableObject> InteractableObjects;
        public Action OnRightPlayerSequence;
        public List<int> RightInteractSequence;
        public List<int> PlayerInteractSequence = new List<int>();
        public int LastPlayerInteract = -1;
        public bool IsStarting = false;

        public SequentialQuest(List<InteractableObject> interactableObjects, List<int> rightInteractSequence, Npc npc)
        {
            InteractableObjects = interactableObjects;
            RightInteractSequence = rightInteractSequence;
            npc.OnStartingQuest += () => { IsStarting = true; };
        }

        public bool AddToPlayerSequence(int interactableObjectIndex)
        {
            if (RightInteractSequence[PlayerInteractSequence.Count] == interactableObjectIndex)
            {
                PlayerInteractSequence.Add(interactableObjectIndex);
                if (RightInteractSequence.Count == PlayerInteractSequence.Count)
                {
                    OnRightPlayerSequence?.Invoke();
                    PlayerInteractSequence.Clear();
                }

                LastPlayerInteract = interactableObjectIndex;
                return true;
            }
            else
            {
                PlayerInteractSequence.Clear();
                return false;
            }
        }
    }
}