using System;
using UnityEngine;

namespace TrafficJam.Core
{
    // tr: Tüm sistemler arası iletişimi sağlayan merkezi olay yöneticisi. Loose coupling mimarisi.
    public static class EventManager
    {
        public static Action<GameObject> OnCarSpawned;
        public static Action<int, int> OnMoneyChanged;
        public static Action<int> OnCarMerged;
        public static Action<int> OnCarCompletedLap;
        public static Action<int, GameObject> OnDragStarted;
        public static Action OnDragEnded;
        public static Action<GameState> OnGameStateChanged;
        public static Action OnLevelLoaded;

        // tr: Level progression (bar) için event'ler.
        // normalizedProgress: 0..1 aralığında.
        public static Action<float> OnLevelProgressChanged;
        public static Action OnLevelProgressReady; // tr: Bar doldu, "LEVEL UP" butonu gösterilebilir.
        public static Action OnLevelProgressConsumed; // tr: Level up'a basıldı; bar resetlendi.

        public static void ClearAllEvents()
        {
            OnCarSpawned = null;
            OnMoneyChanged = null;
            OnCarMerged = null;
            OnCarCompletedLap = null;
            OnDragStarted = null;
            OnDragEnded = null;
            OnGameStateChanged = null;
            OnLevelLoaded = null;
            OnLevelProgressChanged = null;
            OnLevelProgressReady = null;
            OnLevelProgressConsumed = null;
        }
    }
}
