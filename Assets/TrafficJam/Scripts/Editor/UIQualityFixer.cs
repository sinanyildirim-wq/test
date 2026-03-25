using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

namespace TrafficJam.Editor
{
    public class UIQualityFixer : UnityEditor.Editor
    {
        [MenuItem("TrafficJam/Fix UI Quality")]
        public static void FixUIQuality()
        {
            // Kullanıcının belirttiği veya projede olan prefab yolu
            string prefabPath = "Assets/TrafficJam/Prefabs/FloatingText_Prefab.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            
            // Eğer o isimde yoksa, projede tespit ettiğimiz gerçek isme (VFX_FloatingText) bakalım
            if (prefab == null)
            {
                prefabPath = "Assets/TrafficJam/Prefabs/VFX_FloatingText.prefab";
                prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            }

            // GÖREV 1 & 3: Floating Text Prefab Optimizasyonu ve Güvenlik
            if (prefab != null)
            {
                if (prefab.GetComponentInChildren<Text>(true) != null)
                {
                    Debug.LogWarning("Lütfen Legacy Text yerine TextMeshPro kullanın!");
                }

                // Ölçekleri tamamen 1 yap
                prefab.transform.localScale = Vector3.one;

                RectTransform rectTransform = prefab.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    rectTransform.localScale = Vector3.one;
                    // Kutuyu yazının sığacağı küçük bir boyuta genişlet (veya 500x200 gibi piksel boyutu da olabilir, ama 5x2 dendiği için onu kullanıyoruz)
                    // Ancak UI için 5x2 çok küçük olabilir. Kullanıcının istediği örneğe (5,2) sadık kalıyoruz.
                    rectTransform.sizeDelta = new Vector2(5f, 2f);
                }

                TMP_Text tmpText = prefab.GetComponentInChildren<TMP_Text>(true);
                if (tmpText != null)
                {
                    tmpText.fontSize = 12;
                    tmpText.alignment = TextAlignmentOptions.Center; // Hem Center hem Middle ortalaması
                }

                EditorUtility.SetDirty(prefab);
            }
            else
            {
                Debug.LogError($"[UIQualityFixer] Yüzen Yazı Prefab'ı bulunamadı. Lütfen ismini kontrol edin.");
            }

            // GÖREV 2: Canvas Scaler Çözünürlük Sabitleme (HD UI)
            Canvas[] canvases = FindObjectsOfType<Canvas>();
            foreach (Canvas canvas in canvases)
            {
                if (canvas.isRootCanvas)
                {
                    CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
                    if (scaler != null)
                    {
                        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                        scaler.referenceResolution = new Vector2(1080, 1920);
                        scaler.matchWidthOrHeight = 0.5f;

                        EditorUtility.SetDirty(scaler);
                    }
                }
            }

            AssetDatabase.SaveAssets();
            Debug.Log("UI ve Text kalitesi HD standartlarına getirildi!");
        }
    }
}
