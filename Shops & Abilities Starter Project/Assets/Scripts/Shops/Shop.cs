using System;
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
        Dictionary<InventoryItem,int> _stock = new Dictionary<InventoryItem, int>();
        private Shopper _curShoppper;
        private bool _isBuyingMode = true;
        private ItemCategory _filter = ItemCategory.None;
        public event Action onChange;

        private void Awake()
        {
            foreach (var config in _stockItemConfigs)
            {
                _stock[config.Item] = config.InitialStock;
            }
        }

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
            foreach (var config in _stockItemConfigs)
            {
                if (config.levleToUnlock <= GetShopperLevel())
                {
                    var price = GetPrice(config);
                    _transaction.TryGetValue(config.Item, out var quantityInTransaction);
                    var curStock = GetAvailability(config.Item);
                    yield return new ShopItem(config.Item,curStock,price,quantityInTransaction);    
                }
            }
        }
        
        private int GetShopperLevel()
        {
            var stats = _curShoppper.GetComponent<BaseStats>();
            if (stats ==null) return 0;

            return stats.GetLevel();
        }

        private int GetAvailability(InventoryItem item)
        {
            if (_isBuyingMode)
            {
                return _stock[item];
            }
            else
            {
                return CountItemsInInventory(item);
            }
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

        private float GetPrice(StockItemConfig config)
        {
            if (_isBuyingMode)
            {
                return config.Item.GetPrice() * (1 - config.BuyingDiscountPercentage / 100);
            }

            return config.Item.GetPrice() * (_sellingDiscountPercentage / 100);
        }

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
            _stock[item]++;
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
                _stock[item]--;
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

            var availability = GetAvailability(item);
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