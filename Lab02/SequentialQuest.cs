using System;
using System.Collections.Generic;
using QuestGame;
using SharpDX;

namespace Lab01
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
            npc.OnStartingQuest += () =>
            {
                IsStarting = true;
            };
        }

        public void AddToPlayerSequence(InteractableObject interactableObject)
        {
            
        }

        public bool AddToPlayerSequence(int interactableObjectIndex)
        {
            if (RightInteractSequence[PlayerInteractSequence.Count] == interactableObjectIndex)
            {
                PlayerInteractSequence.Add(interactableObjectIndex);
                if (RightInteractSequence.Count == PlayerInteractSequence.Count)
                {
                    OnRightPlayerSequence?.Invoke();
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

        private bool CheckIfRightInteractSequence()
        {
            if (PlayerInteractSequence.Count != RightInteractSequence.Count)
            {
                for (int i = 0; i < PlayerInteractSequence.Count; i++)
                {
                    if (PlayerInteractSequence[i] != RightInteractSequence[i])
                    {
                        return false;
                    }
                }
                return true;
            }
            
            for (int i = 0; i < PlayerInteractSequence.Count; i++)
            {
                if (PlayerInteractSequence[i] != RightInteractSequence[i])
                {
                    return false;
                }
            }
            
            OnRightPlayerSequence?.Invoke();

            return true;
        }
    }
}