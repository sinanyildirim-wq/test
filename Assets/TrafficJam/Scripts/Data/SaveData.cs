using System;

namespace TrafficJam.Data
{
    // tr: Oyunun kayıt dosyasına JSON olarak dönüştürülecek verileri taşır.
    [Serializable]
    public class SaveData
    {
        public int currentMoney = 0;
        public int currentLevelIndex = 1;
        public int speedUpgradeLevel = 1;
        public int incomeUpgradeLevel = 1;
        public string lastLoginTime = "";
    }
}
