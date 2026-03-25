using System;
using UnityEngine;

namespace TrafficJam.Core
{
    // tr: Tüm sistemler arası iletişim noktası.
    // tr: Amaç: Birbirini tanımayan sistemlerin (UI, trafik, ekonomi, level) Action event'leri üzerinden haberleşmesi.
    // tr: Not: Event'ler static olduğu için sahne reload / play-stop sırasında "eski aboneler" kalmasın diye ClearAllEvents kullanılır.
    public static class EventManager
    {
        public static Action<GameObject> OnCarSpawned;
        public static Action<int, int> OnMoneyChanged;
        public static Action<int, Vector3> OnCarMerged;
        public static Action<int, Vector3> OnCarCompletedLap;
        public static Action<int, GameObject> OnDragStarted;
        public static Action OnDragEnded;
        public static Action<GameState> OnGameStateChanged;
        public static Action OnLevelLoaded;

        // tr: Level progression (bar) için event'ler.
        // normalizedProgress: 0..1 aralığında.
        public static Action<float> OnLevelProgressChanged;
        public static Action OnLevelProgressReady; // tr: Bar doldu, "LEVEL UP" butonu gösterilebilir.
        public static Action OnLevelProgressConsumed; // tr: Level up'a basıldı; bar resetlendi.

        // tr: Phase 4 - Çevrimdışı kazanç hesaplandığında UI popup için tetiklenir. Parametre: kazanılan miktar.
        public static Action<int> OnOfflineEarningsCalculated;

        public static void ClearAllEvents()
        {
            // tr: Statik event'leri sıfırlayarak "ghost listener" (eski sahneden kalan aboneler) riskini azaltır.
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
            OnOfflineEarningsCalculated = null;
        }
    }
}
