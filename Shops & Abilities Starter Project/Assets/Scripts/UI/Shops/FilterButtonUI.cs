using System;
using GameDevTV.Inventories;
using RPG.Shops;
using UnityEngine;
using UnityEngine.UI;

namespace RPG.UI.Shops
{
    public class FilterButtonUI : MonoBehaviour
    {
        [SerializeField] private ItemCategory _category = ItemCategory.None;
        private Button _button;
        private Shop _currentShop;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(SelectFilter);
        }

        public void SetShop(Shop currentShop)  //TODO子物体调用父物体，考虑使用委托;
        {
            _currentShop = currentShop;
        }

        public void RefreshUI()
        {
            _button.interactable = _currentShop.GetFilter() != _category;
        }

        private void SelectFilter()
        {
            _currentShop.SelectFilter(_category);
        }
    }
}