using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GameDevTV.Inventories;
using RPG.Control;
using RPG.Inventories;
using RPG.Stats;
using UnityEngine;

namespace RPG.Shops
{
    public class Shop : MonoBehaviour, IRaycastable
    {
        [SerializeField] private string _shopName;
        [Range(0,100)]
        [SerializeField] private float _sellingDiscountPercentage = 80f;
        
        [SerializeField] private StockItemConfig[] _stockItemConfigs;
        
        [Serializable]
        class StockItemConfig
        {
            public InventoryItem Item;
            public int InitialStock;
            [Range(0, 100)] public float BuyingDiscountPercentage;
            public int levleToUnlock = 0;
        }

        Dictionary<InventoryItem,int> _transaction = new Dictionary<InventoryItem, int>();
        Dictionary<InventoryItem,int> _stockSold = new Dictionary<InventoryItem, int>();
        private Shopper _curShoppper;
        private bool _isBuyingMode = true;
        private ItemCategory _filter = ItemCategory.None;
        public event Action onChange;

        public void SetShopper(Shopper shopper)
        {
            _curShoppper = shopper;
        }
        
        public IEnumerable<ShopItem> GetFilteredItems()
        {
            return GetAllItems().Where(x =>_filter == ItemCategory.None || x.GetInventoryItem().GetCategory() == _filter);
        }
        
        public IEnumerable<ShopItem> GetAllItems()
        {
            Dictionary<InventoryItem, float> prices = GetPrices();
            Dictionary<InventoryItem, int> availabilities = GetAvailabilities();//TODO 还原上一版
            
            foreach (var item in availabilities.Keys)
            {
                if(availabilities[item] <= 0) continue;

                var price = prices[item];
                _transaction.TryGetValue(item, out var quantityInTransaction);
                var availability = availabilities[item];
                yield return new ShopItem(item,availability,price,quantityInTransaction);
            }
        }

        private Dictionary<InventoryItem, int> GetAvailabilities()
        {
            var availabilities = new Dictionary<InventoryItem, int>();

            foreach (var config in GetAvailableConfigs())
            {
                if (_isBuyingMode)
                {
                    if (!availabilities.ContainsKey(config.Item))
                    {
                        var soldNum = 0;
                    
                        _stockSold.TryGetValue(config.Item, out soldNum);
                        availabilities[config.Item] = -soldNum;
                    }

                    availabilities[config.Item] += config.InitialStock;
                }
                else
                {
                    availabilities[config.Item] = CountItemsInInventory(config.Item);
                }
            }

            return availabilities;
        }

        private Dictionary<InventoryItem, float> GetPrices()
        {
            var prices = new Dictionary<InventoryItem,float>();

            foreach (var config in GetAvailableConfigs())
            {
                if (_isBuyingMode)
                {
                    if (!prices.ContainsKey(config.Item))
                    {
                        prices[config.Item] = config.Item.GetPrice();
                    }

                    prices[config.Item] *= (1 - config.BuyingDiscountPercentage / 100);
                }
                else
                {
                    prices[config.Item] = config.Item.GetPrice() * (_sellingDiscountPercentage / 100);
                }
                
            }

            return prices;
        }
        
        private IEnumerable<StockItemConfig> GetAvailableConfigs()
        {
            var shopperLevel = GetShopperLevel();
            foreach (var config in _stockItemConfigs)
            {
                if (config.levleToUnlock > shopperLevel)
                {
                    continue;
                }

                yield return config;
            }
        }

        private int GetShopperLevel()
        {
            var stats = _curShoppper.GetComponent<BaseStats>();
            if (stats ==null) return 0;

            return stats.GetLevel();
        }

        

        private int CountItemsInInventory(InventoryItem item)
        {
            var inventory = _curShoppper.GetComponent<Inventory>();
            if (inventory == null) return 0;

            var result = 0;
            for (int i = 0; i < inventory.GetSize(); i++)
            {
                if (inventory.GetItemInSlot(i) == item)
                {
                    result += inventory.GetNumberInSlot(i);
                }
            }

            return result;
        }

