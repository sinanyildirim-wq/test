using UnityEngine;

namespace TrafficJam.Core
{
    // tr: Oyunun "durum makinesi" (state machine) tanımı.
    // tr: UI ve gameplay sistemleri bu durumu EventManager.OnGameStateChanged ile dinler.
    public enum GameState
    {
        MainMenu,
        Playing,
        Paused,
        GameOver
    }

    // tr: Oyunun genel akışını (State Machine) kontrol eden Singleton sınıfı.
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }
        public GameState CurrentState { get; private set; }

        [Header("Debug")]
        [Tooltip("tr: Sadece Editor'da hızlı test için. MainMenu yerine direkt Playing başlatır.")]
        [SerializeField] private bool autoStartInEditor = false;

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
            if (autoStartInEditor && Application.isEditor)
            {
                // tr: Hızlı test modu: doğrudan oynanışa girer (menü akışını atlar).
                ChangeState(GameState.Playing);
                return;
            }

            // tr: Normal akış: oyun açılışta ana menüde başlar.
            ChangeState(GameState.MainMenu);
        }

        private void OnDestroy()
        {
            EventManager.ClearAllEvents();
        }

        private void OnApplicationQuit()
        {
            EventManager.ClearAllEvents();
        }

        public void ChangeState(GameState newState)
        {
            if (CurrentState == newState) return;

            Debug.Log($"[GameManager] State: {CurrentState} -> {newState}");
            CurrentState = newState;

            switch (CurrentState)
            {
                case GameState.MainMenu:
                case GameState.Playing:
                case GameState.GameOver:
                    // tr: Oyun akıyor.
                    Time.timeScale = 1f;
                    break;
                case GameState.Paused:
                    // tr: Oyun duruyor (animasyonlar/unscaled tween ayrı yönetilebilir).
                    Time.timeScale = 0f;
                    break;
            }

            // tr: State değişimini en son satırda tüm sisteme duyur.
            EventManager.OnGameStateChanged?.Invoke(CurrentState);
        }

        public void StartGame() => ChangeState(GameState.Playing);
        public void PauseGame() => ChangeState(GameState.Paused);
        public void ResumeGame() => ChangeState(GameState.Playing);

        // tr: UI'daki Home butonuna bağlanır. Oyunu ana menü state'ine taşır.
        public void GoToMainMenu() => ChangeState(GameState.MainMenu);
    }
}
