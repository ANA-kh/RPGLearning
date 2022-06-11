using System;
using RPG.Inventories;
using TMPro;
using UnityEngine;

namespace RPG.UI
{
    public class PurseUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI _balanceField;

        private Purse _playerPouse = null;

        private void Start()
        {
            _playerPouse = GameObject.FindGameObjectWithTag("Player").GetComponent<Purse>();

            if (_playerPouse != null)
            {
                _playerPouse.onChange += RefreshUI;
            }
            
            RefreshUI();
        }

        private void RefreshUI()
        {
            _balanceField.text = $"${_playerPouse.GetBalance():N2}";
        }

        private void OnDestroy()
        {
            if (_playerPouse != null)
            {
                _playerPouse.onChange -= RefreshUI;
            }
        }
    }
}