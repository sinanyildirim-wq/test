using UnityEngine;

namespace TrafficJam.Data
{
    // tr: Yol veya oyuncu seviyelerine ait verileri ve upgrade (geliştirme) gereksinimlerini tutan veri sınıfı.
    [CreateAssetMenu(fileName = "New Level Data", menuName = "Traffic Jam/Data/Level Data")]
    public class LevelDataSO : ScriptableObject
    {
        [Header("Level Info")]
        // tr: Mevcut seviyenin numarası.
        public int currentLevel;
        [Header("Level Environment")]
        public GameObject levelEnvironmentPrefab;

        [Header("Economy & Upgrades")]
        // tr: Bir sonraki seviyeye geçmek (veya yolu geliştirmek) için gereken para miktarı.
        public int upgradeCost;
        
        // tr: Bu seviyede yolda aynı anda bulunabilecek maksimum araç sayısı. Havuz sistemini veya spawn hızını etkiler.
        public int maxCarCapacity;
    }
}
