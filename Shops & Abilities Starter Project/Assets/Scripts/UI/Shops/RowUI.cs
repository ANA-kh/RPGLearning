using RPG.Shops;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RPG.UI.Shops
{
    public class RowUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _nameField;
        [SerializeField] private Image _ico;
        [SerializeField] private TextMeshProUGUI _availability;
        [SerializeField] private TextMeshProUGUI _price;
        [SerializeField] private TextMeshProUGUI _quantity;
        private ShopItem _item;
        private Shop _currentShop;

        public void SetUp(Shop currentShop,ShopItem item)
        {
            this._currentShop = currentShop;
            this._item = item;
            _nameField.text = item.GetName();
            _ico.sprite = item.GetIco(); 
            _availability.text = $"{item.GetAvailability()}";
            _price.text = $"{item.GetPrice():N2}";
            _quantity.text = $"{item.GetQuantityInTransaction()}";
        }

        public void Add()
        {
            _currentShop.AddToTransaction(_item.GetInventoryItem(),1);
        }
        public void Remove()
        {
            _currentShop.AddToTransaction(_item.GetInventoryItem(),-1);
        }
    }
}