using System.Collections.Generic;
using TrafficJam.Core;
using TrafficJam.Data;
using UnityEngine;
using DG.Tweening;

namespace TrafficJam.Gameplay
{
    // tr: Havuzdan spawn edilen araçların rotayı takip edip ilerlemesini sağlayan agent scripti.
    public class CarAgent : MonoBehaviour
    {
        [Header("Data References")]
        public CarDataSO carData;

        private const float DistanceThreshold = 0.2f;

        private List<Transform> waypoints;
        private int currentWaypointIndex = 0;
        private bool isMoving = false;
        private bool isDragging = false;

        [Header("Visuals")]
        [SerializeField] private GameObject highlightIndicator;
        [SerializeField, Range(1f, 3f)] private float highlightScaleMultiplier = 1.6f;

        private Vector3 _defaultHighlightScale;

        private void OnEnable()
        {
            currentWaypointIndex = 0;
            isMoving = false;

            // tr: PathManager hazır değilse sessizce bekle, TrafficManager zaten yol olmadan spawn etmez.
            if (PathManager.Instance == null || PathManager.Instance.GetWaypoints().Count == 0)
                return;

            if (highlightIndicator != null)
            {
                _defaultHighlightScale = highlightIndicator.transform.localScale == Vector3.zero
                    ? Vector3.one
                    : highlightIndicator.transform.localScale;
                highlightIndicator.SetActive(false);
            }

            EventManager.OnDragStarted += HandleDragStarted;
            EventManager.OnDragEnded += HandleDragEnded;
        }

        private void OnDisable()
        {
            EventManager.OnDragStarted -= HandleDragStarted;
            EventManager.OnDragEnded -= HandleDragEnded;
        }

        // tr: TrafficManager spawn sonrasında bu metodu çağırarak aracı rotaya oturtur.
        public void InitializePath()
        {
            if (PathManager.Instance == null || PathManager.Instance.GetWaypoints().Count == 0)
                return;

            waypoints = PathManager.Instance.GetWaypoints();
            currentWaypointIndex = 0;
            transform.position = waypoints[0].position;

            if (waypoints.Count > 1)
                transform.LookAt(waypoints[1]);

            isMoving = true;
        }

        private void Update()
        {
            if (!isMoving || isDragging) return;
            if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing) return;
            if (carData == null)
            {
                // tr: Bu genelde prefab'da CarData referansı unutulduğunda olur (Tier2 merge sonrası en sık).
                // Araba hata fırlatmasın diye hareket etmez; burada net loglayalım.
                Debug.LogWarning($"[CarAgent] carData is NULL on '{name}'. Vehicle will not move. Assign correct CarDataSO on prefab.");
                return;
            }

            if (waypoints == null || waypoints.Count == 0) return;

            MoveTowardsNextWaypoint();
        }

        private void MoveTowardsNextWaypoint()
        {
            Transform target = waypoints[currentWaypointIndex];

            transform.position = Vector3.MoveTowards(
                transform.position, target.position, carData.baseSpeed * Time.deltaTime);

            Vector3 direction = (target.position - transform.position).normalized;
            if (direction != Vector3.zero)
            {
                Quaternion endRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, endRotation, Time.deltaTime * 10f);
            }

            if (Vector3.Distance(transform.position, target.position) < DistanceThreshold)
            {
                currentWaypointIndex++;

                if (currentWaypointIndex >= waypoints.Count)
                {
                    CompleteLap();
                }
            }
        }

        private void CompleteLap()
        {
            EventManager.OnCarCompletedLap?.Invoke(carData.incomePerLap);

            // tr: Aktif araç listesinden çıkar ve havuza geri gönder.
            if (TrafficManager.Instance != null)
                TrafficManager.Instance.RemoveCarFromActive(gameObject);

            ObjectPoolManager.Instance.ReturnToPool(carData.poolId, gameObject);
        }

        public void SetDraggingState(bool state)
        {
            isDragging = state;
        }

        private void HandleDragStarted(int draggedTier, GameObject draggedObj)
        {
            if (draggedObj == gameObject) return;

            // tr: Aynı seviyedeki araçlar ışıklarını yakıp nefes alma animasyonu oynatır.
            if (carData.tier == draggedTier && highlightIndicator != null)
            {
                highlightIndicator.SetActive(true);
                highlightIndicator.transform.DOScale(new Vector3(1.8f, 1.8f, 1.8f), 0.5f)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetEase(Ease.InOutSine);
            }
        }

        private void HandleDragEnded()
        {
            if (highlightIndicator != null)
            {
                highlightIndicator.transform.DOKill();
                highlightIndicator.transform.localScale = _defaultHighlightScale;
                highlightIndicator.SetActive(false);
            }
        }
    }
}
