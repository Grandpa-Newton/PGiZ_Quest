using System;
using QuestGame.Graphics;

namespace QuestGame.Logic
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
        private Func<MainInventoryItem, bool> _interactFunction;

        public MainInventoryItem(Sprite sprite, Func<MainInventoryItem, bool> interactFunction) : base(sprite)
        {
            this._interactFunction = interactFunction;
        }

        public bool Interact()
        {
            return _interactFunction.Invoke(this);
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