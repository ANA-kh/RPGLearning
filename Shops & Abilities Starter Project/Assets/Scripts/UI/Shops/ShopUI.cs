using RPG.Shops;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RPG.UI.Shops
{
    public class ShopUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _shopName;
        [SerializeField] private Transform _listRoot;
        [SerializeField] private RowUI _rowPrefab;
        [SerializeField] private TextMeshProUGUI _totalText;
        [SerializeField] private Button _confirmButton;
        [SerializeField] private Button _switchButton;
        
        private Shopper _shopper = null;
        private Shop _currentShop = null;
        private Color _originalTotalTextColor;

        private void Start()
        {
            _originalTotalTextColor = _totalText.color;
             _shopper = GameObject.FindGameObjectWithTag("Player").GetComponent<Shopper>();
            if(_shopper == null) return;

            _shopper.activeShopChange += ShopChanged;
            _confirmButton.onClick.AddListener(ConfirmTransaction);
            _switchButton.onClick.AddListener(SwitchMode);
            
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
            
            foreach (var buttonUI in GetComponentsInChildren<FilterButtonUI>())
            {
                buttonUI.SetShop(_currentShop);
            }
            
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

            _totalText.text = $"Total : ${_currentShop.TransactionTotal():N2}";
            _totalText.color = _currentShop.HasSufficientFunds() ? _originalTotalTextColor : Color.red;
            _confirmButton.interactable = _currentShop.CanTransact();

            var switchText = _switchButton.GetComponentInChildren<TextMeshProUGUI>();
            var confirmText = _confirmButton.GetComponentInChildren<TextMeshProUGUI>();
            if (_currentShop.IsBuyingMode())
            {
                switchText.text = "Switch To Selling";
                confirmText.text = "Buy";
            }
            else
            {
                switchText.text = "Switch To Buying";
                confirmText.text = "Sell";
            }
            
            foreach (var buttonUI in GetComponentsInChildren<FilterButtonUI>())
            {
                buttonUI.RefreshUI();
            }
        }

        public void Close()
        {
            _shopper.SetActiveShop(null);
        }

        public void ConfirmTransaction()
        {
            _currentShop.ConfirmTransaction();
        }

        public void SwitchMode()
        {
            _currentShop.SelectMode(!_currentShop.IsBuyingMode());
        }
    }
}