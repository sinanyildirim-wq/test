using UnityEngine;

namespace TrafficJam.Data
{
    // tr: Araçların özelliklerini Unity Editor (Inspector) üzerinden kolayca yaratıp düzenlememizi sağlayan veri sınıfı.
    [CreateAssetMenu(fileName = "New Car Data", menuName = "Traffic Jam/Data/Car Data")]
    public class CarDataSO : ScriptableObject
    {
        [Header("General Info")]
        // tr: Aracın seviyesi (1, 2, 3 gibi). Merge sisteminde aynı Tier araçlar birleşecek.
        public int tier;
        
        // tr: Arayüzde veya loglarda göstermek için aracın ismi.
        public string carName;
        
        // tr: ObjectPoolManager'da bu aracı çekerken kullanacağımız benzersiz ID (Örn: "Car_Tier1").
        public string poolId;

        [Header("Economy & Gameplay")]
        // tr: Bu araç gişeden her geçtiğinde (bir turu tamamladığında) oyuncuya kazandıracağı para miktarı.
        public int incomePerLap;
        
        // tr: Aracın yolda ilerleme taban hızı.
        public float baseSpeed;
    }
}
