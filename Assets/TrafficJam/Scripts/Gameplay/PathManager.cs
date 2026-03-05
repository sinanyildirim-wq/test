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
            // tr: Singleton kurulumu.
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
    }
}
