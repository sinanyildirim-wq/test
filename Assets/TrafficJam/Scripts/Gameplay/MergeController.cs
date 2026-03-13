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
        private Vector3 dragOffset;

        private void Update()
        {
            if (GameManager.Instance.CurrentState != GameState.Playing) return;

            HandleInput();
        }

        private void HandleInput()
        {
            // Eğer ortada bir tıklama aygıtı (mouse/parmak) yoksa direkt iptal et.
            if (Pointer.current == null) return; 

            // 1. AN: Parmağın ekrana İLK dokunduğu o kısacık "an" (wasPressedThisFrame)
            if (Pointer.current.press.wasPressedThisFrame)
            {
                // Masadaki kalemi eline aldığın an
                StartDragging(); // Sürüklemeyi Başlat
            }
            
            // 2. AN: Parmağın ekrana basılı kalmaya "devam ettiği" süreç (isPressed)
            // VE eğer elimizde bir obje varsa (draggedObject != null)
            if (Pointer.current.press.isPressed && draggedObject != null)
            {
                // Kalemi havada gezdirdiğin an
                UpdateDragging(); // Sürüklemeyi Güncelle
            }

            // 3. AN: Parmağını ekrandan "çektiğin" an (wasReleasedThisFrame)
            // VE elimizde bir obje varsa
            if (Pointer.current.press.wasReleasedThisFrame && draggedObject != null)
            {
                // Kalemi masaya (ya da başka kalemin üstüne) bıraktığın an
                StopDragging(); // Sürüklemeyi Durdur
            }
        }

        private void StartDragging()
        {
            Vector2 screenPos = Pointer.current.position.ReadValue(); 
            // Oyuncunun telefon camında dokunduğu X,Y noktası (örnek: x:450, y:800 pikseli).

            Ray ray = Camera.main.ScreenPointToRay(screenPos);
            // Kamera, oyuncunun ekranda dokunduğu o 2D piksel noktasından oyunun içine doğru
            // görünmez düz bir "Ray" (Işın) çizer.
            
            // tr: Raycast'in neye çarptığını görmek için önce carLayer maskesi olmadan bir test yapalım (debug için).
            if (Physics.Raycast(ray, out RaycastHit debugHit, 100f))
            {
                Debug.Log($"[MergeController] tr: Raycast bir şeye çarptı: {debugHit.collider.gameObject.name}, Layer: {LayerMask.LayerToName(debugHit.collider.gameObject.layer)} (Index: {debugHit.collider.gameObject.layer})");
            }

            if (Physics.Raycast(ray, out RaycastHit hit, 100f, carLayer)) 
            //Attığımız sanal lazer (ray) 100 metre ileriye gitsin. Çarptığı şeyleri hit isimli sanal kutuya atsın. AMAA! sadece 
            // ve sadece carLayer (Araba Katmanı) etiketine sahip nesneleri görsün. Yere veya binalara çarparsa yok say!
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
                // Arabanın mevcut pozisyonunu hafızaya al.
                
                // Tıklanan nokta ile arabanın merkezi arasındaki farkı hafızaya al.
                dragOffset = draggedObject.transform.position - hit.point; 
                dragOffset.y = 0; // Y eksenindeki (yükseklik) farkı yoksayalım ki araba düzgün havalansın.
                Debug.Log($"[MergeController] tr: Araç tutuldu! ID: {draggedAgent.carData.poolId}");
                
                //Oyunlarda "Game Feel" (Oyun hissi) denilen şey çok önemlidir. Sen araca tıklandığında anında
                // DOTween eklentisi kullanarak arabayı 0.2 saniye içerisinde dragHeight (atıyorum 1 metre) 
                // havaya kaldırıyorsun. Oyuncu şunu algılıyor: "Süper! Arabayı yerden elime aldım ve şu an tutuyorum.
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
            // Yerden "dragHeight" kadar yüksekte, yüzü yukarı dönük (Vector3.up) görünmez, sonsuz bir masa oluşturduk.
            Plane plane = new Plane(Vector3.up, new Vector3(0, dragHeight, 0));

            // Kameradan parmağımıza doğru fırlattığımız lazer, bu cam masaya (plane) nerede çarpıyor? 
            if (plane.Raycast(ray, out float entry))
            {
                // Çarptığı o nokta bizim arabamızın 3D dünyadaki yeni koordinatları olacak!
                Vector3 hitPoint = ray.GetPoint(entry);
                // YENİ MANTIK: Farenin zeminle kesiştiği yere offset'i ekleyerek pozisyon ver.
                Vector3 newPos = hitPoint + dragOffset; 
                newPos.y = dragHeight; // Yüksekliği sabit tut
                draggedObject.transform.position = newPos; // Arabayı tam o noktaya koy.
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
                // DOPunchScale'i siliyoruz, sadece DOScale yeterli ve çok daha pürüzsüzdür.
                newCar.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
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
