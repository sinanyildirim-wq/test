using System;
using UnityEngine;

namespace TrafficJam.Core
{
    // tr: Oyundaki tüm önemli olayları (Event) merkezi bir yerde tutan statik EventManager sınıfı.
    // tr: Sistemlerin birbirini doğrudan tanımasını (tight coupling) engeller. Loose coupling sağlar.
    public static class EventManager
    {
        // tr: Araç Object Pool'dan çekilip yola eklendiğinde tetiklenir.
        public static Action<GameObject> OnCarSpawned;

        // tr: Para/Bakiye miktarı değiştiğinde tetiklenir. İlk parametre değişim miktarı, ikincisi yeni toplam bakiye.
        public static Action<int, int> OnMoneyChanged;

        // tr: İki araç başarıyla birleştirildiğinde (Merge) tetiklenir. Parametre, yeni aracın seviyesi (Tier).
        public static Action<int> OnCarMerged;

        // tr: Turu tamamlayan araç gişeden/bitişten geçtiğinde tetiklenir. Parametre: Kazanılan Para.
        public static Action<int> OnCarCompletedLap;

        // tr: Yeni bir yol veya gişe upgrade edildiğinde tetiklenir.
        public static Action<int> OnRoadUpgraded;

        // tr: Sahne yeniden yüklendiğinde eski event dinleyicilerini temizlemek için kullanılır (Memory leak önlemi).
        public static void ClearAllEvents()
        {
            OnCarSpawned = null;
            OnMoneyChanged = null;
            OnCarMerged = null;
            OnCarCompletedLap = null;
            OnRoadUpgraded = null;
        }
    }
}
