using QuestGame;
using SharpDX.Windows;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http.Headers;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpDX.DirectInput;

namespace Lab01
{
    internal class Inventory<T> where T : Item
    {
        private List<InventoryItem<T>> _inventoryItems;

        private DirectX3DGraphics _directX3DGraphics;

        private DXInput _dxInput;

        private Vector2 _cursorPosition = Vector2.Zero;

        private RenderForm _renderForm;

        public bool IsFull;

        private SharpDX.Vector2 _lastPosition;

        public Inventory(DirectX3DGraphics directX3DGraphics, DXInput dxInput, InventoryItem<T>[] inventoryItems)
        {
            _directX3DGraphics = directX3DGraphics;

            _dxInput = dxInput;

            _renderForm = _directX3DGraphics.RenderForm;

            _inventoryItems = inventoryItems.ToList();

            for (int i = 0; i < inventoryItems.Length; i++)
            {
                if (inventoryItems[i] == null)
                {
                    IsFull = true;
                }
            }

            _lastPosition = inventoryItems[inventoryItems.Length - 1].CenterPosition;

        }


        public InventoryItem<T> GetActiveItem()
        {
            foreach (var item in _inventoryItems)
            {
                if (item.IsActive)
                {
                    return item;
                }
            }

            return null;
        }

        public void ExpanseInventory()
        {
            _inventoryItems.Add(new InventoryItem<T>(_directX3DGraphics, null, _lastPosition + new SharpDX.Vector2(100, 0), 
                0f, new SharpDX.Vector2(800, 600))); //TODO сделать какой-то файл с константами, по типу 800, 600 и т.п.
            _lastPosition = _lastPosition + new SharpDX.Vector2(100, 0);
        }
        public bool AddItem(T item)
        {
            if (IsFull)
            {
                return false;
            }
            else
            {
                for (int i = 0; i < _inventoryItems.Count; i++)
                {
                    if (_inventoryItems[i].Item == null)
                    {
                        _inventoryItems[i].ChangeItem(item);
                        return true;
                    }
                }

                return false;
            }
        }

        public void ChangeItem(T item, int index)
        {
            _inventoryItems[index].ChangeItem(item);
        }

        public void RemoveItem(T item)
        {
            for (int i = 0; i < _inventoryItems.Count; i++)
            {
                if (_inventoryItems[i].Item == item)
                {
                    _inventoryItems[i].Item = null;
                }
            }
        }

        public void DrawInventory()
        {
            System.Drawing.Point point = _renderForm.PointToClient(Cursor.Position);

            SharpDX.Vector2 cursorPosition = new SharpDX.Vector2(point.X, point.Y);

            foreach (InventoryItem<T> item in _inventoryItems)
            {
                item.DrawInventoryItem();
                item.CheckToHover(cursorPosition);
            }

            if (_dxInput.IsMouseButtonPressed(0))
            {
                foreach (var item in _inventoryItems)
                {
                    if (item.CheckToMakeActive(cursorPosition))
                    {
                        foreach (var secondItem in _inventoryItems)
                        {
                            secondItem.IsActive = false;
                        }

                        item.IsActive = true;
                        break;
                    }
                }
            }
        }
    }
}