using System.Collections.Generic;
using UnityEngine;
using TrafficJam.Data;
using TrafficJam.Gameplay;
using DG.Tweening;

namespace TrafficJam.Core
{
    // tr: Level yükleme ve environment yönetimi.
    // tr: Sorumluluk:
    // tr: - CurrentLevelData seçmek (hangi LevelDataSO aktif?)
    // tr: - Level environment prefabını instantiate etmek
    // tr: - Yeni levelin waypoint rotasını global PathManager'a vermek
    // tr: - Level değişiminde eski araçları temizlemek (TrafficManager üzerinden)
    public class LevelManager : MonoBehaviour
    {
        public static LevelManager Instance { get; private set; }
        public LevelDataSO CurrentLevelData { get; private set; }

        [Header("Level Configuration")]
        public List<LevelDataSO> allLevels;
        [Tooltip("tr: Oyun state'i ne olursa olsun sahne açılınca environment prefabını yükler.\nTraffic spawn vb. sistemler yine GameState.Playing'e göre çalışır.")]
        [SerializeField] private bool autoLoadOnStart = true;

        private int currentLevelIndex = 0;
        private GameObject currentEnvironmentInstance;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // tr: Global PathManager sahnede yoksa otomatik oluştur.
            // tr: Level prefabı içinde de PathManager bulunabilir ama o sadece waypoint "data holder" olarak kalır.
            if (PathManager.Instance == null)
            {
                GameObject go = new GameObject("--- PathManager (Global) ---");
                go.AddComponent<PathManager>();
                Debug.LogWarning("[LevelManager] Global PathManager was missing. Auto-created one.");
            }
        }

        private void OnEnable()
        {
            EventManager.OnGameStateChanged += HandleGameStateChanged;
        }

        private void OnDisable()
        {
            EventManager.OnGameStateChanged -= HandleGameStateChanged;
        }

        private void Start()
        {
            // tr: En sık yaşanan mantık hatası: Play butonu/state geçişi olmadan level hiç yüklenmiyor.
            // Bu seçenek açıksa environment her zaman gelir; diğer sistemler state'e göre davranır.
            if (!autoLoadOnStart) return;
            if (currentEnvironmentInstance != null) return;

            if (allLevels == null || allLevels.Count == 0)
            {
                Debug.LogError("[LevelManager] autoLoadOnStart enabled but allLevels is empty!");
                return;
            }

            Debug.Log("[LevelManager] autoLoadOnStart: loading initial environment.");
            LoadLevel(currentLevelIndex);
        }

        // tr: Sadece Playing state'ine geçildiğinde ve sahnede aktif harita yoksa level yükle.
        private void HandleGameStateChanged(GameState newState)
        {
            Debug.Log($"[LevelManager] GameState changed: {newState} (hasEnv={currentEnvironmentInstance != null})");
            if (newState == GameState.Playing && currentEnvironmentInstance == null)
            {
                LoadLevel(currentLevelIndex);
            }
        }

