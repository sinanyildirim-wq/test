using UnityEngine;
using TrafficJam.Core;

namespace TrafficJam.Gameplay
{
    // tr: Phase 2 - Mağaza sistemi. Oyuncunun para ile gelir ve hız yükseltmeleri satın almasını sağlar.
    // tr: Event-driven mimari: Başarılı alım sonrası SaveManager.SaveGame() çağrılır.
    public class ShopManager : MonoBehaviour
    {
        public static ShopManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        // tr: Hız yükseltmesi satın alma akışı. UI butonu bu metodu çağırır.
        public void TryBuySpeedUpgrade()
        {
            if (UpgradeManager.Instance == null || EconomyManager.Instance == null) return;

            int cost = UpgradeManager.Instance.SpeedCost;

            if (EconomyManager.Instance.SpendMoney(cost))
            {
                UpgradeManager.Instance.UpgradeSpeed();
                SaveManager.Instance?.SaveGame();
                Debug.Log($"[ShopManager] tr: Hız yükseltmesi satın alındı! Maliyet: {cost}. Yeni Seviye: {UpgradeManager.Instance.SpeedLevel}");
            }
            else
            {
                Debug.LogWarning($"[ShopManager] tr: Hız yükseltmesi için yeterli para yok! Gerekli: {cost}");
            }
        }

        // tr: Gelir yükseltmesi satın alma akışı. UI butonu bu metodu çağırır.
        public void TryBuyIncomeUpgrade()
        {
            if (UpgradeManager.Instance == null || EconomyManager.Instance == null) return;

            int cost = UpgradeManager.Instance.IncomeCost;

            if (EconomyManager.Instance.SpendMoney(cost))
            {
                UpgradeManager.Instance.UpgradeIncome();
                SaveManager.Instance?.SaveGame();
                Debug.Log($"[ShopManager] tr: Gelir yükseltmesi satın alındı! Maliyet: {cost}. Yeni Seviye: {UpgradeManager.Instance.IncomeLevel}");
            }
            else
            {
                Debug.LogWarning($"[ShopManager] tr: Gelir yükseltmesi için yeterli para yok! Gerekli: {cost}");
            }
        }
    }
}
