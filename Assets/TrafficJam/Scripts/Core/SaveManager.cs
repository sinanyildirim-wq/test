using UnityEngine;
using System;
using TrafficJam.Data;
using TrafficJam.Gameplay;

namespace TrafficJam.Core
{
    // tr: Oyunu kaydetme ve yükleme işlemlerini yönetir (Phase 3).
    // tr: Auto-Save özelliği içerir. (Execution Order'da diğer yöneticilerden önce çalışacak şekilde ayarlanmalıdır).
    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance { get; private set; }
        
        public SaveData Data { get; private set; }
        private const string SAVE_KEY = "TrafficJam_SaveData";

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            LoadGame();
        }

        public void SaveGame()
        {
            if (EconomyManager.Instance != null)
                Data.currentMoney = EconomyManager.Instance.CurrentMoney;

            if (LevelManager.Instance != null && LevelManager.Instance.CurrentLevelData != null)
                Data.currentLevelIndex = LevelManager.Instance.CurrentLevelData.currentLevel;

            if (UpgradeManager.Instance != null)
            {
                Data.incomeUpgradeLevel = UpgradeManager.Instance.IncomeLevel;
                Data.speedUpgradeLevel = UpgradeManager.Instance.SpeedLevel;
            }

            Data.lastLoginTime = DateTime.Now.ToString("O"); // Format: ISO 8601

            string json = JsonUtility.ToJson(Data);
            PlayerPrefs.SetString(SAVE_KEY, json);
            PlayerPrefs.Save();
            Debug.Log("[SaveManager] tr: Oyun başarıyla PlayerPrefs üzerine JSON olarak kaydedildi.");
        }

        public void LoadGame()
        {
            if (PlayerPrefs.HasKey(SAVE_KEY))
            {
                string json = PlayerPrefs.GetString(SAVE_KEY);
                Data = JsonUtility.FromJson<SaveData>(json);
                Debug.Log("[SaveManager] tr: Kaydedilmiş veri yüklendi.");
            }
            else
            {
                Data = new SaveData();
                Data.lastLoginTime = DateTime.Now.ToString("O");
                Debug.Log("[SaveManager] tr: Yeni kayıt (SaveData) oluşturuldu.");
            }
        }

        private void OnApplicationQuit()
        {
            SaveGame();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus) SaveGame(); // Uygulama arka plana atıldığında otomatik kaydet
        }
    }
}