        public void LoadLevel(int index)
        {
            Debug.Log($"[LevelManager] LoadLevel({index}) requested.");
            if (index < 0 || index >= allLevels.Count)
            {
                Debug.LogError($"[LevelManager] Invalid level index: {index}");
                return;
            }

            // tr: Level değişmeden önce eski level'in araçlarını temizle (kritik).
            if (TrafficManager.Instance != null)
                TrafficManager.Instance.ReturnAllActiveCarsToPool();

            // tr: Eski haritayı temizle.
            if (currentEnvironmentInstance != null)
            {
                Destroy(currentEnvironmentInstance);
            }

            LevelDataSO levelToLoad = allLevels[index];

            if (levelToLoad.levelEnvironmentPrefab == null)
            {
                Debug.LogError("[LevelManager] levelEnvironmentPrefab is null on LevelDataSO!");
                return;
            }

            currentEnvironmentInstance = Instantiate(levelToLoad.levelEnvironmentPrefab);
            Debug.Log($"[LevelManager] Instantiated env prefab: '{levelToLoad.levelEnvironmentPrefab.name}' -> instance '{currentEnvironmentInstance.name}'");

            // tr: Yeni haritadaki rotayı PathManager'a ilet.
            // tr: Bizim level prefablarımızın içinde PathManager component'i var ve waypoints listesi orada set edilmiş.
            // tr: Bu yüzden önce prefab'ın kendi PathManager'ını arıyoruz; yoksa LevelEnvironment fallback'ine düşüyoruz.
            PathManager envPathManager = currentEnvironmentInstance.GetComponentInChildren<PathManager>();
            if (envPathManager != null && envPathManager.GetWaypoints() != null && envPathManager.GetWaypoints().Count > 0)
            {
                // tr: Sahne genelinde tek bir global PathManager.Instance vardır; onun listesini güncelle.
                if (PathManager.Instance == null)
                {
                    Debug.LogError("[LevelManager] Global PathManager.Instance is null (should not happen).");
                    return;
                }

                // tr: Listeyi kopyalayarak veriyoruz (referans paylaşımı olmasın).
                PathManager.Instance.SetWaypoints(new List<Transform>(envPathManager.GetWaypoints()));
                Debug.Log($"[LevelManager] Waypoints set from prefab PathManager. Count={envPathManager.GetWaypoints().Count}");

                // tr: Prefab içindeki PathManager artık singleton çatışmasında kendini destroy etmez (devre dışı kalır).
            }
            else
            {
                // tr: Alternatif kurulum: LevelEnvironment komponenti üzerinden waypoint listesi.
                LevelEnvironment envData = currentEnvironmentInstance.GetComponent<LevelEnvironment>();
                if (envData == null)
                    envData = currentEnvironmentInstance.GetComponentInChildren<LevelEnvironment>();

                if (envData != null && envData.levelWaypoints != null && envData.levelWaypoints.Count > 0)
                {
                    if (PathManager.Instance == null)
                    {
                        Debug.LogError("[LevelManager] Global PathManager.Instance is null (should not happen).");
                        return;
                    }

                    // tr: Listeyi kopyalayarak veriyoruz (referans paylaşımı olmasın).
                    PathManager.Instance.SetWaypoints(new List<Transform>(envData.levelWaypoints));
                    Debug.Log($"[LevelManager] Waypoints set from LevelEnvironment. Count={envData.levelWaypoints.Count}");
                }
                else
                {
                    Debug.LogError("[LevelManager] No waypoints found in new environment (no PathManager waypoints, no LevelEnvironment list).");
                    return;
                }
            }

            CurrentLevelData = levelToLoad;
            Debug.Log($"[LevelManager] CurrentLevelData set. level={levelToLoad.currentLevel}, maxCarCapacity={levelToLoad.maxCarCapacity}, upgradeCost={levelToLoad.upgradeCost}");

            // tr: Eski manuel LevelManager kamera animasyonu kaldırıldı, görev CameraController.cs'te!

            // tr: Rota hazır, level yüklendi sinyali gönder (CameraController Auto-Framing için bu event'i dinleyecek).
            EventManager.OnLevelLoaded?.Invoke();
        }

        public void LoadNextLevel()
        {
            if (currentLevelIndex + 1 < allLevels.Count)
            {
                currentLevelIndex++;
                Debug.Log($"[LevelManager] tr: Yeni level'e geçiliyor ({currentLevelIndex}). Soft Reset başlatılıyor...");

                // tr: 1) Parayı sıfırla — oyuncu eski parayı yeni levele taşımasın.
                EconomyManager.Instance.ResetMoney();

                // tr: 2) Dükkan yükseltmelerini sıfırla — çarpanlar 1.0f'e döner.
                UpgradeManager.Instance.ResetUpgrades();

                // tr: 3) Hemen kaydet — oyuncu çık/gir döngüsüyle eski parayı geri almasın.
                SaveManager.Instance.SaveGame();

                LoadLevel(currentLevelIndex);
            }
        }
    }
}
