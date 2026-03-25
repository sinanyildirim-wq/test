using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using TrafficJam.Gameplay;
using TrafficJam.Core;

namespace TrafficJam.Editor
{
    // tr: Sahnede Shop ve Offline Kazanç UI panellerini otomatik olarak oluşturan Editor aracı.
    public class AutoUISetup : UnityEditor.Editor
    {
        [MenuItem("TrafficJam/Generate UI Panels")]
        public static void GenerateUIPanels()
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[AutoUISetup] tr: Sahnede aktif Canvas bulunamadı!");
                return;
            }

            CreateShopPanel(canvas);
            CreateOfflineEarningsPanel(canvas);

            Debug.Log("[AutoUISetup] tr: UI Panelleri başarıyla oluşturuldu! Butonların OnClick bağlantılarını Inspector üzerinden yapmanız gerekiyor — aşağıdaki adımları takip edin.");
            Debug.Log("[AutoUISetup] Shop > BuySpeedBtn -> ShopManager.TryBuySpeedUpgrade()");
            Debug.Log("[AutoUISetup] Shop > BuyIncomeBtn -> ShopManager.TryBuyIncomeUpgrade()");
            Debug.Log("[AutoUISetup] OfflinePanel > ClaimBtn -> OfflineEarningsManager.ClaimEarnings()");
        }

        private static void CreateShopPanel(Canvas canvas)
        {
            // Ana panel (Alt ekrana yapışık)
            GameObject shopPanel = new GameObject("ShopPanel", typeof(RectTransform), typeof(Image));
            shopPanel.transform.SetParent(canvas.transform, false);
            RectTransform rt = shopPanel.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(1, 0);
            rt.pivot = new Vector2(0.5f, 0);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(0, 200);
            shopPanel.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 0.85f);

            // Yatay düzenleme
            HorizontalLayoutGroup layout = shopPanel.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(20, 20, 20, 20);
            layout.spacing = 20;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;

            // Hız Yükseltme Butonu
            CreateShopButton(shopPanel, "BuySpeedBtn", "⚡ Hız Yükselt");
            // Gelir Yükseltme Butonu
            CreateShopButton(shopPanel, "BuyIncomeBtn", "💰 Gelir Yükselt");

            EditorUtility.SetDirty(shopPanel);
        }

        private static void CreateShopButton(GameObject parent, string btnName, string label)
        {
            GameObject btnObj = new GameObject(btnName, typeof(RectTransform), typeof(Image), typeof(Button));
            btnObj.transform.SetParent(parent.transform, false);
            btnObj.GetComponent<Image>().color = new Color(0.2f, 0.6f, 1f, 1f);

            GameObject textObj = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            textObj.transform.SetParent(btnObj.transform, false);
            RectTransform trt = textObj.GetComponent<RectTransform>();
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.offsetMin = Vector2.zero;
            trt.offsetMax = Vector2.zero;
            TextMeshProUGUI tmp = textObj.GetComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontSize = 22;
            tmp.color = Color.white;
        }

        private static void CreateOfflineEarningsPanel(Canvas canvas)
        {
            // Ekran ortası modal panel
            GameObject panel = new GameObject("OfflineEarningsPanel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(canvas.transform, false);
            RectTransform rt = panel.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(600, 400);
            panel.GetComponent<Image>().color = new Color(0.08f, 0.08f, 0.12f, 0.95f);

            // Başlık yazısı
            GameObject titleObj = new GameObject("TitleText", typeof(RectTransform), typeof(TextMeshProUGUI));
            titleObj.transform.SetParent(panel.transform, false);
            RectTransform trt = titleObj.GetComponent<RectTransform>();
            trt.anchorMin = new Vector2(0, 0.55f);
            trt.anchorMax = new Vector2(1, 1f);
            trt.offsetMin = new Vector2(20, 0);
            trt.offsetMax = new Vector2(-20, -20);
            TextMeshProUGUI titleTmp = titleObj.GetComponent<TextMeshProUGUI>();
            titleTmp.text = "Yokken Şu Kadar Kazandın! 🎉\n<size=36><b>+0 $</b></size>";
            titleTmp.alignment = TextAlignmentOptions.Center;
            titleTmp.fontSize = 28;
            titleTmp.color = Color.white;

            // "Al" (CLAIM) butonu
            GameObject btnObj = new GameObject("ClaimBtn", typeof(RectTransform), typeof(Image), typeof(Button));
            btnObj.transform.SetParent(panel.transform, false);
            RectTransform brt = btnObj.GetComponent<RectTransform>();
            brt.anchorMin = new Vector2(0.2f, 0.1f);
            brt.anchorMax = new Vector2(0.8f, 0.4f);
            brt.offsetMin = Vector2.zero;
            brt.offsetMax = Vector2.zero;
            btnObj.GetComponent<Image>().color = new Color(0.2f, 0.85f, 0.4f, 1f);

            GameObject claimTextObj = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            claimTextObj.transform.SetParent(btnObj.transform, false);
            RectTransform claimTrt = claimTextObj.GetComponent<RectTransform>();
            claimTrt.anchorMin = Vector2.zero;
            claimTrt.anchorMax = Vector2.one;
            claimTrt.offsetMin = Vector2.zero;
            claimTrt.offsetMax = Vector2.zero;
            TextMeshProUGUI claimTmp = claimTextObj.GetComponent<TextMeshProUGUI>();
            claimTmp.text = "AL (CLAIM)";
            claimTmp.alignment = TextAlignmentOptions.Center;
            claimTmp.fontSize = 28;
            claimTmp.fontStyle = FontStyles.Bold;
            claimTmp.color = Color.white;

            // Başlangıçta panel kapalı
            panel.SetActive(false);

            EditorUtility.SetDirty(panel);
        }
    }
}
