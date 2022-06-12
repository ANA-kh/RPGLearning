using System;
using GameDevTV.Saving;
using UnityEngine;

namespace RPG.Inventories
{
    public class Purse :MonoBehaviour ,ISaveable
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

        public object CaptureState()
        {
            return _balance;
        }

        public void RestoreState(object state)
        {
            _balance = (float) state;
        }
    }
}