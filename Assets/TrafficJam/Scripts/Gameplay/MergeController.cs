using UnityEngine.InputSystem;
using UnityEngine;
using TrafficJam.Core;
using DG.Tweening;

namespace TrafficJam.Gameplay
{
    // tr: Oyuncunun araçları sürükleyip birleştirmesini sağlayan kontrolcü sınıfı.
    public class MergeController : MonoBehaviour
    {
        [Header("Settings")]
        public LayerMask carLayer;
        public float dragHeight = 1.0f;

        private GameObject draggedObject;
        private Vector3 originalPosition;
        private CarAgent draggedAgent;
        private Vector3 dragOffset;

        private void Update()
        {
            if (GameManager.Instance == null) return;
            if (GameManager.Instance.CurrentState != GameState.Playing) return;

            HandleInput();
        }

        private void HandleInput()
        {
            if (Pointer.current == null) return;

            if (Pointer.current.press.wasPressedThisFrame)
                StartDragging();

            if (Pointer.current.press.isPressed && draggedObject != null)
                UpdateDragging();

            if (Pointer.current.press.wasReleasedThisFrame && draggedObject != null)
                StopDragging();
        }

        private void StartDragging()
        {
            Vector2 screenPos = Pointer.current.position.ReadValue();
            Ray ray = Camera.main.ScreenPointToRay(screenPos);

            if (!Physics.Raycast(ray, out RaycastHit hit, 100f, carLayer))
                return;

            draggedObject = hit.collider.gameObject;
            draggedAgent = draggedObject.GetComponent<CarAgent>();

            if (draggedAgent == null)
            {
                draggedObject = null;
                return;
            }

            draggedAgent.SetDraggingState(true);
            EventManager.OnDragStarted?.Invoke(draggedAgent.carData.tier, draggedObject);

            originalPosition = draggedObject.transform.position;
            dragOffset = draggedObject.transform.position - hit.point;
            dragOffset.y = 0;

            // tr: Game feel — yumuşak yükselme + hafif scale arttırma.
            Transform t = draggedObject.transform;
            t.DOKill();
            Sequence pickSeq = DOTween.Sequence();
            pickSeq.Join(t.DOMoveY(dragHeight, 0.18f).SetEase(Ease.OutQuad));
            pickSeq.Join(t.DOScale(1.08f, 0.18f).SetEase(Ease.OutBack));
        }

        private void UpdateDragging()
        {
            Vector2 screenPos = Pointer.current.position.ReadValue();
            Ray ray = Camera.main.ScreenPointToRay(screenPos);

            Plane plane = new Plane(Vector3.up, new Vector3(0, dragHeight, 0));
            if (plane.Raycast(ray, out float entry))
            {
                Vector3 hitPoint = ray.GetPoint(entry);
                Vector3 newPos = hitPoint + dragOffset;
                newPos.y = dragHeight;
                draggedObject.transform.position = newPos;
            }
        }

        private void StopDragging()
        {
            Vector2 screenPos = Pointer.current.position.ReadValue();
            Ray ray = Camera.main.ScreenPointToRay(screenPos);

            RaycastHit[] hits = Physics.RaycastAll(ray, 100f, carLayer);
            GameObject targetObject = null;

            foreach (var hit in hits)
            {
                if (hit.collider.gameObject != draggedObject)
                {
                    targetObject = hit.collider.gameObject;
                    break;
                }
            }

            if (targetObject != null)
            {
                CheckMerge(targetObject);
            }
            else
            {
                // tr: Uyumsuz yere bırakıldıysa eski pozisyona yumuşak dönüş.
                Transform t = draggedObject.transform;
                t.DOKill();
                Sequence dropBackSeq = DOTween.Sequence();
                dropBackSeq.Join(t.DOMove(originalPosition, 0.25f).SetEase(Ease.OutQuad));
                dropBackSeq.Join(t.DOScale(1f, 0.2f).SetEase(Ease.OutQuad));
            }

            draggedAgent.SetDraggingState(false);
            EventManager.OnDragEnded?.Invoke();

            draggedObject = null;
            draggedAgent = null;
        }

        private void CheckMerge(GameObject target)
        {
            CarAgent targetAgent = target.GetComponent<CarAgent>();

            if (targetAgent == null || draggedAgent == null)
            {
                draggedObject.transform.DOMove(originalPosition, 0.3f);
                return;
            }

            int draggedTier = draggedAgent.carData.tier;
            int targetTier = targetAgent.carData.tier;

            if (draggedTier == targetTier)
            {
                PerformMerge(targetAgent, targetTier);
            }
            else
            {
                draggedObject.transform.DOMove(originalPosition, 0.3f);
            }
        }

        private void PerformMerge(CarAgent targetAgent, int currentTier)
        {
            int nextTier = currentTier + 1;
            string newPoolId = $"Car_Tier{nextTier}";
            Vector3 spawnPos = targetAgent.transform.position;

            // tr: Bu araçlar trafikten geldiyse kapasite hesabı doğru kalsın.
            if (TrafficManager.Instance != null)
            {
                TrafficManager.Instance.RemoveCarFromActive(draggedObject);
                TrafficManager.Instance.RemoveCarFromActive(targetAgent.gameObject);
            }

            ObjectPoolManager.Instance.ReturnToPool(draggedAgent.carData.poolId, draggedObject);
            ObjectPoolManager.Instance.ReturnToPool(targetAgent.carData.poolId, targetAgent.gameObject);

            GameObject newCar = ObjectPoolManager.Instance.SpawnFromPool(newPoolId, spawnPos, Quaternion.identity);

            if (newCar != null)
            {
                newCar.transform.localScale = Vector3.zero;
                newCar.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
                // tr: Tier2 (ve üstü) araç merge sonrası rotaya oturtulmazsa olduğu yerde kalır. Yolu verip hareketi başlat.
                CarAgent newAgent = newCar.GetComponent<CarAgent>();
                if (newAgent != null)
                    newAgent.InitializePath();
                // tr: Yeni araç da yolda sayılsın (kapasite/spawn hesabı için).
                if (TrafficManager.Instance != null)
                    TrafficManager.Instance.AddCarToActive(newCar);
                EventManager.OnCarMerged?.Invoke(nextTier);
            }
            else
            {
                Debug.LogError($"[MergeController] Pool '{newPoolId}' returned null!");
            }
        }
    }
}
