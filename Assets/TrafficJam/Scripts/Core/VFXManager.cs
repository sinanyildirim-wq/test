using UnityEngine;
using TMPro;
using DG.Tweening;

namespace TrafficJam.Core
{
    // tr: Oyun hissiyatı (Game Feel) için parçacık ve yüzen yazı sistemlerini yönetir.
    public class VFXManager : MonoBehaviour
    {
        public static VFXManager Instance { get; private set; }

        [Header("Görsel Efekt Prefabları")]
        [SerializeField] private GameObject floatingTextPrefab;
        [SerializeField] private GameObject mergeParticlePrefab;

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
            EventManager.OnCarCompletedLap += HandleCarCompletedLapVFX;
            EventManager.OnCarMerged += HandleCarMergedVFX;
        }

        private void OnDisable()
        {
            EventManager.OnCarCompletedLap -= HandleCarCompletedLapVFX;
            EventManager.OnCarMerged -= HandleCarMergedVFX;
        }

        // tr: Araç turu tamamladığında yüzen para yazısı oluşturur.
        private void HandleCarCompletedLapVFX(int baseIncome, Vector3 carPosition)
        {
            if (floatingTextPrefab == null) return;

            // tr: Önce nihai parayı (çarpanlı) hesaplıyoruz.
            int finalIncome = Mathf.RoundToInt(baseIncome * UpgradeManager.Instance.IncomeMultiplier);

            // tr: Yazıyı spawnla (Şimdilik Instantiate, ileride Object Pool kullanılabilir)
            GameObject floatingTextObj = Instantiate(floatingTextPrefab, carPosition, Quaternion.identity);
            
            // tr: Text değerini değiştir
            TextMeshPro textMesh = floatingTextObj.GetComponentInChildren<TextMeshPro>();
            if (textMesh != null)
            {
                textMesh.text = $"+$ {finalIncome}";
            }

            // tr: Oyun hissiyatı (DOTween) - Yazı yukarı çıkar ve kaybolur
            floatingTextObj.transform.DOMoveY(carPosition.y + 2f, 1f).SetEase(Ease.OutQuad);
            if (textMesh != null)
            {
                textMesh.DOFade(0f, 1f).SetEase(Ease.InExpo);
            }

            // tr: 1 Saniye sonra yok et
            Destroy(floatingTextObj, 1.1f);
        }

        // tr: Araçlar birleştiğinde (Merge) ufak bir kutlama parçacığı patlatır.
        private void HandleCarMergedVFX(int nextTier, Vector3 mergePosition)
        {
            if (mergeParticlePrefab == null) return;

            GameObject particleObj = Instantiate(mergeParticlePrefab, mergePosition, Quaternion.identity);
            
            // tr: Kendini yok etmesi için
            Destroy(particleObj, 2f);
        }
    }
}
