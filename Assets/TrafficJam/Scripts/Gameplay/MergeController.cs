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
        public LayerMask carLayer; // tr: Raycast'in hangi layer'daki objeleri yakalayacağını belirler.
        public float dragHeight = 1.0f; // tr: Sürükleme sırasında aracın yerden yüksekliği.

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
            if (Pointer.current == null) return;

            // tr: Tıklama veya dokunma başladığında.
            if (Pointer.current.press.wasPressedThisFrame)
            {
                Debug.Log("[MergeController] tr: Ekrana tıklandı, Raycast fırlatılıyor...");
                StartDragging();
            }
            
            // tr: Tıklama veya dokunma devam ederken ve bir obje tutuluyorsa.
            if (Pointer.current.press.isPressed && draggedObject != null)
            {
                UpdateDragging();
            }

            // tr: Tıklama veya dokunma bırakıldığında ve bir obje tutuluyorsa.
            if (Pointer.current.press.wasReleasedThisFrame && draggedObject != null)
            {
                StopDragging();
            }
        }

        private void StartDragging()
        {
            Vector2 screenPos = Pointer.current.position.ReadValue();
            Ray ray = Camera.main.ScreenPointToRay(screenPos);
            
            // tr: Raycast'in neye çarptığını görmek için önce carLayer maskesi olmadan bir test yapalım (debug için).
            if (Physics.Raycast(ray, out RaycastHit debugHit, 100f))
            {
                Debug.Log($"[MergeController] tr: Raycast bir şeye çarptı: {debugHit.collider.gameObject.name}, Layer: {LayerMask.LayerToName(debugHit.collider.gameObject.layer)} (Index: {debugHit.collider.gameObject.layer})");
            }

            if (Physics.Raycast(ray, out RaycastHit hit, 100f, carLayer))
            {
                draggedObject = hit.collider.gameObject;
                draggedAgent = draggedObject.GetComponent<CarAgent>();
                
                if (draggedAgent == null)
                {
                    Debug.LogWarning("[MergeController] tr: Çarpılan obje üzerinde CarAgent bulunamadı!");
                    draggedObject = null;
                    return;
                }

                originalPosition = draggedObject.transform.position;
                
                Debug.Log($"[MergeController] tr: Araç tutuldu! ID: {draggedAgent.carData.poolId}");
                
                // tr: Sürüklenen objeyi görsel olarak biraz yukarı kaldır.
                draggedObject.transform.DOMoveY(dragHeight, 0.2f);
            }
            else
            {
                // tr: Eğer carLayer ile bir şey yakalayamadıysak ama debugHit bir şeye çarptıysa, layer uyuşmazlığı vardır.
                if (debugHit.collider != null)
                {
                    Debug.LogWarning("[MergeController] tr: Çarpılan obje geçerli bir araç değil veya doğru Layer'da değil!");
                }
            }
        }

        private void UpdateDragging()
        {
            Vector2 screenPos = Pointer.current.position.ReadValue();
            Ray ray = Camera.main.ScreenPointToRay(screenPos);
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
            Debug.Log("[MergeController] tr: Araç bırakıldı, altındaki eşleşmeler kontrol ediliyor...");

            Vector2 screenPos = Pointer.current.position.ReadValue();
            Ray ray = Camera.main.ScreenPointToRay(screenPos);
            // tr: Bıraktığımız yerde başka bir araç var mı kontrol et.
            
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
                // tr: Boşluk veya uyumsuz yere bırakıldıysa eski yerine döndür.
                Debug.Log("[MergeController] tr: Boşluk veya uyumsuz yere bırakıldı, eski pozisyona dönülüyor.");
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
