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
        
        [SerializeField] private StockItemConfig[] _stockItemConfigs;
        
        [Serializable]
        class StockItemConfig
        {
            public InventoryItem Item;
            public int InitialStock;
            [Range(0, 100)] public float BuyingDiscountPercentage;
        }

        Dictionary<InventoryItem,int> _transaction = new Dictionary<InventoryItem, int>();
        private Shopper _curShoppper;
        public event Action onChange;

        public void SetShopper(Shopper shopper)
        {
            _curShoppper = shopper;
        }
        
        public IEnumerable<ShopItem> GetFilteredItems()
        {
            return GetAllItems();
        }
        
        public IEnumerable<ShopItem> GetAllItems()
        {
            foreach (var config in _stockItemConfigs)
            {
                var price = config.Item.GetPrice() * (1 - config.BuyingDiscountPercentage / 100);
                _transaction.TryGetValue(config.Item, out var quantityInTransaction);
                yield return new ShopItem(config.Item,config.InitialStock,price,quantityInTransaction);
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
            Inventory shopperInventory = _curShoppper.GetComponent<Inventory>();
            if(shopperInventory == null) return;

            var transactionSnapshot = new Dictionary<InventoryItem,int>(_transaction); 
            foreach (var item in transactionSnapshot.Keys)
            {
                var quantity = _transaction[item];
                for (int i = 0; i < quantity; i++)
                {
                    bool success = shopperInventory.AddToFirstEmptySlot(item, 1);
                    if (success)
                    {
                        AddToTransaction(item,-1);
                    }
                }
                shopperInventory.AddToFirstEmptySlot(item, quantity);
            }
        }

        public float TransactionTotal()
        {
            float result = 0;
            foreach (var shopItem in GetAllItems())
            {
                if (_transaction.TryGetValue(shopItem.GetInventoryItem(),out var quantity))
                {
                    result += shopItem.GetPrice() * shopItem.GetQuantityInTransaction();
                }
            }

            return result;
        }

        public void AddToTransaction(InventoryItem item, int quantity)
        {
            if (!_transaction.ContainsKey(item))
            {
                _transaction[item] = 0;
            }

            _transaction[item] += quantity;

            if (_transaction[item] <= 0)
            {
                _transaction.Remove(item);
            }
            
            onChange?.Invoke();
        }
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