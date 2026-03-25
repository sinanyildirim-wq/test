using UnityEngine;
using System.Collections;
using TMPro;
using DG.Tweening;
using TrafficJam.Core;

namespace TrafficJam.Core
{
    // tr: Oyun hissiyatı (Game Feel) için parçacık ve yüzen yazı sistemlerini yönetir.
    // tr: ÖNEMLİ: Bu manager prefab referansı TUTMAZ!
    // tr: Tüm VFX objeleri ObjectPoolManager üzerinden spawn/return edilir.
    // tr: Pool ID'leri: "VFX_Merge" (merge partikülü), "VFX_FloatingText" (yüzen para yazısı)
    public class VFXManager : MonoBehaviour
    {
        public static VFXManager Instance { get; private set; }

        // tr: Pool ID sabitleri — ObjectPoolManager Inspector'ında aynı isimlerle tanımlanmalıdır.
        private const string PoolId_MergeVFX = "VFX_Merge";
        private const string PoolId_FloatingText = "VFX_FloatingText";

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void OnEnable()
        {
            // tr: Event-driven mimari: EventManager üzerinden merge ve tur tamamlama olaylarını dinle.
            EventManager.OnCarCompletedLap += HandleCarCompletedLapVFX;
            EventManager.OnCarMerged += HandleCarMergedVFX;
        }

        private void OnDisable()
        {
            // tr: Ghost listener olmaması için abonelikten çık.
            EventManager.OnCarCompletedLap -= HandleCarCompletedLapVFX;
            EventManager.OnCarMerged -= HandleCarMergedVFX;
        }

        // ──────────────────────────────────────────────
        //  MERGE VFX  (Object Pool)
        // ──────────────────────────────────────────────

        // tr: Araçlar birleştiğinde (Merge) havuzdan partikül çek ve 0.8 sn sonra geri gönder.
        private void HandleCarMergedVFX(int nextTier, Vector3 mergePosition)
        {
            // tr: Havuzdan merge partikülünü spawn et.
            GameObject particleObj = ObjectPoolManager.Instance.SpawnFromPool(
                PoolId_MergeVFX, mergePosition, Quaternion.identity);

            if (particleObj == null)
            {
                Debug.LogWarning($"[VFXManager] tr: '{PoolId_MergeVFX}' havuzundan obje alınamadı!");
                return;
            }

            // tr: Partikül çok daha kısa sürede (0.8s) havuza geri dönsün ki seri birleşimlerde doğal dursun.
            StartCoroutine(ReturnToPoolAfterDelay(PoolId_MergeVFX, particleObj, 0.8f));
        }

        // ──────────────────────────────────────────────
        //  FLOATING TEXT VFX  (Object Pool + DOTween)
        // ──────────────────────────────────────────────

