using UnityEngine;

namespace TrafficJam.Core
{
    // tr: Meta oyun sistemleri ve çarpanları yöneten Singleton.
    // tr: Diğer birimler burayı okuyarak (IncomeMultiplier, SpeedMultiplier) değerlerini çarpar.
    public class UpgradeManager : MonoBehaviour
    {
        public static UpgradeManager Instance { get; private set; }

        public float IncomeMultiplier { get; private set; } = 1.0f;
        public float SpeedMultiplier { get; private set; } = 1.0f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        // tr: Para/Gelir yükseltmesi (İleride butonlarla tetiklenecek)
        public void UpgradeIncome()
        {
            IncomeMultiplier += 0.5f;
            Debug.Log($"[UpgradeManager] tr: Gelir çarpanı yükseltildi. Yeni çarpan: {IncomeMultiplier}x");
        }

        // tr: Hız yükseltmesi (İleride butonlarla tetiklenecek)
        public void UpgradeSpeed()
        {
            SpeedMultiplier += 0.2f;
            Debug.Log($"[UpgradeManager] tr: Hız çarpanı yükseltildi. Yeni çarpan: {SpeedMultiplier}x");
        }
    }
}
