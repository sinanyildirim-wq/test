using UnityEngine;
using TrafficJam.Core;
using DG.Tweening;

namespace TrafficJam.Gameplay
{
    // tr: Oyuncunun araçları sürükleyip birleştirmesini sağlayan kontrolcü sınıfı.
    public class MergeController : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private LayerMask carLayer; // tr: Raycast'in hangi layer'daki objeleri yakalayacağını belirler.
        [SerializeField] private float dragHeight = 1.0f; // tr: Sürükleme sırasında aracın yerden yüksekliği.

        private GameObject draggedObject; // tr: Şu an sürüklenen araç.
        private Vector3 originalPosition; // tr: Sürükleme başarısız olursa geri döneceği pozisyon.
        private CarAgent draggedAgent; // tr: Sürüklenen aracın agent bileşeni.

        private void Update()
        {
            if (GameManager.Instance.CurrentState != GameState.Playing) return;

            HandleInput();
        }

        private void HandleInput()
        {
            // tr: Mouse sol tık veya dokunma başladığında.
            if (Input.GetMouseButtonDown(0))
            {
                StartDragging();
            }
            
            // tr: Mouse sol tık veya dokunma devam ederken.
            if (Input.GetMouseButton(0) && draggedObject != null)
            {
                UpdateDragging();
            }

            // tr: Mouse sol tık veya dokunma bırakıldığında.
            if (Input.GetMouseButtonUp(0) && draggedObject != null)
            {
                StopDragging();
            }
        }

        private void StartDragging()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, carLayer))
            {
                draggedObject = hit.collider.gameObject;
                draggedAgent = draggedObject.GetComponent<CarAgent>();
                originalPosition = draggedObject.transform.position;

                // tr: Hareket halindeyken sürüklemeyi durdurmak için agent'ı deaktive edebiliriz veya durumunu değiştirebiliriz.
                // tr: Şimdilik basitçe transform kontrolünü elimize alıyoruz.
                
                Debug.Log($"[MergeController] tr: Sürükleme başladı: {draggedObject.name}");
                
                // tr: Sürüklenen objeyi görsel olarak biraz yukarı kaldır.
                draggedObject.transform.DOMoveY(dragHeight, 0.2f);
            }
        }

        private void UpdateDragging()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            // tr: Yerdeki hayali bir düzleme (Plane) göre pozisyonu güncelle.
            Plane plane = new Plane(Vector3.up, new Vector3(0, dragHeight, 0));
            
            if (plane.Raycast(ray, out float entry))
            {
                Vector3 newPos = ray.GetPoint(entry);
                draggedObject.transform.position = newPos;
            }
        }

        private void StopDragging()
        {
            Debug.Log($"[MergeController] tr: Sürükleme bırakıldı: {draggedObject.name}");

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            // tr: Bıraktığımız yerde başka bir araç var mı kontrol et.
            // tr: draggedObject'in kendi collider'ını IgnoreRaycast yapmaya gerek kalmaması için RaycastAll veya offset kullanabiliriz.
            
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
                // tr: Boşluğa bırakıldıysa eski yerine döndür.
                Debug.Log("[MergeController] tr: Boşluğa bırakıldı, eski pozisyona dönülüyor.");
                draggedObject.transform.DOMove(originalPosition, 0.3f);
            }

            draggedObject = null;
            draggedAgent = null;
        }

        private void CheckMerge(GameObject target)
        {
            CarAgent targetAgent = target.GetComponent<CarAgent>();
            
            if (targetAgent == null || draggedAgent == null) 
            {
                Debug.LogWarning("[MergeController] tr: Hedef veya sürüklenen objede CarAgent bulunamadı!");
                draggedObject.transform.DOMove(originalPosition, 0.3f);
                return;
            }

            int draggedTier = draggedAgent.carData.tier;
            int targetTier = targetAgent.carData.tier;

            Debug.Log($"[MergeController] tr: Birleşme kontrol ediliyor: {draggedObject.name} (Tier {draggedTier}) -> {target.name} (Tier {targetTier})");

            if (draggedTier == targetTier)
            {
                PerformMerge(targetAgent, targetTier);
            }
            else
            {
                Debug.Log("[MergeController] tr: Seviyeler uyuşmuyor, birleşme başarısız.");
                draggedObject.transform.DOMove(originalPosition, 0.3f);
            }
        }

        private void PerformMerge(CarAgent targetAgent, int currentTier)
        {
            int nextTier = currentTier + 1;
            string newPoolId = $"Car_Tier{nextTier}";
            Vector3 spawnPos = targetAgent.transform.position;

            Debug.Log($"[MergeController] tr: BİRLEŞME BAŞARILI! Seviye {currentTier} -> {nextTier}");

            // tr: Eski araçları havuza geri gönder.
            ObjectPoolManager.Instance.ReturnToPool(draggedAgent.carData.poolId, draggedObject);
            ObjectPoolManager.Instance.ReturnToPool(targetAgent.carData.poolId, targetAgent.gameObject);

            // tr: Yeni üst seviye aracı aynı yerde oluştur.
            GameObject newCar = ObjectPoolManager.Instance.SpawnFromPool(newPoolId, spawnPos, Quaternion.identity);

            if (newCar != null)
            {
                // tr: DOTween ile tatlı bir tepki ver.
                newCar.transform.localScale = Vector3.zero;
                newCar.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
                newCar.transform.DOPunchScale(new Vector3(0.2f, 0.2f, 0.2f), 0.5f);
                
                // tr: Olayı sistemlere duyur.
                EventManager.OnCarMerged?.Invoke(nextTier);
            }
            else
            {
                Debug.LogError($"[MergeController] tr: {newPoolId} havuzdan çekilemedi! Lütfen ObjectPoolManager ayarlarını kontrol edin.");
            }
        }
    }
}
