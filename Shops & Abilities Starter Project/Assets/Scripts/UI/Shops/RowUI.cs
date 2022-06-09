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
        
        public void SetUp(ShopItem item)
        {
            _nameField.text = item.GetName();
            _ico.sprite = item.GetIco(); 
            _availability.text = $"{item.GetAvailability()}";
            _price.text = $"{item.GetPrice():N2}";
        }
    }
}