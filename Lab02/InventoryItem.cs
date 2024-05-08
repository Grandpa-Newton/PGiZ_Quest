using QuestGame;
using SharpDX.Direct2D1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lab01;
using ObjLoader.Loader.Common;
using QuestGame.Graphics;
using QuestGame.Infrastructure;
using SharpDX;

namespace QuestGame.Logic
{
    internal class InventoryItem<T> where T : Item
    {
        public T Item;

        public bool IsActive = false;
        public bool IsHovered = false;

        private float _itemScale = 0.5f;

        private Sprite _activeBox;
        private Sprite _inactiveBox;

        private DirectX3DGraphics _directX3DGraphics;

        private Bitmap _activeBoxBitmap;

        private float _defaultBoxScale;

        public Vector2 CenterPosition;


        public InventoryItem(DirectX3DGraphics directX3DGraphics, T item, Vector2 centerPosition, float angle,
            Vector2 defaultSize, float defaultBoxScale = 1f, float defaultItemScale = 1f)
        {
            _directX3DGraphics = directX3DGraphics;


            if (item != null)
            {
                Item = item;
            }

            Bitmap activeBoxBitmap = DirectX3DGraphics.LoadFromFile(directX3DGraphics.D2DRenderTarget, "activeBox.bmp");

            _activeBoxBitmap = activeBoxBitmap;

            float boxSize = 2.5f;

            _activeBox = new Sprite(directX3DGraphics, activeBoxBitmap, centerPosition, angle,
                defaultSize, boxSize * defaultBoxScale);

            Bitmap inactiveBoxBitmap =
                DirectX3DGraphics.LoadFromFile(directX3DGraphics.D2DRenderTarget, "inactiveBox.bmp");

            _inactiveBox = new Sprite(directX3DGraphics, inactiveBoxBitmap, centerPosition, angle,
                defaultSize, boxSize * defaultBoxScale);

            _defaultBoxScale = defaultBoxScale;

            CenterPosition = centerPosition;
        }

        public void DrawInventoryItem()
        {
            if (IsActive || IsHovered)
            {
                _activeBox.Draw(1.0f);
            }
            else
            {
                _inactiveBox.Draw(1.0f);
            }

            if (Item != null)
                Item.Sprite.Draw(1.0f);
        }

        public void CheckToHover(Vector2 position)
        {
            IsHovered = CheckPosition(position);
        }

        public bool CheckToMakeActive(Vector2 position)
        {
            bool isToMakeActive = CheckPosition(position);

            IsActive = isToMakeActive || IsActive;

            return isToMakeActive;
        }


        private bool CheckPosition(Vector2 position)
        {
            float width = _directX3DGraphics.D2DRenderTarget.Size.Width / _activeBox.DefaultSize.X;

            Vector2 point1 = _activeBox.Translation;
            Vector2 point2 = _inactiveBox.Translation +
                             new Vector2(_activeBoxBitmap.Size.Width * width, _activeBoxBitmap.Size.Height * width) *
                             2.5f * _defaultBoxScale;

            if ((position.X >= point1.X && position.X <= point2.X) &&
                (position.Y >= point1.Y && position.Y <= point2.Y))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void ChangeItem(T item)
        {
            Item = item;
            Item.Sprite.CenterPosition = _activeBox.CenterPosition;
            Item.Sprite.DefaultSize = _activeBox.DefaultSize;
            Item.Sprite.DefaultScale *= _defaultBoxScale;
        }
    }
}