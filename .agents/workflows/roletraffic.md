---
description: TrafficJamRule
---

BÖLÜM 1: Genel C# ve Kodlama Standartları (Genel Kurallar)
Görseldeki 10 temel kalite standardına (Okunabilirlik, Modülerlik, Tutarlılık vb.) dayanmaktadır.

Dil Sınırları: Tüm sınıf (class), metod ve değişken isimleri KESİNLİKLE İngilizce yazılacaktır. Ancak kod içindeki tüm açıklama satırları ve Debug.Log mesajları KESİNLİKLE Türkçe (// tr:) olacaktır.

İsimlendirme (Naming Conventions):

Sınıflar, Metodlar ve Özellikler (Properties): PascalCase (Örn: TrafficManager, CalculateIncome())

Değişkenler ve Parametreler: camelCase (Örn: spawnInterval, carData)

Sabitler (Constants): PASCAL_SNAKE_CASE (Örn: MAX_CAR_CAPACITY)

Hardcode Yasaktır (No Magic Numbers): Kodun içine rastgele 15, 2.5f, "Player" gibi değerler yazılamaz. Bu değerler Inspector'dan ayarlanabilen değişkenler (SerializeField) veya ScriptableObject'ler üzerinden çekilmelidir.

Tek Sorumluluk Prensibi (Single Responsibility): Bir script sadece bir işi yapmalıdır. Araba hareket ediyorsa (CarAgent), parayı o hesaplamaz; sadece "Turu bitirdim" der. Parayı hesaplama işi EconomyManager'ındır.

BÖLÜM 2: Unity Optimizasyon ve Geliştirme Kuralları
Update Metodu Kullanımı: Update, FixedUpdate veya LateUpdate metodları sadece zorunlu durumlarda (örneğin hareket matematiği) kullanılacaktır. UI güncellemek veya durum kontrol etmek için Update kullanılmaz, Event'ler (Olaylar) kullanılır.

Pahalı Çağrılardan Kaçınma: GetComponent<>, GameObject.Find(), Camera.main gibi işlemciyi yoran kodlar KESİNLİKLE Update içinde kullanılamaz. Bunlar Awake veya Start içinde çağrılıp bir değişkene önbelleklenmelidir (Cache).

UI (Arayüz) Performansı: Panelleri gizlemek için Alpha (saydamlık) değeri 0 yapılmaz. Gerçekten ekranda olmaması gereken objeler gameObject.SetActive(false) ile kapatılır. Canvas'lar statik (değişmeyen) ve dinamik (sürekli güncellenen) olarak ayrılmalıdır.

Draw Call ve Materyal Kuralları: Kod üzerinden renderer.material.color = ... kullanılarak materyal kopyalanması yaratılamaz (Performans katili). Animasyonlar ve renk değişimleri için DOTween veya Shader Property Block kullanılır.

BÖLÜM 3: TrafficJam Özel Mimari Kuralları
Olay Güdümlü Mimari (Event-Driven Architecture): Sistemler birbirini tanımaz. Biri diğerine emir vermez. Her iletişim EventManager.cs üzerinden Action'lar ile sağlanır. (Loose Coupling - Gevşek Bağlantı).

Single Scene Architecture (Tek Sahne Mimarisi): Her level için yeni bir Unity Scene (Sahne) oluşturulmaz. Yollar ve çevre LevelDataSO üzerinden Prefab olarak yüklenir ve silinir. Oyun sadece SampleScene (veya MainScene) üzerinde döner.

Nesne Havuzu (Object Pooling): Sahnede sürekli yaratılıp yok edilen objeler (Arabalar, Particle Efektleri, Yüzen Yazılar) KESİNLİKLE Instantiate ve Destroy kodlarıyla oluşturulamaz. ObjectPoolManager kullanılarak havuzdan çağrılır ve havuza iade edilir.

Veri Yönetimi (Data-Driven): Arabaların hızları, kazançları veya levellerin kapasiteleri scriptlerin içinde değil, ScriptableObjects (Örn: CarDataSO) içinde tutulur.

BÖLÜM 4: MCP (Yapay Zeka Asistan) Kullanım Kuralları
Yapay zeka asistanından kod veya sistem talep ederken aşağıdaki "MCP Talimatları" geçerlidir:

Kuralları Hatırlatma: Büyük bir mimari değişiklik isteneceğinde MCP'ye her zaman "TrafficJam kurallarına (Event-Driven, Object Pooling, Türkçe Yorumlar) sadık kalarak şu sistemi yaz" şeklinde komut verilecektir.

Kapsam Daraltma: MCP'den "Oyunu yap" gibi devasa isteklerde bulunulmaz. "Sadece CarAgent içindeki x fonksiyonunu değiştir" veya "Yeni bir UpgradeManager yaz" gibi atomik (nokta atışı) istekler verilir.

Loglama Zorunluluğu: MCP yeni bir sistem yazdığında, KESİNLİKLE sistemin doğru çalışıp çalışmadığını test edebilmek için kilit noktalara Debug.Log("[SistemAdı] tr: ..."); eklemesi emredilir.

Gereksiz Silme Yasağı: MCP'nin, mevcut çalışan sistemleri "ben daha iyisini yazarım" diyerek habersiz silmesine izin verilmez. MCP sadece istenen bloğu günceller.