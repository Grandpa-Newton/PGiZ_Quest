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

        public PlayerBoost PlayerBoost;
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
                    npcResponse = new NpcResponse(TakenItems, InteractTextAfterComplete, PlayerBoost);
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
            string interactTextAfterComplete, ref Action questCompleteAction,
            MainInventoryItem[] givingItems = null, CollectibleItem[] givingCollectibles = null,
            MainInventoryItem[] takenItems = null, PlayerBoost playerBoost = null)
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

            PlayerBoost = playerBoost;
            
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
        public PlayerBoost PlayerBoost;
        

        public NpcResponse(MainInventoryItem[] items, string responseText, PlayerBoost playerBoost = null)
        {
            Items = items;
            ResponseText = responseText;
            this.PlayerBoost = playerBoost;
        }
    }
    
    public enum NpcStates
    {
        BeforeGivingQuest = 0,
        AfterGivingQuest = 1,
        AfterQuestComplete = 2
    }
}