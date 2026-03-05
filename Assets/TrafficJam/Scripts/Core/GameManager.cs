using UnityEngine;

namespace TrafficJam.Core
{
    // tr: Oyunun anlık durumlarını belirten enum.
    public enum GameState
    {
        MainMenu,
        Playing,
        Paused,
        GameOver
    }

    // tr: Oyunun genel akışını (State Machine) ve temel yaşam döngüsünü kontrol eden Singleton sınıfı.
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public GameState CurrentState { get; private set; }

        private void Awake()
        {
            // tr: Basit Singleton kurulumu.
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject); // tr: Sahneler arası geçişte silinmesini engeller.
        }

        private void Start()
        {
            // tr: Şimdilik test aşamasında olduğumuz için doğrudan Playing durumuna geçiyoruz.
            // İleride UI eklediğimizde burayı tekrar MainMenu yaparız.
            ChangeState(GameState.Playing);
        }

        private void OnDestroy()
        {
            // tr: GameManager yok olduğunda (örneğin sahne değişimi veya uygulama kapandığında), eventleri temizleyerek memory leak'i önleriz.
            EventManager.ClearAllEvents();
        }

        private void OnApplicationQuit()
        {
            // tr: Uygulama tamamen kapandığında da eventleri garanti olarak temizliyoruz.
            EventManager.ClearAllEvents();
        }

        // tr: Oyun durumunu değiştiren evrensel metod.
        public void ChangeState(GameState newState)
        {
            if (CurrentState == newState) return;

            CurrentState = newState;
            Debug.Log($"[GameManager] tr: Oyun durumu değişti: {CurrentState}");

            // tr: State'e göre spesifik işlemler yapılabilir.
            switch (CurrentState)
            {
                case GameState.MainMenu:
                    // tr: Ana menü gösterimi vs.
                    Time.timeScale = 1f;
                    break;
                case GameState.Playing:
                    // tr: Oyun başladığında yapılacaklar
                    Time.timeScale = 1f;
                    break;
                case GameState.Paused:
                    // tr: Oyunu duraklatma, zamanı durdurma işlemi.
                    Time.timeScale = 0f;
                    break;
                case GameState.GameOver:
                    // tr: Oyun bitiş ekranı, hesaplamalar vs.
                    Time.timeScale = 1f;
                    break;
            }
        }

        // tr: EventManager veya UI üzerinden çağrılabilecek yardımcı metodlar.
        public void StartGame()
        {
            ChangeState(GameState.Playing);
        }

        public void PauseGame()
        {
            ChangeState(GameState.Paused);
        }

        public void ResumeGame()
        {
            ChangeState(GameState.Playing);
        }
    }
}
