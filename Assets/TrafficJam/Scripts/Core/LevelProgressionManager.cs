using TrafficJam.Gameplay;
using UnityEngine;

namespace TrafficJam.Core
{
    // tr: "Traffic Jam Fever" tarzı level progression yöneticisi.
    // Merge ve/veya para kazanımıyla barı doldurur. Bar dolunca UI'a "Level Up" sinyali verir.
    public class LevelProgressionManager : MonoBehaviour
    {
        public static LevelProgressionManager Instance { get; private set; }

        [Header("Progress Hedefi (Modüler)")]
        [Tooltip("tr: Progress bar hedefi, CurrentLevelData.upgradeCost değerine göre hesaplanır.\nÖrn: cost=100, +10 para => +0.10 progress.\nÖrn: cost=100, +25 para => +0.25 progress.")]
        [SerializeField] private bool useUpgradeCostAsTarget = true;

        [Header("Ek Tetikleyiciler (Opsiyonel)")]
        [Tooltip("tr: İstersen merge de barı hızlandırabilir. 0 ise kapalı.")]
        [SerializeField, Range(0f, 1f)] private float progressBonusPerMerge = 0f;

        [Header("State")]
        [SerializeField, Range(0f, 1f)] private float currentProgress = 0f;
        private bool isReady = false;
        private int earnedMoneyThisLevel = 0;

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

        private void OnEnable()
        {
            EventManager.OnCarMerged += HandleCarMerged;
            EventManager.OnMoneyChanged += HandleMoneyChanged;
            EventManager.OnLevelLoaded += HandleLevelLoaded;
        }

        private void OnDisable()
        {
            EventManager.OnCarMerged -= HandleCarMerged;
            EventManager.OnMoneyChanged -= HandleMoneyChanged;
            EventManager.OnLevelLoaded -= HandleLevelLoaded;
        }

        private void Start()
        {
            PublishProgress();
        }

        private void HandleLevelLoaded()
        {
            // tr: Yeni level geldiğinde progression sıfırlanabilir.
            ResetProgress();
        }

        private void HandleCarMerged(int newTierIndex)
        {
            // tr: Merge, opsiyonel bonus progress verir (para tabanlı hedefi bozmaz).
            if (progressBonusPerMerge > 0f)
                AddProgress(progressBonusPerMerge, $"merge->tier{newTierIndex}");
        }

        private void HandleMoneyChanged(int changeAmount, int totalMoney)
        {
            if (changeAmount <= 0) return; // tr: sadece kazanç

            // tr: Progress, bu level içinde kazanılan paranın upgradeCost'a oranıdır.
            // tr: Önemli: totalMoney global olabilir; bu yüzden earnedMoneyThisLevel ile ilerliyoruz.
            earnedMoneyThisLevel += changeAmount;

            if (!useUpgradeCostAsTarget || LevelManager.Instance == null || LevelManager.Instance.CurrentLevelData == null)
                return;

            int cost = LevelManager.Instance.CurrentLevelData.upgradeCost;
            if (cost <= 0) return;

            float normalized = Mathf.Clamp01(earnedMoneyThisLevel / (float)cost);
            currentProgress = normalized;
            PublishProgress();

            if (currentProgress >= 1f && !isReady)
            {
                isReady = true;
                Debug.Log($"[LevelProgressionManager] Progress READY (earned/cost). earned={earnedMoneyThisLevel}, cost={cost}. Show LEVEL UP.");
                EventManager.OnLevelProgressReady?.Invoke();
            }
        }

        private void AddProgress(float delta, string reason)
        {
            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing)
                return;

            if (delta <= 0f) return;
            if (isReady) return;

            currentProgress = Mathf.Clamp01(currentProgress + delta);
            PublishProgress();

            if (currentProgress >= 1f && !isReady)
            {
                isReady = true;
                Debug.Log($"[LevelProgressionManager] Progress READY (reason={reason}). Show LEVEL UP.");
                EventManager.OnLevelProgressReady?.Invoke();
            }
        }

        private void PublishProgress()
        {
            EventManager.OnLevelProgressChanged?.Invoke(currentProgress);
        }

        public void ResetProgress()
        {
            currentProgress = 0f;
            isReady = false;
            earnedMoneyThisLevel = 0;
            PublishProgress();
        }

        // tr: UI butonu burayı çağırır. Harcama/upgrade EconomyManager içinden yapılır.
        public void ConsumeAndLevelUp()
        {
            if (!isReady)
            {
                Debug.LogWarning("[LevelProgressionManager] Consume requested but progress is not ready.");
                return;
            }

            if (EconomyManager.Instance == null)
            {
                Debug.LogError("[LevelProgressionManager] EconomyManager.Instance is null.");
                return;
            }

            bool ok = EconomyManager.Instance.TryUpgradeToNextLevel();
            if (!ok) return;

            Debug.Log("[LevelProgressionManager] Progress consumed. Level up triggered.");
            EventManager.OnLevelProgressConsumed?.Invoke();
            ResetProgress();
        }
    }
}

