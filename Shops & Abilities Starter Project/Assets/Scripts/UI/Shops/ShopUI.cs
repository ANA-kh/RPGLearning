using RPG.Shops;
using TMPro;
using UnityEngine;

namespace RPG.UI.Shops
{
    public class ShopUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI _shopName;
        [SerializeField] private Transform _listRoot;
        [SerializeField] private RowUI _rowPrefab;
        
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

            RefreshUI();
        }

        private void RefreshUI()
        {
            foreach (Transform child in _listRoot)
            {
                Destroy(child.gameObject);//对象池   bug
            }

            foreach (var item in _currentShop.GetFilteredItems())
            {
                var row = Instantiate(_rowPrefab, _listRoot);
                row.SetUp(item);
            }
        }

        public void Close()
        {
            _shopper.SetActiveShop(null);
        }
    }
}