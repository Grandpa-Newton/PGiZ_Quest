using System;

namespace Lab01
{
    internal abstract class Item
    {
        public Item(Sprite sprite)
        {
            Sprite = sprite;
        }
        public Sprite Sprite;
    }

    internal class MainInventoryItem : Item
    {
        private Func<MainInventoryItem, bool> interactFunction;
        public MainInventoryItem(Sprite sprite, Func<MainInventoryItem, bool> interactFunction) : base(sprite)
        {
            this.interactFunction = interactFunction;
        }
        public bool Interact()
        {
            return interactFunction.Invoke(this);
        }
    }
    
    internal class CollectibleItem : Item
    {
        public string Description;
        public CollectibleItem(Sprite sprite, string description) : base(sprite)
        {
            Description = description;
        }
    }
}