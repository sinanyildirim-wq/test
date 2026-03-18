using System.Collections.Generic;
using UnityEngine;

namespace TrafficJam.Core
{
    // tr: Inspector'dan havuz (pool) tanımlamak için veri yapısı.
    // tr: Her PoolItem: bir prefab + başlangıç adedi + hiyerarşide nereye parent'lanacağı.
    [System.Serializable]
    public class PoolItem
    {
        public string poolId; // tr: "Car_Tier1", "FloatingMoney" gibi benzersiz ID
        public GameObject prefab;
        public int initialSize;
        public Transform parentTransform; // tr: Hiyerarşiyi oyundayken temiz tutmak için ebeveyn obje
    }

    // tr: Performans optimizasyonu için objeleri Destroy/Instantiate yapmak yerine yeniden kullanan Singleton havuz yöneticisi.
    public class ObjectPoolManager : MonoBehaviour
    {
        public static ObjectPoolManager Instance { get; private set; }

        // tr: Inspector'dan eklenecek havuz tanımları. Car_Tier1, Car_Tier2, FloatingMoney vb.
        [SerializeField] private List<PoolItem> poolItems = new List<PoolItem>();

        // tr: String ID'ye göre Queue tutan asıl havuzumuz.
        private Dictionary<string, Queue<GameObject>> poolDictionary;
        private Dictionary<string, PoolItem> poolItemConfigs;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            // tr: Oyun başında bütün havuzları doldurur. Bu sayede runtime'da Instantiate maliyeti azalır.
            InitializePools();
        }

        // tr: Başlangıçta tüm havuzları verilen 'initialSize' kadar doldurur.
        private void InitializePools()
        {
            poolDictionary = new Dictionary<string, Queue<GameObject>>();
            poolItemConfigs = new Dictionary<string, PoolItem>();

            foreach (var item in poolItems)
            {
                poolItemConfigs.Add(item.poolId, item);
                Queue<GameObject> objectPool = new Queue<GameObject>();

                for (int i = 0; i < item.initialSize; i++)
                {
                    GameObject obj = Instantiate(item.prefab, item.parentTransform);
                    obj.SetActive(false);
                    objectPool.Enqueue(obj);
                }

                poolDictionary.Add(item.poolId, objectPool);
            }
        }

        // tr: Havuzdan obje çeker. Obje bitmişse otomatik genişletir (Expandable logic).
        public GameObject SpawnFromPool(string id, Vector3 position, Quaternion rotation)
        {
            if (!poolDictionary.ContainsKey(id))
            {
                Debug.LogWarning($"[ObjectPoolManager] tr: {id} ID'li havuz bulunamadı!");
                return null;
            }

            // tr: Eğer havuz boşsa, dinamik olarak yeni bir instance oluştur. (Oyun sırasında takılmaları en aza indiririz ama loglarız)
            if (poolDictionary[id].Count == 0)
            {
                // tr: Bu durum "initialSize düşük" veya "aynı anda çok fazla spawn" demektir.
                Debug.Log($"[ObjectPoolManager] tr: {id} havuzu boşaldı, kapasite otomatik genişletiliyor (Expandable). Lütfen InitialSize'ı artırmayı düşünün.");
                PoolItem config = poolItemConfigs[id];
                GameObject newObj = Instantiate(config.prefab, config.parentTransform);
                newObj.SetActive(false);
                poolDictionary[id].Enqueue(newObj);
            }

            GameObject objectToSpawn = poolDictionary[id].Dequeue();

            objectToSpawn.transform.position = position;
            objectToSpawn.transform.rotation = rotation;
            objectToSpawn.SetActive(true);

            // tr: Not: Spawn edilen objenin "reset" ihtiyacı varsa (anim, state vb.) OnEnable/Initialize metodlarıyla yönetilir.
            return objectToSpawn;
        }

        // tr: Objenin kullanımı bittiğinde objeyi tekrar havuza geri gönderir. Destroy metodu yerine bu kullanılmalıdır.
        public void ReturnToPool(string id, GameObject obj)
        {
            if (!poolDictionary.ContainsKey(id))
            {
                Debug.LogWarning($"[ObjectPoolManager] tr: {id} ID'li havuz bulunamadı! Obje havuza geri atılamadı, doğrudan yok ediliyor.");
                Destroy(obj); // tr: Havuz yapılandırmasında hata varsa memory'de öksüz obje kalmasın.
                return;
            }

            obj.SetActive(false);
            poolDictionary[id].Enqueue(obj);
        }
    }
}
