using System;
using UnityEngine;

namespace RPG.Inventories
{
    public class Purse :MonoBehaviour
    {
        [SerializeField] private float _startingBalance = 400f;

        private float _balance = 0;

        public event Action onChange;

        private void Awake()
        {
            _balance = _startingBalance;
        }

        public float GetBalance()
        {
            return _balance;
        }

        public void UpdateBalance(float amount)
        {
            _balance += amount;
            onChange?.Invoke();
        }
    }
}