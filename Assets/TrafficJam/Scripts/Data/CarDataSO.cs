using UnityEngine;

namespace TrafficJam.Data
{
    // tr: Araç tanım verisi (ScriptableObject).
    // tr: Bu dosya "araba prefabı" değildir; sadece arabanın istatistiklerini taşır.
    // tr: Nerede kullanılır?
    // tr: - CarAgent: hızı (baseSpeed) ve gelirini (incomePerLap) buradan okur.
    // tr: - MergeController: tier değerine göre birleşmeye izin verir.
    // tr: - ObjectPoolManager: poolId ile doğru prefab instance'ını havuzdan çeker/iade eder.
    [CreateAssetMenu(fileName = "New Car Data", menuName = "Traffic Jam/Data/Car Data")]
    public class CarDataSO : ScriptableObject
    {
        [Header("General Info")]
        // tr: Aracın seviyesi (1, 2, 3 gibi). Merge sisteminde aynı Tier araçlar birleşecek.
        public int tier;
        
        // tr: Arayüzde veya loglarda göstermek için aracın ismi (gameplay için zorunlu değil, daha çok debug/UI).
        public string carName;
        
        // tr: ObjectPoolManager'da bu aracı çekerken kullanacağımız benzersiz ID.
        // tr: Örn: "Car_Tier1" => havuzda bu ID ile tanımlı prefab spawn edilir.
        public string poolId;

        [Header("Economy & Gameplay")]
        // tr: Bu araç 1 tur tamamladığında oyuncuya kazandıracağı para.
        // tr: EconomyManager bu değeri EventManager.OnCarCompletedLap ile alır ve parayı artırır.
        public int incomePerLap;
        
        // tr: Aracın yolda ilerleme taban hızı.
        // tr: CarAgent.Update içinde MoveTowards ile bu hız kullanılır.
        public float baseSpeed;
    }
}
