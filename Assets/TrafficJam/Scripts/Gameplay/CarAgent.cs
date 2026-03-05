using System.Collections.Generic;
using TrafficJam.Core;
using TrafficJam.Data;
using UnityEngine;

namespace TrafficJam.Gameplay
{
    // tr: Objekt havuzundan spawn edilen araçların rotayı takip edip ilerlemesini sağlayan AI kodu.
    public class CarAgent : MonoBehaviour
    {
        [Header("Data References")]
        // tr: Aracın özelliklerini (seviye, hız, gelir) tutan ScriptableObject referansı.
        public CarDataSO carData;

        // tr: Performans optimizasyonu: Waypoint geçişlerinde mesafeyi hesaplarken kullanacağımız hata payı.
        private const float DistanceThreshold = 0.1f;

        private List<Transform> waypoints;
        private int currentWaypointIndex = 0;
        private bool isMoving = false;

        private void OnEnable()
        {
            // tr: Object Pool'dan obje aktif edildiğinde rotayı sıfırlıyoruz.
            currentWaypointIndex = 0;
            
            // tr: Rotayı PathManager'dan alıyoruz. Eğer henüz PathManager veya rota yoksa hareket başlamaz.
            if (PathManager.Instance != null)
            {
                waypoints = PathManager.Instance.GetWaypoints();
                if (waypoints.Count > 0)
                {
                    // tr: Arabayı ilk noktanın (başlangıç çizgisinin) pozisyonuna ışınla.
                    transform.position = waypoints[0].position;
                    // tr: İlk noktadan sonraki noktaya doğru yönelt.
                    if (waypoints.Count > 1) transform.LookAt(waypoints[1]);
                    
                    isMoving = true;
                }
            }
        }

        private void Update()
        {
            // tr: Eğer oyun duraklatılmışsa veya araç durmuşsa hareketi kes.
            if (!isMoving || GameManager.Instance.CurrentState != GameState.Playing) return;
            if (waypoints == null || waypoints.Count == 0 || carData == null) return;

            MoveTowardsNextWaypoint();
        }

        private void MoveTowardsNextWaypoint()
        {
            Transform targetWaypoint = waypoints[currentWaypointIndex];

            // tr: Ağır Unity fizikleri (Rigidbody/AddForce) yerine mobil optimizasyona uygun basit vektör matematiği.
            transform.position = Vector3.MoveTowards(transform.position, targetWaypoint.position, carData.baseSpeed * Time.deltaTime);

            // tr: Hedefe yavaşça dönme (Rotasyon). Snap olmaması için Slerp kullanıyoruz.
            Vector3 direction = (targetWaypoint.position - transform.position).normalized;
            if (direction != Vector3.zero)
            {
                Quaternion endRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, endRotation, Time.deltaTime * 10f);
            }

            // tr: Hedef noktaya ulaştıysak ve mesafe toleransın altındaysa indeks artır.
            if (Vector3.Distance(transform.position, targetWaypoint.position) < DistanceThreshold)
            {
                currentWaypointIndex++;

                // tr: Son waypoint'e (Gişeye) ulaştıysa...
                // tr: Idle oyun döngüsünde yoldan çıkan araç tekrar başa döner ve o esnada para fırlatır.
                if (currentWaypointIndex >= waypoints.Count)
                {
                    CompleteLap();
                }
            }
        }

        private void CompleteLap()
        {
            // tr: Aracın kazandırdığı parayı EconomyManager'a ve diğer sistemlere bildir.
            EventManager.OnCarCompletedLap?.Invoke(carData.incomePerLap);

            // tr: Döngüyü başa sarıp aracın yolda kalmasını sağlıyoruz (Traffic Jam tipi idle mantığı).
            // İleride aracı despawn edip havuzda bekletme istersen: ObjectPoolManager.Instance.ReturnToPool(carData.poolId, gameObject);
            currentWaypointIndex = 0;
            if (waypoints.Count > 0)
            {
                transform.position = waypoints[0].position;
            }
        }
    }
}
