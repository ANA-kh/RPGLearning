using GameDevTV.Inventories;
using UnityEngine;

namespace RPG.Shops
{
    public class ShopItem
    {
        InventoryItem _item;
        int _availability;
        float _price;
        private int _quantityInTransaction;

        public ShopItem(InventoryItem item, int availability, float price, int quantityInTransaction)
        {
            _item = item;
            _availability = availability;
            _price = price;
            _quantityInTransaction = quantityInTransaction;
        }

        public string GetName()
        {
            return _item.GetDisplayName();
        }

        public float GetPrice()
        {
            return _price;
        }

        public int GetAvailability()
        {
            return _availability;
        }

        public Sprite GetIco()
        {
            return _item.GetIcon();
        }

        public InventoryItem GetInventoryItem()
        {
            return _item;
        }

        public int GetQuantityInTransaction()
        {
            return _quantityInTransaction;
        }
    }
}