        // private float GetPrice(StockItemConfig config)
        // {
        //     if (_isBuyingMode)
        //     {
        //         return config.Item.GetPrice() * (1 - config.BuyingDiscountPercentage / 100);
        //     }
        //
        //     return config.Item.GetPrice() * (_sellingDiscountPercentage / 100);
        // }
        // private int GetAvailability(InventoryItem item)
        // {
        //     if (_isBuyingMode)
        //     {
        //         return 0;
        //     }
        //     else
        //     {
        //         return CountItemsInInventory(item);
        //     }
        // }

        public void SelectFilter(ItemCategory category)
        {
            _filter = category;
            onChange?.Invoke();
        }

        public ItemCategory GetFilter()
        {
            return _filter;
        }

        public void SelectMode(bool isBuying)
        {
            _isBuyingMode = isBuying;
            onChange?.Invoke();
        }

        public bool IsBuyingMode()
        {
            return _isBuyingMode;
        }

        public bool CanTransact()
        {
            if (IsTransactionEmpty()) return false;
            if (!HasSufficientFunds()) return false;
            if (!HasInventorySpace()) return false;
            
            return true;
        }

        public bool HasInventorySpace()
        {
            if (!_isBuyingMode) return true;
            
            var shopperInventory = _curShoppper.GetComponent<Inventory>();
            if (shopperInventory == null) return false;

            var flaItems = new List<InventoryItem>();
            foreach (var shopItem in GetAllItems())
            {
                var item = shopItem.GetInventoryItem();
                var quantity = shopItem.GetQuantityInTransaction();
                for (int i = 0; i < quantity; i++)
                {
                    flaItems.Add(item);
                }
            }

            return shopperInventory.HasSpaceFor(flaItems);
        }

        public bool IsTransactionEmpty()
        {
            return _transaction.Count == 0;
        }

        public bool HasSufficientFunds()
        {
            if (!_isBuyingMode) return true;
            
            var shopperPurse = _curShoppper.GetComponent<Purse>();
            if (shopperPurse == null) return false;
            return shopperPurse.GetBalance() >= TransactionTotal();
        }

        public void ConfirmTransaction()
        {
            Inventory shopperInventory = _curShoppper.GetComponent<Inventory>();
            Purse shopperPurse = _curShoppper.GetComponent<Purse>();
            
            if (shopperInventory == null || shopperPurse == null) return;

            foreach (var shopItem in GetAllItems())
            {
                var item = shopItem.GetInventoryItem();
                var quantity = shopItem.GetQuantityInTransaction();
                var price = shopItem.GetPrice();
                for (int i = 0; i < quantity; i++)
                {
                    if (_isBuyingMode)
                    {
                        if (BuyItem(shopperPurse, price, shopperInventory, item)) break;
                    }
                    else
                    {
                        SellItem(shopperPurse, price, shopperInventory, item);
                    }
                }
            }
            
            onChange?.Invoke();
        }

        private void SellItem(Purse shopperPurse, float price, Inventory shopperInventory, InventoryItem item)
        {
            var slot = FindFirstItemSlot(shopperInventory, item);
            if(slot == -1) return;
            
            shopperInventory.RemoveFromSlot(slot,1);
            AddToTransaction(item, -1);
            if (!_stockSold.ContainsKey(item))
            {
                _stockSold[item] = 0;
            }
            _stockSold[item]--;
            shopperPurse.UpdateBalance(price);
            
        }

        private int FindFirstItemSlot(Inventory shopperInventory, InventoryItem item)
        {
            for (int i = 0; i < shopperInventory.GetSize(); i++)
            {
                if (shopperInventory.GetItemInSlot(i) == item)
                {
                    return i;
                }
            }

            return -1;
        }

        private bool BuyItem(Purse shopperPurse, float price, Inventory shopperInventory, InventoryItem item)
        {
            if (shopperPurse.GetBalance() < price) return true;

            bool success = shopperInventory.AddToFirstEmptySlot(item, 1);
            if (success)
            {
                AddToTransaction(item, -1);
                if (!_stockSold.ContainsKey(item))
                {
                    _stockSold[item] = 0;
                }
                _stockSold[item]++;
                shopperPurse.UpdateBalance(-price);
            }

            return false;
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

            var availabilities = GetAvailabilities();
            var availability = availabilities[item];
            if (_transaction[item] + quantity > availability)
            {
                _transaction[item] = availability;
            }
            else
            {
                _transaction[item] += quantity;
            }

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