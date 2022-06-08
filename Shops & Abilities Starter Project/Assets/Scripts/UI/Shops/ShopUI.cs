using RPG.Shops;
using TMPro;
using UnityEngine;

namespace RPG.UI.Shops
{
    public class ShopUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI _shopName;
        
        private Shopper _shopper = null;
        private Shop _currentShop = null;

        private void Start()
        {
             _shopper = GameObject.FindGameObjectWithTag("Player").GetComponent<Shopper>();
            if(_shopper == null) return;

            _shopper.activeShopChange += ShopChanged;
            
            ShopChanged();
        }

        private void ShopChanged()
        {
            _currentShop = _shopper.GetActiveShop();
            gameObject.SetActive(_currentShop != null);
            
            if(_currentShop == null) return;
            _shopName.text = _currentShop.GetShopName();
        }

        public void Close()
        {
            _shopper.SetActiveShop(null);
        }
    }
}