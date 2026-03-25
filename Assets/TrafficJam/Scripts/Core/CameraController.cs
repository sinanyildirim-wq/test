using UnityEngine;
using DG.Tweening;
using TrafficJam.Gameplay;
using System.Collections.Generic;

namespace TrafficJam.Core
{
    // tr: Akıllı Kamera Yöneticisi - Manuel hedefler yerine haritanın tamamını ekrana sığdıracak şekilde kamerayı ayarlar.
    public class CameraController : MonoBehaviour
    {
        public static CameraController Instance { get; private set; }

        [Header("Camera Auto-Framing Settings")]
        [Tooltip("tr: Kameranın merkeze olan izometrik uzaklığı/açısı.")]
        public Vector3 cameraOffset = new Vector3(0, 20f, -20f);
        
        [Tooltip("tr: Ekran kenarlarında kalacak boşluk payı.")]
        public float padding = 5f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void OnEnable()
        {
            EventManager.OnLevelLoaded += FitCameraToLevel;
        }

        private void OnDisable()
        {
            EventManager.OnLevelLoaded -= FitCameraToLevel;
        }

        public void FitCameraToLevel()
        {
            if (PathManager.Instance == null) return;
            
            List<Transform> waypoints = PathManager.Instance.GetWaypoints();
            if (waypoints == null || waypoints.Count == 0) return;

            // tr: Kullanıcının istediği spesifik hesaplama:
            // Sadece İlk (Point 1) ve Son (Last Point) noktayı baz alıyoruz.
            Transform firstPoint = waypoints[0];
            Transform lastPoint = waypoints[waypoints.Count - 1];

            // 1. Ekranın tam ortasına gelmesini istediğimiz merkez noktayı hesapla (İlk ve Son noktanın tam orta noktası)
            Vector3 centerPoint = (firstPoint.position + lastPoint.position) / 2f;

            // 2. Bu iki nokta arasındaki genişliği / mesafeyi hesapla
            float totalDistance = Vector3.Distance(firstPoint.position, lastPoint.position);

            Camera mainCam = Camera.main;
            if (mainCam == null) return;

            // Var olan tween'leri çakışmaması için iptal edelim.
            mainCam.transform.DOKill();
            mainCam.DOKill();

            // 3. Kameranın gitmesi gereken hedef pozisyonu bul (Ortalanmış nokta + kameranın yukarı/geri açısı)
            Vector3 targetPos = centerPoint + cameraOffset;

            // 4. Kamerayı DOTween ile yumuşakça götür ve MERKEZE BAKACAK şekilde açısını (Rotation) düzelt.
            // Rotasyon düzeltilmezse Raycast (Araç Sürükleme) ufuk çizgisine doğru ateş edip bozulur!
            mainCam.transform.DOMove(targetPos, 1.5f).SetEase(Ease.InOutSine);
            
            Vector3 lookDirection = centerPoint - targetPos;
            if (lookDirection != Vector3.zero)
            {
                Quaternion targetRot = Quaternion.LookRotation(lookDirection);
                mainCam.transform.DORotateQuaternion(targetRot, 1.5f).SetEase(Ease.InOutSine);
            }

            // 5. Görüş açısını (Zoom miktarını) ilk ve son nokta arasındaki mesafeye göre ayarla
            if (mainCam.orthographic)
            {
                // tr: Mesafe arttıkça kamera geriye gidecek. distance'ın yarısını baz alıp padding ekliyoruz ki arabalar ekrandan taşmasın.
                float targetOrthoSize = (totalDistance / 2f) + padding;
                
                mainCam.DOOrthoSize(targetOrthoSize, 1.5f).SetEase(Ease.InOutSine);
            }
        }
    }
}
