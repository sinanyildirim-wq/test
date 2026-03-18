using System.Collections;
using System.Collections.Generic;
using TrafficJam.Core;
using UnityEngine;

namespace TrafficJam.Gameplay
{
    // tr: Araç trafiği (spawn + aktif araç takibi) yöneticisi.
    // tr: Sorumluluk:
    // tr: - Level hazır olduğunda ve oyun Playing iken belirli aralıklarla araç spawn etmek
    // tr: - Aktif araç listesini tutmak (kapasite kontrolü + level geçişinde temizlik)
    // tr: - Level geçişinde eski araçları havuza geri göndermek (ReturnAllActiveCarsToPool)
    public class TrafficManager : MonoBehaviour
    {
        public static TrafficManager Instance { get; private set; }

        [SerializeField] private float spawnInterval = 1.0f;

        [Header("Debug")]
        [Tooltip("tr: Açıkken her spawn tick'inde log basar.")]
        [SerializeField] private bool verboseSpawnLogs = false;

        private List<GameObject> activeCarsOnRoad = new List<GameObject>();
        private Coroutine spawnCoroutine;
        private bool isLevelReady = false;

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
            EventManager.OnGameStateChanged += HandleGameStateChanged;
            EventManager.OnLevelLoaded += HandleLevelLoaded;
            EventManager.OnCarMerged += HandleCarMerged;
            Debug.Log("[TrafficManager] Enabled. Waiting for level + Playing state to start traffic.");
        }

        private void OnDisable()
        {
            EventManager.OnGameStateChanged -= HandleGameStateChanged;
            EventManager.OnLevelLoaded -= HandleLevelLoaded;
            EventManager.OnCarMerged -= HandleCarMerged;
            StopSpawnRoutine();
        }

