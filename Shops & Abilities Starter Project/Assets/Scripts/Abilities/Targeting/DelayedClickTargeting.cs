using System;
using System.Collections;
using System.Collections.Generic;
using RPG.Control;
using UnityEngine;

namespace RPG.Abilities.Targeting
{
    [CreateAssetMenu(fileName = "DelayedClickTargeting", menuName = "Abilities/Targeting/DelayedClick",order = 0)]
    public class DelayedClickTargeting : TargetingStrategy
    {
        [SerializeField] Texture2D _cursorTexture;
        [SerializeField] Vector2 _cursorHotspot;
        [SerializeField] LayerMask layerMask;
        [SerializeField] float areaAffectRadius;

        public override void StartTargeting(GameObject user, Action<IEnumerable<GameObject>> finished)
        {
            var playerController = user.GetComponent<PlayerController>();
            playerController.StartCoroutine(Targeting(user, playerController,finished));
        }

        private IEnumerator Targeting(GameObject user, PlayerController playerController, Action<IEnumerable<GameObject>> finished)
        {
            playerController.enabled = false;
            while (true)
            {
                Cursor.SetCursor(_cursorTexture,_cursorHotspot,CursorMode.Auto);

                if (Input.GetMouseButtonDown(0))
                {
                    yield return new WaitWhile(() => Input.GetMouseButton(0));
                    playerController.enabled = true;
                    finished(GetGameObjectInRadius());
                    yield break;
                }
                yield return null;
            }
        }

        private IEnumerable<GameObject> GetGameObjectInRadius()
        {
            RaycastHit raycastHit;
            if (Physics.Raycast(PlayerController.GetMouseRay(),out raycastHit,1000,layerMask))
            {
                var hits = Physics.SphereCastAll(raycastHit.point, areaAffectRadius, Vector3.up, 0);
                foreach (var hit in hits)
                {
                    yield return hit.collider.gameObject;
                }
            }
        }
    }
}