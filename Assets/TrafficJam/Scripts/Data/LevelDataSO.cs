using UnityEngine;

namespace TrafficJam.Data
{
    // tr: Level tanım verisi (ScriptableObject).
    // tr: LevelManager bu datayı "hangi level yüklenecek?" sorusunun cevabı olarak kullanır.
    // tr: LevelProgressionManager bu datadaki upgradeCost ile ilerleme barını hedefe bağlar.
    [CreateAssetMenu(fileName = "New Level Data", menuName = "Traffic Jam/Data/Level Data")]
    public class LevelDataSO : ScriptableObject
    {
        [Header("Level Info")]
        // tr: Mevcut seviyenin numarası.
        public int currentLevel;
        [Header("Level Environment")]
        // tr: Sahneye inecek environment prefabı (yol, waypointler, dekor vb.).
        // tr: LevelManager.LoadLevel bu prefabı Instantiate eder.
        public GameObject levelEnvironmentPrefab;

        [Header("Economy & Upgrades")]
        // tr: Bir sonraki seviyeye geçmek (veya yolu geliştirmek) için gereken para miktarı.
        // tr: EconomyManager.TryUpgradeToNextLevel bu cost'u harcar.
        // tr: LevelProgressionManager bu cost'a göre progress hesaplar (earned/cost).
        public int upgradeCost;
        
        // tr: Bu seviyede yolda aynı anda bulunabilecek maksimum araç sayısı. Havuz sistemini veya spawn hızını etkiler.
        // tr: TrafficManager spawn rutininde aktif araç sayısı bu kapasitenin altındaysa yeni araç spawn eder.
        public int maxCarCapacity;
    }
}
