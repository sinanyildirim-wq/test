using UnityEngine;
using TMPro;
using DG.Tweening;
using TrafficJam.Core;
using TrafficJam.Gameplay;
using UnityEngine.UI;

namespace TrafficJam.UI
{
    // tr: Para göstergesi vb. arayüz elemanlarını yöneten Singleton sınıfı.
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("UI References")]
        public TextMeshProUGUI moneyText; // tr: Para miktarını gösterecek Text objesi.
        public GameObject mainMenuPanel;  // tr: Ana menü arayüzü.
        public GameObject gamePanel;      // tr: Oyun içi HUD/arayerüz paneli.
        public GameObject pausePanel;     // tr: Duraklatma (Pause) menüsü.

        [Header("Progression UI")]
        [Tooltip("tr: Üstteki progress bar (Slider). Value 0..1 beklenir.")]
        [SerializeField] private Slider levelProgressSlider;
        [Tooltip("tr: Bar dolunca görünen LEVEL UP butonu (GameObject olarak).")]
        [SerializeField] private GameObject levelUpButtonRoot;

        [Header("LEVEL UP Etkisi")]
        [Tooltip("tr: Cost dolunca LEVEL UP ikonu nabız gibi büyüyüp küçülsün.")]
        [SerializeField] private bool pulseLevelUpButton = true;
        [Tooltip("tr: LEVEL UP hazır olunca oyunu durdur (Time.timeScale=0).")]
        [SerializeField] private bool pauseGameOnLevelUpReady = true;
        [SerializeField, Range(1.05f, 1.6f)] private float levelUpPulseScale = 1.2f;
        [SerializeField] private float levelUpPulseDuration = 0.6f;

        private float _savedTimeScale = 1f;

        [Header("Panel Görünüm")]
        [Tooltip("tr: GamePanel açıkken arka plan Image'ı tam saydam yapılır; sadece içindeki Money, Pause vb. görünür kalır.")]
        [SerializeField] private bool makeGamePanelBackgroundTransparent = true;
        [Tooltip("tr: PausePanel arka planı için kullanılacak alpha (0=saydam, 1=opak). Örn: 0.6 = buğulu karartma.")]
        [Range(0f, 1f)]
        [SerializeField] private float pausePanelOverlayAlpha = 0.6f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            if (moneyText == null)
                Debug.LogError("[UIManager] tr: moneyText referansı atanmamış!");
        }

        private void Start()
        {
            // tr: Panel arka plan saydamlıklarını uygula; sonra mevcut state'e göre aç/kapa (eş zamanlı mantık).
            ApplyPanelBackgroundSettings();
            SyncPanelsToGameState();
        }

        private void OnEnable()
        {
            // tr: Para değiştiğinde UI'ı güncellemek için event'e abone oluyoruz.
            EventManager.OnMoneyChanged += UpdateMoneyUI;

            // tr: Oyun durumu değiştiğinde (MainMenu/Playing/Paused/GameOver) panel görünürlüğünü güncelle.
            EventManager.OnGameStateChanged += HandleGameStateChanged;

            // tr: Progress bar & level-up görünürlüğü.
            EventManager.OnLevelProgressChanged += HandleLevelProgressChanged;
            EventManager.OnLevelProgressReady += HandleLevelProgressReady;
            EventManager.OnLevelProgressConsumed += HandleLevelProgressConsumed;

        }

        private void OnDisable()
        {
            // tr: Hafıza sızıntısını önlemek için abonelikten çıkıyoruz.
            EventManager.OnMoneyChanged -= UpdateMoneyUI;
            EventManager.OnGameStateChanged -= HandleGameStateChanged;
            EventManager.OnLevelProgressChanged -= HandleLevelProgressChanged;
            EventManager.OnLevelProgressReady -= HandleLevelProgressReady;
            EventManager.OnLevelProgressConsumed -= HandleLevelProgressConsumed;
        }

        // tr: GameState değiştiğinde panelleri tek merkezden senkronize et. Oyun başında sadece HUD; durdurma tuşunda pause paneli.
        private void HandleGameStateChanged(GameState newState)
        {
            SyncPanelsToGameState();
        }

        // tr: Tüm panel aç/kapa mantığı burada toplanır — hem Start hem HandleGameStateChanged bu metodu kullanır (eş zamanlı çalışma).
        private void SyncPanelsToGameState()
        {
            GameState state = GameManager.Instance != null ? GameManager.Instance.CurrentState : GameState.MainMenu;

            if (mainMenuPanel != null)
                mainMenuPanel.SetActive(state == GameState.MainMenu);
            if (gamePanel != null)
            {
                gamePanel.SetActive(state == GameState.Playing);
                if (state == GameState.Playing && makeGamePanelBackgroundTransparent)
                    SetPanelBackgroundAlpha(gamePanel, 0f);
            }
            if (pausePanel != null)
            {
                pausePanel.SetActive(state == GameState.Paused);
                if (state == GameState.Paused)
                    SetPanelBackgroundAlpha(pausePanel, pausePanelOverlayAlpha);
            }
        }

        // tr: GamePanel arka planını saydam yapar; içindeki Money, Pause, Slider vb. görünür kalır. Kötü buğulu görüntü kalkar.
        private void ApplyPanelBackgroundSettings()
        {
            if (makeGamePanelBackgroundTransparent && gamePanel != null)
                SetPanelBackgroundAlpha(gamePanel, 0f);
            if (pausePanel != null && pausePanelOverlayAlpha > 0f)
                SetPanelBackgroundAlpha(pausePanel, pausePanelOverlayAlpha);
        }

        // tr: Bir panel GameObject'inde Image bileşeni varsa rengini ayarlar (child'lar etkilenmez). Pause için siyah karartma.
        private void SetPanelBackgroundAlpha(GameObject panel, float alpha)
        {
            if (panel == null) return;
            var image = panel.GetComponent<Image>();
            if (image == null) return;

            float a = Mathf.Clamp01(alpha);
            Color c;
            if (panel == pausePanel && alpha > 0f)
                c = new Color(0f, 0f, 0f, a); // tr: Pause paneli = siyah, yarı saydam (buğulu karartma).
            else
            {
                c = image.color;
                c.a = a;
            }
            image.color = c;
        }

        // tr: Para miktarını ekranda güncelleyen ve animasyon ekleyen metod.
        private void UpdateMoneyUI(int changeAmount, int totalMoney)
        {
            if (moneyText == null) return;

            moneyText.text = totalMoney.ToString() + " $";
            
            // tr: DOTween PunchScale ile "parlamış/vurgulanmış" hissi veriyoruz.
            moneyText.transform.DOPunchScale(new Vector3(0.15f, 0.15f, 0.15f), 0.3f, 10, 1)
                .OnComplete(() => moneyText.transform.localScale = Vector3.one); // tr: Ölçeğin sıfırlanmasını garanti et.

        }

        // tr: Ana menüdeki "Play" butonuna bağlanacak metod.
        public void OnPlayButtonClicked()
        {
            GameManager.Instance.StartGame();
        }

        // tr: Oyun içi "Pause" butonuna bağlanacak metod.
        public void OnPauseButtonClicked()
        {
            GameManager.Instance.PauseGame();
        }

        // tr: Pause menüsündeki "Resume" butonuna bağlanacak metod.
        public void OnResumeButtonClicked()
        {
            GameManager.Instance.ResumeGame();
        }

        // tr: Oyun içindeki "Upgrade / Next Level" butonuna bağlanacak metod.
        public void OnUpgradeLevelButtonClicked()
        {
            if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing)
            {
                Debug.LogWarning("[UIManager] Upgrade requested while not Playing.");
                return;
            }

            // tr: Eğer progression sistemi kullanılıyorsa, level up buradan consume edilir.
            if (LevelProgressionManager.Instance != null)
            {
                LevelProgressionManager.Instance.ConsumeAndLevelUp();
                return;
            }

            if (EconomyManager.Instance == null)
            {
                Debug.LogError("[UIManager] EconomyManager.Instance is null.");
                return;
            }

            EconomyManager.Instance.TryUpgradeToNextLevel();
        }

        private void HandleLevelProgressChanged(float normalizedProgress)
        {
            if (levelProgressSlider != null)
                levelProgressSlider.value = Mathf.Clamp01(normalizedProgress);
            // tr: Slider atanmamışsa Inspector'da UIManager -> Level Progress Slider alanına sürükle.
            else if (normalizedProgress > 0.001f)
                Debug.LogWarning("[UIManager] tr: Level progress slider atanmamış; bar güncellenemiyor. Inspector'da Level Progress Slider alanını doldur.");
        }

        private void HandleLevelProgressReady()
        {
            if (levelUpButtonRoot != null)
            {
                levelUpButtonRoot.SetActive(true);

                if (pulseLevelUpButton)
                {
                    // tr: Oyun dursa bile animasyon aksın diye unscaled update kullanıyoruz.
                    Transform t = levelUpButtonRoot.transform;
                    t.DOKill();
                    t.localScale = Vector3.one;
                    t.DOScale(levelUpPulseScale, levelUpPulseDuration)
                        .SetEase(Ease.InOutSine)
                        .SetLoops(-1, LoopType.Yoyo)
                        .SetUpdate(true);
                }
            }

            // tr: LEVEL UP ekranı görünürken HUD (gamePanel) kapansın.
            if (gamePanel != null)
                gamePanel.SetActive(false);

            if (pauseGameOnLevelUpReady)
            {
                _savedTimeScale = Time.timeScale;
                Time.timeScale = 0f;
            }
        }

        private void HandleLevelProgressConsumed()
        {
            if (levelUpButtonRoot != null)
            {
                levelUpButtonRoot.transform.DOKill();
                levelUpButtonRoot.transform.localScale = Vector3.one;
                levelUpButtonRoot.SetActive(false);
            }

            if (pauseGameOnLevelUpReady)
                Time.timeScale = _savedTimeScale <= 0f ? 1f : _savedTimeScale;

            // tr: Level up tüketildi; mevcut GameState'e göre panelleri tekrar senkronize et (HUD geri gelsin).
            SyncPanelsToGameState();
        }
    }
}