        // tr: Araç turu tamamladığında yüzen para yazısı oluşturur.
        // tr: UpgradeManager çarpanı burada uygulanarak nihai gelir (finalIncome) hesaplanır.
        private void HandleCarCompletedLapVFX(int baseIncome, Vector3 carPosition)
        {
            // tr: Nihai geliri UpgradeManager çarpanıyla hesapla.
            int finalIncome = Mathf.RoundToInt(baseIncome * UpgradeManager.Instance.IncomeMultiplier);

            // tr: Eğer UI paneline taşınacaksa objenin yeri ScreenSpace'de hesaplanmalı.
            Vector3 screenPosition = carPosition;
            if (Camera.main != null)
                screenPosition = Camera.main.WorldToScreenPoint(carPosition);

            // tr: Havuzdan floating text objesini spawn et.
            GameObject floatingTextObj = ObjectPoolManager.Instance.SpawnFromPool(
                PoolId_FloatingText, screenPosition, Quaternion.identity);

            if (floatingTextObj == null)
            {
                Debug.LogWarning($"[VFXManager] tr: '{PoolId_FloatingText}' havuzundan obje alınamadı!");
                return;
            }

            // tr: Objeyi aktif bir UI panelinin içine evlatlık (child) olarak atıyoruz ki 2D ekranda gözüksün.
            if (TrafficJam.UI.UIManager.Instance != null && TrafficJam.UI.UIManager.Instance.gamePanel != null)
            {
                floatingTextObj.transform.SetParent(TrafficJam.UI.UIManager.Instance.gamePanel.transform, true);
            }

            // tr: TextMeshPro veya TextMeshProUGUI (Canvas texti) fark etmeksizin TMP_Text olarak al.
            TMP_Text textMesh = floatingTextObj.GetComponentInChildren<TMP_Text>();
            if (textMesh != null)
            {
                textMesh.text = $"+$ {finalIncome}";
                // tr: Havuzdan gelen objenin alpha'sı sıfırlanmış olabilir; tam görünür yap.
                textMesh.alpha = 1f;
            }

            // tr: Başlangıç state'i: Boyutu sıfırla ki Pop-up (sıçrama) efekti güzel görünsün.
            floatingTextObj.transform.localScale = Vector3.zero;

            // tr: DOTween animasyonu 1: 0.2 saniyede 1.2x boyutuna fırla (OutBack ile hafif yaylanma).
            floatingTextObj.transform.DOScale(1.2f, 0.2f).SetEase(Ease.OutBack);

            // tr: DOTween animasyonu 2: 1 saniye içerisinde UI (ScreenSpace) üzerinde yukarıya süzül (Y ekseninde piksel bazlı +150 piksel).
            floatingTextObj.transform
                .DOMoveY(screenPosition.y + 150f, 1f)
                .SetEase(Ease.OutQuad);

            if (textMesh != null)
            {
                // tr: DOTween animasyonu 3: 0.8 saniyede saydamlaş (Pop-up bittikten hemen sonra başlar)
                textMesh.DOFade(0f, 0.8f)
                    .SetEase(Ease.InExpo)
                    .SetDelay(0.2f)
                    .OnComplete(() =>
                    {
                        // tr: Animasyon tamamlandı. Objeyi sıfırla ve havuza iade et.
                        ResetAndReturnFloatingText(floatingTextObj, textMesh);
                    });
            }
            else
            {
                // tr: TextMeshPro bulunamazsa yine de 1 sn sonra havuza gönder.
                StartCoroutine(ReturnToPoolAfterDelay(PoolId_FloatingText, floatingTextObj, 1.1f));
            }
        }

        // ──────────────────────────────────────────────
        //  YARDIMCI METODLAR
        // ──────────────────────────────────────────────

        // tr: Floating text objesini sıfırlayıp havuza geri gönderir.
        // tr: Alpha ve pozisyon gibi değerler bir sonraki kullanım için temizlenir.
        private void ResetAndReturnFloatingText(GameObject obj, TMP_Text textMesh)
        {
            // tr: DOTween tweenlerini durdur (güvenlik).
            obj.transform.DOKill();
            textMesh.DOKill();

            // tr: Alpha'yı ve Scale'i geri getir (bir sonraki spawn'da düzgün görünmesi için).
            textMesh.alpha = 1f;
            textMesh.text = "";
            obj.transform.localScale = Vector3.one;

            // tr: Objeyi havuza geri gönderirken Panel child'lığından çıkarıp orijinal Pool_Parent'a alınabilir,
            // tr: Veya öyle kalabilir. ObjectPoolManager Spawn ederken kendi düzeltiyor, direkt gönderiyoruz.
            ObjectPoolManager.Instance.ReturnToPool(PoolId_FloatingText, obj);
        }

        // tr: Belirtilen süre sonunda objeyi havuza geri gönderen Coroutine.
        // tr: Merge partikülü gibi kendi kendine biten efektler için kullanılır.
        private IEnumerator ReturnToPoolAfterDelay(string poolId, GameObject obj, float delay)
        {
            yield return new WaitForSeconds(delay);

            if (obj != null && obj.activeInHierarchy)
            {
                // tr: Objeyi havuza iade et.
                ObjectPoolManager.Instance.ReturnToPool(poolId, obj);
            }
        }
    }
}
