using System.Collections;
using System.Collections.Generic;
using GameDevTV.Inventories;
using UnityEngine;

namespace RPG.Abilities
{
    [CreateAssetMenu(fileName = "My Ability", menuName = "Abilities/Ability", order = 0)]
    public class Ability : ActionItem
    {
        [SerializeField] private TargetingStrategy _targetingStrategy;

        public override void Use(GameObject user)
        {
            _targetingStrategy.StartTargeting(user, TargetAcquired);
        }

        private void TargetAcquired(IEnumerable<GameObject> targets)
        {
            foreach (var gameObject in targets)
            {
                Debug.Log($"{gameObject.name}");
            }
        }
    }
}