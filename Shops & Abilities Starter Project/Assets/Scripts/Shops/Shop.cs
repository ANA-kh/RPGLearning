using System;
using System.Collections.Generic;
using GameDevTV.Inventories;
using RPG.Control;
using UnityEngine;

namespace RPG.Shops
{
    public class Shop : MonoBehaviour, IRaycastable
    {
        [SerializeField] private string _shopName;
        public event Action onChange;
        [SerializeField] private StockItemConfig[] _stockItemConfigs;
        
        [Serializable]
        class StockItemConfig
        {
            public InventoryItem Item;
            public int InitialStock;
            [Range(0, 100)] public float BuyingDiscountPercentage;
        }

        public IEnumerable<ShopItem> GetFilteredItems()
        {
            foreach (var config in _stockItemConfigs)
            {
                var price = config.Item.GetPrice() * (1 - config.BuyingDiscountPercentage / 100);
                yield return new ShopItem(config.Item,config.InitialStock,price,0);
            }
        }

        public void SelectFilter(ItemCategory category){}

        public ItemCategory GetFilter()
        {
            return ItemCategory.None;
        }
        
        public void SelectMode(bool isBuying){}

        public bool IsBuyingMode()
        {
            return true;
        }

        public bool CanTransact()
        {
            return true;
        }
        public void ConfirmTransaction()
        {
        }

        public float TransactionTotal()
        {
            return 0;
        }
        public void AddToTransaction(InventoryItem item, int quantity){}
        public CursorType GetCursorType()
        {
            return CursorType.Shop;
        }

        public bool HandleRaycast(PlayerController callingController)
        {
            if (Input.GetMouseButtonDown(0))
            {
                callingController.GetComponent<Shopper>().SetActiveShop(this);
            }

            return true;
        }

        public string GetShopName()
        {
            return _shopName;
        }
    }
}