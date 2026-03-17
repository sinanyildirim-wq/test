using System.Collections.Generic;
using UnityEngine;

namespace TrafficJam.Gameplay
{
    // tr: Araçların takip edeceği düğüm noktalarını (Waypoint) tutan merkezi sınıf.
    public class PathManager : MonoBehaviour
    {
        public static PathManager Instance { get; private set; }

        [Header("Path Settings")]
        // tr: Araçların sırayla takip edeceği Transform listesi. Unity Inspector'dan sürüklenecek.
        [SerializeField] private List<Transform> waypoints = new List<Transform>();

        private void Awake()
        {
            // tr: Bu component 2 farklı amaçla kullanılabilir:
            // tr: 1) Global PathManager (sahne root'unda) -> Singleton Instance budur.
            // tr: 2) Level prefabının içinde gömülü PathManager -> sadece waypoint "data holder" olarak durur (singleton OLMAZ).

            // tr: Prefab içindeki PathManager (parent'ı varsa) singleton'a dokunmasın.
            if (transform.parent != null)
                return;

            // tr: Global singleton kurulumu.
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        // tr: Dışarıdan rotayı almak isteyen araçlar için listeyi dönen metod.
        public List<Transform> GetWaypoints()
        {
            return waypoints;
        }

        // tr: (İsteğe bağlı) Editor ekranında yolları görebilmek için Gizmos çizimi.
        private void OnDrawGizmos()
        {
            if (waypoints == null || waypoints.Count == 0) return;

            Gizmos.color = Color.green;
            for (int i = 0; i < waypoints.Count; i++)
            {
                if (waypoints[i] != null)
                {
                    Gizmos.DrawSphere(waypoints[i].position, 0.3f);
                    
                    if (i < waypoints.Count - 1 && waypoints[i + 1] != null)
                    {
                        Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
                    }
                }
            }
        }

        // tr: LevelManager yeni bir harita yüklediğinde bu metodu çağırıp yeni rotayı verir.
        public void SetWaypoints(List<Transform> newWaypoints)
        {
            // tr: Defansif kopya: Prefab içindeki başka bir component'in listesini referans almak riskli.
            // tr: Environment destroy olunca Unity bazı serialized list referanslarını temizleyebiliyor; bu da runtime'da "no waypoints" hatasına yol açar.
            waypoints = newWaypoints == null ? new List<Transform>() : new List<Transform>(newWaypoints);
            Debug.Log($"[PathManager] Waypoints updated. Count={(waypoints == null ? 0 : waypoints.Count)}");
        }
    }
}
