using UnityEngine;

namespace TrafficJam.Core
{
    // tr: Oyun hissiyatı (Game Feel) için ses efektlerini oynatan Singleton yapı.
    [RequireComponent(typeof(AudioSource))]
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Ses Klipleri")]
        [SerializeField] private AudioClip coinSound;
        [SerializeField] private AudioClip mergeSound;
        [SerializeField] private AudioClip clickSound;

        private AudioSource audioSource;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            audioSource = GetComponent<AudioSource>();
        }

        private void OnEnable()
        {
            EventManager.OnCarCompletedLap += HandleCarCompletedLapAudio;
            EventManager.OnCarMerged += HandleCarMergedAudio;
            
            // tr: İstenirse buton tıklamaları için de global bir click event dinlenebilir.
        }

        private void OnDisable()
        {
            EventManager.OnCarCompletedLap -= HandleCarCompletedLapAudio;
            EventManager.OnCarMerged -= HandleCarMergedAudio;
        }

        // tr: Tur bitince para sesi oynat.
        private void HandleCarCompletedLapAudio(int baseIncome, Vector3 carPosition)
        {
            if (coinSound != null)
            {
                audioSource.PlayOneShot(coinSound);
            }
        }

        // tr: Merge olunca birleşme efekti sesi oynat.
        private void HandleCarMergedAudio(int nextTier, Vector3 mergePosition)
        {
            if (mergeSound != null)
            {
                audioSource.PlayOneShot(mergeSound);
            }
        }

        // tr: Kullanıcı arayüzünde butonlara basıldığında çağrılacak public metod.
        public void PlayClickSound()
        {
            if (clickSound != null)
            {
                audioSource.PlayOneShot(clickSound);
            }
        }
    }
}
