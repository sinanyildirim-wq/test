using System.Collections;
using System.Collections.Generic;
using TrafficJam.Core;
using TrafficJam.Data;
using UnityEngine;

namespace TrafficJam.Gameplay
{
    // tr: Oyundaki araç trafiğini yöneten, havuzdan belirli aralıklarla araç spawn eden Singleton sınıf.
    public class TrafficManager : MonoBehaviour
    {
        public static TrafficManager Instance { get; private set; }

        [Header("Settings & Data")]
        // tr: Sahnedeki max kapasite sınırını bilmek için yola ait genel data.
        [SerializeField] private LevelDataSO currentLevelData;
        
        // tr: Araçların yola çıkmadan önce ne kadar süre bekleyeceğini (Spawn Rate) belirler.
        [SerializeField] private float spawnInterval = .5f;

        // tr: Sahnede aktif olan (yolda gezen) araçların listesi. Limit kontrolü için gereklidir.
        private List<GameObject> activeCarsOnRoad = new List<GameObject>();

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
            EventManager.OnCarMerged += HandleCarMerged;
        }

        private void OnDisable()
        {
            EventManager.OnCarMerged -= HandleCarMerged;
        }

        private void Start()
        {
            // tr: Sürekli araba üreten coroutine tetiklenir.
            StartCoroutine(SpawnCarRoutine());
        }

        private IEnumerator SpawnCarRoutine()
        {
            while (true)
            {
                // tr: Oyun Playing durumundaysa ve hala yolda eklenebilecek araç kapasitesi varsa araç spawn et.
                if (GameManager.Instance.CurrentState == GameState.Playing && currentLevelData != null)
                {
                    // FAIL-SAFE
                    if (PathManager.Instance == null || PathManager.Instance.GetWaypoints().Count == 0)
                    {
                        Debug.LogError("[TrafficManager] tr: Waypoint listesi BOŞ! Lütfen PathManager'a waypointleri atayın.");
                        yield return new WaitForSeconds(2f);
                        continue;
                    }

                    if (activeCarsOnRoad.Count < currentLevelData.maxCarCapacity)
                    {
                        SpawnCar("Car_Tier1");
                    }
                }

                // tr: Belirtilen saniye kadar bekle.
                yield return new WaitForSeconds(spawnInterval);
            }
        }

        // tr: ObjectPoolManager kullanarak spawn işlemini yapar.
        private void SpawnCar(string poolId)
        {
            Transform startPoint = PathManager.Instance.GetWaypoints()[0];
            
            // tr: ObjectPool'dan aracı al.
            GameObject car = ObjectPoolManager.Instance.SpawnFromPool(poolId, startPoint.position, startPoint.rotation);

            if (car != null)
            {
                if (!activeCarsOnRoad.Contains(car))
                {
                    activeCarsOnRoad.Add(car);
                }
                
                // tr: Araç başarıyla yola çıktığını global olarak haber ver (örnek: UI'ı güncellemek için).
                EventManager.OnCarSpawned?.Invoke(car);
                Debug.Log($"[TrafficManager] tr: Araç spawn edildi. Aktif araç sayısı: {activeCarsOnRoad.Count}");
            }
        }

        // tr: Arabalar birleştiğinde (Merge) eski arabalar yok olur, yenisi çıkacaktır.
        private void HandleCarMerged(int newTierIndex)
        {
            // İleride burada birleşen arabaları listeden çıkarıp üst seviye arabayı listeye / havuza ekleme kodları olacak.
            // Örnek: activeCarsOnRoad.Remove(eskiAraba1); // vesaire
            Debug.Log($"[TrafficManager] tr: Arabalar birleşti ve Tier {newTierIndex} oluşturuldu. Gerekli işlemler burada yapılacak.");
        }
    }
}
