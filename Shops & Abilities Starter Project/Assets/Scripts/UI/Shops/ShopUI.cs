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
        [SerializeField] private TextMeshProUGUI _total;
        
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
            if (_currentShop != null)
            {
                _currentShop.onChange -= RefreshUI;
            }
            _currentShop = _shopper.GetActiveShop();
            gameObject.SetActive(_currentShop != null);
            
            if(_currentShop == null) return;
            _shopName.text = _currentShop.GetShopName();
            _currentShop.onChange += RefreshUI;
            RefreshUI();
        }

        private void RefreshUI()
        {
            foreach (Transform child in _listRoot)
            {
                Destroy(child.gameObject);//对象池   bug
            }

            foreach (var item in _currentShop.GetFilteredItems())//TODO 把shopui看作view的话，考虑不要持有shop，让数据从Refreshui传过来，更新界面
            {
                var row = Instantiate(_rowPrefab, _listRoot);
                row.SetUp(_currentShop,item);
            }

            _total.text = $"Total : ${_currentShop.TransactionTotal():N2}";
        }

        public void Close()
        {
            _shopper.SetActiveShop(null);
        }

        public void ConfirmTransaction()
        {
            _currentShop.ConfirmTransaction();
        }
    }
}