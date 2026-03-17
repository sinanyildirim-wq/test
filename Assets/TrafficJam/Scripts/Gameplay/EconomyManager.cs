using TrafficJam.Core;
using UnityEngine;

namespace TrafficJam.Gameplay
{
    // tr: Oyundaki ana ekonomiyi (para, bakiye, gelir, gider) yöneten Singleton sınıfı.
    public class EconomyManager : MonoBehaviour
    {
        public static EconomyManager Instance { get; private set; }

        [Header("Economy Status")]
        // tr: Oyuncunun anlık sahip olduğu toplam para. Inspector'dan takip edilebilir.
        [SerializeField] private int currentMoney = 0;

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
            // tr: Araç turu tamamlandığında OnCarCompletedLap tetiklenir, biz de AddMoney metoduna yönlendiririz.
            if (EventManager.OnCarCompletedLap != null)
                EventManager.OnCarCompletedLap += AddMoney;
            else
                EventManager.OnCarCompletedLap = AddMoney; // tr: null check
        }

        private void OnDisable()
        {
            // tr: Memory leak olmaması için OnDisable'da abonelikten çıkılır.
            if (EventManager.OnCarCompletedLap != null)
            {
                EventManager.OnCarCompletedLap -= AddMoney;
            }
        }

        // tr: Bakiye arttırma metodu. Aracın turun tamamlaması (veya reklamlardan gelir) gibi durumlarda çağrılır.
        public void AddMoney(int amount)
        {
            if (amount <= 0) return;

            currentMoney += amount;
            Debug.Log($"[EconomyManager] tr: {amount} para kazanıldı. Toplam Bakiye: {currentMoney}");

            // tr: UI'ı (arayüzü) veya diğer dinleyicileri haberdar et.
            EventManager.OnMoneyChanged?.Invoke(amount, currentMoney);
        }

        // tr: Bakiye harcama metodu. Araç satın alırken veya yol geliştirilirken çağrılır.
        // tr: Yeterli para varsa harcar ve true döner, yoksa false döner.
        public bool SpendMoney(int amount)
        {
            if (amount <= 0) return false;

            if (currentMoney >= amount)
            {
                currentMoney -= amount;
                Debug.Log($"[EconomyManager] tr: {amount} para harcandı. Kalan Bakiye: {currentMoney}");
                
                // tr: Negatif amount göndererek harcama olduğunu belirtebiliriz (isteğe bağlı).
                EventManager.OnMoneyChanged?.Invoke(-amount, currentMoney);
                return true;
            }
            else
            {
                Debug.LogWarning("[EconomyManager] tr: Yeterli bakiye yok!");
                return false;
            }
        }

        // tr: Level geçişini (upgrade) satın alma mantığı burada toplanır.
        // UI butonu, tutorial veya başka sistemler bu metodu çağırabilir.
        public bool TryUpgradeToNextLevel()
        {
            if (LevelManager.Instance == null)
            {
                Debug.LogError("[EconomyManager] TryUpgradeToNextLevel failed: LevelManager.Instance is null.");
                return false;
            }

            if (LevelManager.Instance.CurrentLevelData == null)
            {
                Debug.LogError("[EconomyManager] TryUpgradeToNextLevel failed: CurrentLevelData is null.");
                return false;
            }

            int cost = LevelManager.Instance.CurrentLevelData.upgradeCost;
            if (cost <= 0)
            {
                Debug.LogWarning("[EconomyManager] upgradeCost <= 0, upgrading for free.");
                LevelManager.Instance.LoadNextLevel();
                return true;
            }

            if (!SpendMoney(cost))
            {
                Debug.LogWarning($"[EconomyManager] Not enough money for upgrade. Need={cost}");
                return false;
            }

            Debug.Log($"[EconomyManager] Upgrade purchased. Cost={cost}. Loading next level...");
            LevelManager.Instance.LoadNextLevel();
            return true;
        }
    }
}
