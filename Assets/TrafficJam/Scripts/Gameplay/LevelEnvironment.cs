using System.Collections.Generic;
using UnityEngine;

namespace TrafficJam.Gameplay
{
    // tr: Level environment "data container" (opsiyonel).
    // tr: Bazı level prefablarında waypoint listesi PathManager component'i üzerinden tutulur.
    // tr: Eğer istersen waypointleri doğrudan burada da tutabilirsin:
    // tr: - LevelManager önce prefab içindeki PathManager'ı arar
    // tr: - Bulamazsa bu komponenti (LevelEnvironment) fallback olarak kullanır
    public class LevelEnvironment : MonoBehaviour
    {
        [Header("Route Settings")]
        [Tooltip("tr: Bu leveldeki araçların takip edeceği noktalar sırasıyla buraya eklenmeli.")]
        public List<Transform> levelWaypoints;
    }
}