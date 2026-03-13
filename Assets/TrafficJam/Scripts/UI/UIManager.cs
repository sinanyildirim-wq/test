using UnityEngine;
using TMPro;
using DG.Tweening;
using TrafficJam.Core;

namespace TrafficJam.UI
{
    // tr: Para göstergesi vb. arayüz elemanlarını yöneten Singleton sınıfı.
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("UI References")]
        public TextMeshProUGUI moneyText; // tr: Para miktarını gösterecek Text objesi.

        private void Awake()
        {
            if (Instance != null && Instance != this) //
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            
            if (moneyText == null)
            {
                Debug.LogError("[UIManager] tr: moneyText referansı atanmamış!");
            }
        }

        private void OnEnable()
        {
            // tr: Para değiştiğinde UI'ı güncellemek için event'e abone oluyoruz.
            if (EventManager.OnMoneyChanged != null)
                EventManager.OnMoneyChanged += UpdateMoneyUI;
            else
                EventManager.OnMoneyChanged = UpdateMoneyUI; // tr: null check ve ilk atama

            Debug.Log("[UIManager] tr: Başlatıldı ve OnMoneyChanged eventine abone olundu.");
        }

        private void OnDisable()
        {
            // tr: Hafıza sızıntısını önlemek için abonelikten çıkıyoruz.
            if (EventManager.OnMoneyChanged != null)
            {
                EventManager.OnMoneyChanged -= UpdateMoneyUI;
            }
        }

        // tr: Para miktarını ekranda güncelleyen ve animasyon ekleyen metod.
        private void UpdateMoneyUI(int changeAmount, int totalMoney)
        {
            if (moneyText == null) return;

            moneyText.text = totalMoney.ToString() + " $";
            
            // tr: DOTween PunchScale ile "parlamış/vurgulanmış" hissi veriyoruz.
            moneyText.transform.DOPunchScale(new Vector3(0.15f, 0.15f, 0.15f), 0.3f, 10, 1)
                .OnComplete(() => moneyText.transform.localScale = Vector3.one); // tr: Ölçeğin sıfırlanmasını garanti et.

            Debug.Log($"[UIManager] tr: Para güncellendi: {totalMoney}. Değişim: {changeAmount}");
        }
    }
}
