using UnityEngine;
using TrafficJam.Data;

namespace TrafficJam.Core
{
    // tr: Meta oyun sistemleri ve çarpanları yöneten Singleton (Phase 2 & 3).
    // tr: Seviye bilgileri SaveManager üzerinden kalıcı hale getirildi.
    public class UpgradeManager : MonoBehaviour
    {
        public static UpgradeManager Instance { get; private set; }

        [Header("Upgrade Settings")]
        public float incomePerLevel = 0.25f;  // tr: Her seviyede gelire eklenecek çarpan miktarı.
        public float speedPerLevel = 0.15f;   // tr: Her seviyede hıza eklenecek çarpan miktarı.
        public int baseCost = 100;            // tr: Başlangıç upgrade maliyeti.
        public float costScalingFactor = 1.5f; // tr: Her seviyede maliyet ne kadar artacak.

        // tr: Dinamik seviye değerleri. SaveManager tarafından doldurulur.
        public int IncomeLevel { get; private set; } = 1;
        public int SpeedLevel { get; private set; } = 1;

        // tr: Hesaplanan çarpanlar. Seviyeye göre otomatik belirlenir.
        public float IncomeMultiplier => 1.0f + (IncomeLevel - 1) * incomePerLevel;
        public float SpeedMultiplier  => 1.0f + (SpeedLevel  - 1) * speedPerLevel;

        // tr: Mevcut upgrade maliyetlerini hesapla.
        public int IncomeCost => Mathf.RoundToInt(baseCost * Mathf.Pow(costScalingFactor, IncomeLevel - 1));
        public int SpeedCost  => Mathf.RoundToInt(baseCost * Mathf.Pow(costScalingFactor, SpeedLevel  - 1));

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

        private void Start()
        {
            // tr: SaveManager Awake'te yüklediğinden Start'ta güvenle okuyabiliriz.
            LoadFromSave();
        }

        public void LoadFromSave()
        {
            if (SaveManager.Instance == null) return;
            SaveData data = SaveManager.Instance.Data;
            IncomeLevel = Mathf.Max(1, data.incomeUpgradeLevel);
            SpeedLevel  = Mathf.Max(1, data.speedUpgradeLevel);
            Debug.Log($"[UpgradeManager] tr: Seviyeler yüklendi. Gelir Lvl:{IncomeLevel}, Hız Lvl:{SpeedLevel}");
        }

        // tr: Gelir seviyesini bir artır. ShopManager tarafından çağrılır.
        public void UpgradeIncome()
        {
            IncomeLevel++;
            Debug.Log($"[UpgradeManager] tr: Gelir seviyesi → {IncomeLevel}  | Çarpan: {IncomeMultiplier:F2}x");
        }

        // tr: Hız seviyesini bir artır. ShopManager tarafından çağrılır.
        public void UpgradeSpeed()
        {
            SpeedLevel++;
            Debug.Log($"[UpgradeManager] tr: Hız seviyesi → {SpeedLevel}  | Çarpan: {SpeedMultiplier:F2}x");
        }
    }
}
