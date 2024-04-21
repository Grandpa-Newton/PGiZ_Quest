using System;
using QuestGame;

namespace Lab01
{
    internal class Npc
    {
        public MeshObject GameObject;
        public string FirstInteractText;
        public string RepeatedInteractText;
        public string InteractTextAfterComplete;


        public Action OnStartingQuest;
        
        public NpcStates NpcState;

        public MainInventoryItem[] GivingItems;
        public CollectibleItem[] GivingCollectibles;
        public MainInventoryItem[] TakenItems;
        public NpcResponse Interact()
        {
            NpcResponse npcResponse = null;
            switch (NpcState)
            {
                case NpcStates.BeforeGivingQuest:
                    npcResponse = new NpcResponse(GivingItems, FirstInteractText);
                    NpcState = NpcStates.AfterGivingQuest;
                    OnStartingQuest?.Invoke();
                    break;
                
                case NpcStates.AfterGivingQuest:
                    npcResponse = new NpcResponse(null, RepeatedInteractText);
                    break;
                
                case NpcStates.AfterQuestComplete:
                    npcResponse = new NpcResponse(TakenItems, InteractTextAfterComplete);
                    break;
            }

            return npcResponse;
        }

        public CollectibleItem[] GetCollectibles()
        {
            if (NpcState == NpcStates.AfterQuestComplete)
            {
                return GivingCollectibles;
            }
            else
            {
                return null;
            }
        }

        public Npc(MeshObject gameObject, string firstInteractText, string repeatedInteractText,
            string interactTextAfterComplete, ref Action questCompleteAction, MainInventoryItem[] givingItems = null, CollectibleItem[] givingCollectibles = null,
            MainInventoryItem[] takenItems = null)
        {
            GameObject = gameObject;
            FirstInteractText = firstInteractText;
            RepeatedInteractText = repeatedInteractText;
            InteractTextAfterComplete = interactTextAfterComplete;

            if (givingItems != null)
            {
                GivingItems = givingItems;
            }

            if (givingCollectibles != null)
            {
                GivingCollectibles = givingCollectibles;
            }

            if (takenItems != null)
            {
                TakenItems = takenItems;
            }
            
            questCompleteAction += QuestComplete;
        }

        private void QuestComplete()
        {
            NpcState = NpcStates.AfterQuestComplete;
        }
    }

    internal class NpcResponse
    {
        public MainInventoryItem[] Items;
        public string ResponseText;
        

        public NpcResponse(MainInventoryItem[] items, string responseText)
        {
            Items = items;
            ResponseText = responseText;
        }
    }
    
    public enum NpcStates
    {
        BeforeGivingQuest = 0,
        AfterGivingQuest = 1,
        AfterQuestComplete = 2
    }
}