using System.Collections.Generic;
using UnityEngine;

namespace TrafficJam.Gameplay
{
    // tr: Her bir level prefabının kök (root) objesinde duracak script. Kendi rotasını barındırır.
    public class LevelEnvironment : MonoBehaviour
    {
        [Header("Route Settings")]
        [Tooltip("tr: Bu leveldeki araçların takip edeceği noktalar sırasıyla buraya eklenmeli.")]
        public List<Transform> levelWaypoints;
    }
}