        private void HandleLevelLoaded()
        {
            // tr: LevelManager rota + environment hazır dedi (OnLevelLoaded).
            // tr: Bundan sonra Playing state'teysek spawn rutinini başlatabiliriz.
            isLevelReady = true;
            Debug.Log("[TrafficManager] Level loaded. Traffic is ready.");

            if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Playing)
            {
                // tr: Level geçişinde bazı frame'lerde PathManager.Instance/waypoints anlık olarak boş görünebiliyor.
                // tr: Bu yüzden spawn başlatmayı bir frame erteleyip kısa süre waypoint gelene kadar bekliyoruz.
                StartSpawnRoutineDeferred();
            }
            else
            {
                Debug.Log($"[TrafficManager] Level ready but GameState is not Playing yet (state={GameManager.Instance?.CurrentState}).");
            }
        }

        private void HandleGameStateChanged(GameState newState)
        {
            // tr: MainMenu/Pause gibi state'lerde spawn durur; Playing olunca tekrar başlar.
            Debug.Log($"[TrafficManager] GameState changed: {newState} (isLevelReady={isLevelReady})");
            if (newState != GameState.Playing)
            {
                StopSpawnRoutine();
                return;
            }

            // tr: Playing'e geçildi ve level hazırsa coroutine'i başlat.
            if (isLevelReady)
            {
                StartSpawnRoutineDeferred();
            }
            else
            {
                Debug.Log("[TrafficManager] Playing state reached, but level is not ready yet. Waiting for OnLevelLoaded.");
            }
        }

        private void StartSpawnRoutineDeferred()
        {
            // tr: Level load sırasında singleton/waypoint referansları bir frame gecikmeli oturabiliyor.
            // tr: Bu nedenle StartSpawnRoutine'yi doğrudan çağırmak yerine önce waypoint'i bekliyoruz.
            StopSpawnRoutine();
            StartCoroutine(WaitForWaypointsThenStart());
        }

        private IEnumerator WaitForWaypointsThenStart()
        {
            // tr: 60 frame ~ 1 saniye. Bu süre içinde waypoint gelmezse hata basıp vazgeçeriz.
            const int maxFrames = 60;
            for (int i = 0; i < maxFrames; i++)
            {
                if (PathManager.Instance != null && PathManager.Instance.GetWaypoints() != null && PathManager.Instance.GetWaypoints().Count > 0)
                {
                    StartSpawnRoutine();
                    yield break;
                }
                yield return null; // tr: bir sonraki frame
            }

            int count = (PathManager.Instance == null || PathManager.Instance.GetWaypoints() == null) ? 0 : PathManager.Instance.GetWaypoints().Count;
            Debug.LogError($"[TrafficManager] Cannot start spawn: waypoints still missing after wait. PathManager={(PathManager.Instance == null ? "null" : PathManager.Instance.name)}, count={count}");
        }

        private void StartSpawnRoutine()
        {
            StopSpawnRoutine();

            if (PathManager.Instance == null || PathManager.Instance.GetWaypoints() == null || PathManager.Instance.GetWaypoints().Count == 0)
            {
                Debug.LogError("[TrafficManager] Cannot start spawn: PathManager has no waypoints yet.");
                return;
            }

            if (LevelManager.Instance == null || LevelManager.Instance.CurrentLevelData == null)
            {
                Debug.LogError("[TrafficManager] Cannot start spawn: LevelManager.CurrentLevelData is null (level not initialized).");
                return;
            }

            Debug.Log($"[TrafficManager] Starting spawn routine. spawnInterval={spawnInterval}, capacity={LevelManager.Instance.CurrentLevelData.maxCarCapacity}");
            spawnCoroutine = StartCoroutine(SpawnRoutine());
        }

        private void StopSpawnRoutine()
        {
            if (spawnCoroutine != null)
            {
                StopCoroutine(spawnCoroutine);
                spawnCoroutine = null;
            }
        }

        private IEnumerator SpawnRoutine()
        {
            while (true)
            {
                // tr: Zamanlama beklentisi: araçlar spawnInterval kadar bekledikten sonra aktif olsun.
                yield return new WaitForSeconds(spawnInterval);

                if (GameManager.Instance.CurrentState == GameState.Playing && isLevelReady)
                {
                    int currentActiveCars = activeCarsOnRoad.Count;
                    if (currentActiveCars < LevelManager.Instance.CurrentLevelData.maxCarCapacity)
                    {
                        if (verboseSpawnLogs)
                        {
                            Debug.Log($"[TrafficManager] Spawn tick. t={Time.time:F2}, active={currentActiveCars}/{LevelManager.Instance.CurrentLevelData.maxCarCapacity}");
                        }
                        SpawnCar("Car_Tier1");
                    }
                    else if (verboseSpawnLogs)
                    {
                        Debug.Log($"[TrafficManager] Spawn skipped (capacity). t={Time.time:F2}, active={currentActiveCars}/{LevelManager.Instance.CurrentLevelData.maxCarCapacity}");
                    }
                }
            }
        }

        private void SpawnCar(string poolId)
        {
            // tr: Spawn noktası olarak waypoint[0] kullanıyoruz.
            // tr: Bu yüzden waypoint listesi boşsa spawn iptal edilir.
            if (PathManager.Instance == null || PathManager.Instance.GetWaypoints() == null || PathManager.Instance.GetWaypoints().Count == 0)
            {
                int count = (PathManager.Instance == null || PathManager.Instance.GetWaypoints() == null) ? 0 : PathManager.Instance.GetWaypoints().Count;
                Debug.LogError($"[TrafficManager] SpawnCar failed: no waypoints. PathManager={(PathManager.Instance == null ? "null" : PathManager.Instance.name)}, count={count}");
                return;
            }

            Transform startPoint = PathManager.Instance.GetWaypoints()[0];
            GameObject car = ObjectPoolManager.Instance.SpawnFromPool(poolId, startPoint.position, startPoint.rotation);

            if (car == null) return;

            if (!activeCarsOnRoad.Contains(car))
                activeCarsOnRoad.Add(car);

            CarAgent agent = car.GetComponent<CarAgent>();
            if (agent != null)
                agent.InitializePath();

            // tr: UI/debug tarafı "araba spawn oldu" gibi şeyler yapmak isterse bu event'i dinler.
            EventManager.OnCarSpawned?.Invoke(car);
        }

        // tr: Araç havuza döndüğünde aktif listeden çıkar.
        public void RemoveCarFromActive(GameObject car)
        {
            activeCarsOnRoad.Remove(car);
        }

        // tr: Merge ile doğan yeni aracı aktif listeye ekler (kapasite hesabı için).
        public void AddCarToActive(GameObject car)
        {
            if (car != null && !activeCarsOnRoad.Contains(car))
                activeCarsOnRoad.Add(car);
        }

        // tr: Level geçişinde kritik temizlik. Eski level'in araçlarını havuza geri yollar (gizler).
        // tr: Böylece yeni environment yüklendiğinde "eski yolun" araçları saçmalamaz.
        public void ReturnAllActiveCarsToPool()
        {
            // tr: LevelManager.LoadLevel en başta bunu çağırır.
            // tr: Amaç: Eski levelin araçları yeni level yolunda kalıp karışıklık yaratmasın.
            // tr: Spawn rutini dursun; temizlik sırasında yeni araç gelmesin.
            StopSpawnRoutine();
            isLevelReady = false;

            if (activeCarsOnRoad == null || activeCarsOnRoad.Count == 0) return;
            if (ObjectPoolManager.Instance == null)
            {
                Debug.LogError("[TrafficManager] ReturnAllActiveCarsToPool failed: ObjectPoolManager.Instance is null.");
                return;
            }

            // tr: Listeyi iterate ederken değiştirmemek için kopya alıyoruz.
            List<GameObject> snapshot = new List<GameObject>(activeCarsOnRoad);

            foreach (GameObject car in snapshot)
            {
                if (car == null) continue;

                CarAgent agent = car.GetComponent<CarAgent>();
                if (agent != null && agent.carData != null && !string.IsNullOrEmpty(agent.carData.poolId))
                {
                    ObjectPoolManager.Instance.ReturnToPool(agent.carData.poolId, car);
                }
                else
                {
                    // tr: Havuz ID'si bulunamazsa güvenli fallback: sahnede görünmesin.
                    car.SetActive(false);
                }
            }

            // tr: Aktif araç sayısını sıfırla (kapasite ve spawn hesapları için).
            activeCarsOnRoad.Clear();
        }

        private void HandleCarMerged(int newTierIndex)
        {
            // tr: İleride birleşen araçların listeden çıkarılması burada yapılacak.
        }
    }
}
