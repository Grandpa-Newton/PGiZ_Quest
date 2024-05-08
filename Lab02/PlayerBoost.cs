namespace QuestGame.Logic
{
    public class PlayerBoost
    {
        public float SpeedMultiplier;
        public int InventoryExpansion;

        public PlayerBoost(float speedMultiplier, int inventoryExpansion)
        {
            SpeedMultiplier = speedMultiplier;
            InventoryExpansion = inventoryExpansion;
        }
    }
}