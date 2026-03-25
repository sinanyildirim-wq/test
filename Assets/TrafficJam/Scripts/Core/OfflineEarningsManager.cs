using UnityEngine;
using System;
using TrafficJam.Gameplay;

namespace TrafficJam.Core
{
    // tr: Phase 4 - Oyuncu çevrimdışıyken kapalı kaldığı süreyi hesaplayıp para kazanmasını sağlar.
    public class OfflineEarningsManager : MonoBehaviour
    {
        public static OfflineEarningsManager Instance { get; private set; }

        [Header("Settings")]
        public float earningsMultiplierBase = 5f; // Her dakika için taban kazanç

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            // Start'ta çağırıyoruz ki SaveManager ve UpgradeManager Awake'te işini bitirmiş olsun.
            CalculateOfflineEarnings();
        }

        private void CalculateOfflineEarnings()
        {
            if (SaveManager.Instance == null || string.IsNullOrEmpty(SaveManager.Instance.Data.lastLoginTime))
                return;

            DateTime lastLogin;
            if (DateTime.TryParse(SaveManager.Instance.Data.lastLoginTime, out lastLogin))
            {
                TimeSpan timeAway = DateTime.Now - lastLogin;
                
                // Eğer oyuncu 1 dakikadan fazla oyundan uzak kaldıysa
                if (timeAway.TotalMinutes >= 1.0)
                {
                    float incMult = UpgradeManager.Instance != null ? UpgradeManager.Instance.IncomeMultiplier : 1f;
                    
                    // Formül: Toplam Dakika * Gelir Çarpanı * Sabit Değer
                    int totalEarnings = Mathf.RoundToInt((float)timeAway.TotalMinutes * incMult * earningsMultiplierBase);

                    if (totalEarnings > 0)
                    {
                        Debug.Log($"[OfflineEarnings] tr: {timeAway.TotalMinutes:F1} dakika çevrimdışı kalındı. Kazanılan: {totalEarnings}");
                        
                        // Önce kazanılan parayı hemen veriyoruz
                        if (EconomyManager.Instance != null)
                        {
                            EconomyManager.Instance.AddMoney(totalEarnings);
                        }

                        // UI'da popup göstermek için event tetiklenir
                        EventManager.OnOfflineEarningsCalculated?.Invoke(totalEarnings);
                    }
                }
            }

            // Hesaptan sonra anlık zamanı yeniden kaydet
            SaveManager.Instance.Data.lastLoginTime = DateTime.Now.ToString("O");
            SaveManager.Instance.SaveGame();
        }
        
        // Modal panelden CLAIM butonuna basıldığında (Opsiyonel, eğer AddMoney'i UI'da çağıracaksanız yukarıdaki AddMoney'i silip buraya taşıyabilirsiniz. 
        // Ama logiğe göre para direkt eklendi, bu sadece UI kapatmak içindir).
        public void ClaimEarnings()
        {
            // İsterseniz parayı burada da EconomyManager.AddMoney ile verebilirsiniz. 
            // Şu an arka planda verdik, sadece UI kapatılmasını beklenebilir.
            Debug.Log("[OfflineEarnings] tr: Kazanç toplandı (Claimed).");
        }
    }
}
