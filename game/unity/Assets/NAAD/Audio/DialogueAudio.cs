using UnityEngine;
using System.Collections.Generic;

namespace NAAD.Audio
{
    /// <summary>
    /// Dialogue audio system for context-appropriate sound effects,
    /// ambient background music, and UI sound cues.
    /// </summary>
    public class DialogueAudio : MonoBehaviour
    {
        public static DialogueAudio Instance { get; private set; }

        [Header("Audio Sources")]
        [SerializeField] private AudioSource dialogueSource;
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource uiSource;

        [Header("Dialogue Sounds")]
        [SerializeField] private AudioClip[] dialogueStartSounds;
        [SerializeField] private AudioClip[] dialogueEndSounds;
        [SerializeField] private AudioClip[] typingSounds;
        [SerializeField] private AudioClip[] responseSelectSounds;

        [Header("Ambient Music")]
        [SerializeField] private AudioClip[] ambientMusicTracks;
        [SerializeField] private float musicFadeDuration = 1f;
        [SerializeField] private float defaultMusicVolume = 0.5f;

        [Header("UI Sound Cues")]
        [SerializeField] private AudioClip uiHoverSound;
        [SerializeField] private AudioClip uiClickSound;
        [SerializeField] private AudioClip uiOpenSound;
        [SerializeField] private AudioClip uiCloseSound;
        [SerializeField] private AudioClip notificationSound;

        [Header("Environmental Audio")]
        [SerializeField] private AudioClip[] locationAmbientTracks;
        [SerializeField] private Dictionary<string, AudioClip> locationAmbience = new Dictionary<string, AudioClip>();

        private int currentMusicTrack = -1;
        private bool isMusicFading;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            SetupAudioSources();
        }

        private void SetupAudioSources()
        {
            if (dialogueSource == null)
            {
                dialogueSource = gameObject.AddComponent<AudioSource>();
            }
            dialogueSource.playOnAwake = false;

            if (musicSource == null)
            {
                musicSource = gameObject.AddComponent<AudioSource>();
            }
            musicSource.playOnAwake = false;
            musicSource.loop = true;
            musicSource.volume = defaultMusicVolume;

            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
            }
            sfxSource.playOnAwake = false;

            if (uiSource == null)
            {
                uiSource = gameObject.AddComponent<AudioSource>();
            }
            uiSource.playOnAwake = false;
        }

        #region Dialogue Sounds

        /// <summary>
        /// Play a random dialogue start sound
        /// </summary>
        public void PlayDialogueStart()
        {
            if (dialogueStartSounds != null && dialogueStartSounds.Length > 0)
            {
                AudioClip clip = dialogueStartSounds[Random.Range(0, dialogueStartSounds.Length)];
                dialogueSource.PlayOneShot(clip);
            }
        }

        /// <summary>
        /// Play a random dialogue end sound
        /// </summary>
        public void PlayDialogueEnd()
        {
            if (dialogueEndSounds != null && dialogueEndSounds.Length > 0)
            {
                AudioClip clip = dialogueEndSounds[Random.Range(0, dialogueEndSounds.Length)];
                dialogueSource.PlayOneShot(clip);
            }
        }

        /// <summary>
        /// Play typing sound for character-by-character dialogue
        /// </summary>
        public void PlayTypingSound()
        {
            if (typingSounds != null && typingSounds.Length > 0)
            {
                AudioClip clip = typingSounds[Random.Range(0, typingSounds.Length)];
                sfxSource.PlayOneShot(clip);
            }
        }

        /// <summary>
        /// Play response selection sound
        /// </summary>
        public void PlayResponseSelect()
        {
            if (responseSelectSounds != null && responseSelectSounds.Length > 0)
            {
                AudioClip clip = responseSelectSounds[Random.Range(0, responseSelectSounds.Length)];
                uiSource.PlayOneShot(clip);
            }
        }

        #endregion

        #region Ambient Music

        /// <summary>
        /// Start playing ambient music with fade in
        /// </summary>
        public void PlayAmbientMusic(int trackIndex = -1)
        {
            if (ambientMusicTracks == null || ambientMusicTracks.Length == 0)
            {
                return;
            }

            if (trackIndex < 0 || trackIndex >= ambientMusicTracks.Length)
            {
                trackIndex = Random.Range(0, ambientMusicTracks.Length);
            }

            if (currentMusicTrack == trackIndex && musicSource.isPlaying)
            {
                return;
            }

            currentMusicTrack = trackIndex;
            musicSource.clip = ambientMusicTracks[trackIndex];
            StartCoroutine(FadeMusicIn());
        }

        /// <summary>
        /// Stop ambient music with fade out
        /// </summary>
        public void StopAmbientMusic()
        {
            StartCoroutine(FadeMusicOut());
        }

        /// <summary>
        /// Fade music in coroutine
        /// </summary>
        private System.Collections.IEnumerator FadeMusicIn()
        {
            if (isMusicFading) yield break;
            isMusicFading = true;

            musicSource.Play();
            float startVolume = 0f;
            musicSource.volume = startVolume;

            float elapsed = 0f;
            while (elapsed < musicFadeDuration)
            {
                elapsed += Time.deltaTime;
                musicSource.volume = Mathf.Lerp(startVolume, defaultMusicVolume, elapsed / musicFadeDuration);
                yield return null;
            }

            musicSource.volume = defaultMusicVolume;
            isMusicFading = false;
        }

        /// <summary>
        /// Fade music out coroutine
        /// </summary>
        private System.Collections.IEnumerator FadeMusicOut()
        {
            if (isMusicFading) yield break;
            isMusicFading = true;

            float startVolume = musicSource.volume;
            float elapsed = 0f;

            while (elapsed < musicFadeDuration)
            {
                elapsed += Time.deltaTime;
                musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / musicFadeDuration);
                yield return null;
            }

            musicSource.volume = 0f;
            musicSource.Stop();
            isMusicFading = false;
        }

        /// <summary>
        /// Set music volume
        /// </summary>
        public void SetMusicVolume(float volume)
        {
            defaultMusicVolume = Mathf.Clamp01(volume);
            if (!isMusicFading)
            {
                musicSource.volume = defaultMusicVolume;
            }
        }

        #endregion

        #region UI Sound Cues

        /// <summary>
        /// Play UI hover sound
        /// </summary>
        public void PlayUIHover()
        {
            if (uiHoverSound != null)
            {
                uiSource.PlayOneShot(uiHoverSound);
            }
        }

        /// <summary>
        /// Play UI click sound
        /// </summary>
        public void PlayUIClick()
        {
            if (uiClickSound != null)
            {
                uiSource.PlayOneShot(uiClickSound);
            }
        }

        /// <summary>
        /// Play UI open sound
        /// </summary>
        public void PlayUIOpen()
        {
            if (uiOpenSound != null)
            {
                uiSource.PlayOneShot(uiOpenSound);
            }
        }

        /// <summary>
        /// Play UI close sound
        /// </summary>
        public void PlayUIClose()
        {
            if (uiCloseSound != null)
            {
                uiSource.PlayOneShot(uiCloseSound);
            }
        }

        /// <summary>
        /// Play notification sound
        /// </summary>
        public void PlayNotification()
        {
            if (notificationSound != null)
            {
                uiSource.PlayOneShot(notificationSound);
            }
        }

        #endregion

        #region Environmental Audio

        /// <summary>
        /// Play location-specific ambient audio
        /// </summary>
        public void PlayLocationAmbience(string locationName)
        {
            if (locationAmbience.TryGetValue(locationName, out AudioClip clip))
            {
                sfxSource.clip = clip;
                sfxSource.loop = true;
                sfxSource.Play();
            }
        }

        /// <summary>
        /// Stop location ambient audio
        /// </summary>
        public void StopLocationAmbience()
        {
            sfxSource.Stop();
        }

        /// <summary>
        /// Register location ambient clip
        /// </summary>
        public void RegisterLocationAmbience(string locationName, AudioClip clip)
        {
            if (!locationAmbience.ContainsKey(locationName))
            {
                locationAmbience.Add(locationName, clip);
            }
            else
            {
                locationAmbience[locationName] = clip;
            }
        }

        #endregion

        #region Master Controls

        /// <summary>
        /// Mute all audio
        /// </summary>
        public void MuteAll()
        {
            dialogueSource.mute = true;
            musicSource.mute = true;
            sfxSource.mute = true;
            uiSource.mute = true;
        }

        /// <summary>
        /// Unmute all audio
        /// </summary>
        public void UnmuteAll()
        {
            dialogueSource.mute = false;
            musicSource.mute = false;
            sfxSource.mute = false;
            uiSource.mute = false;
        }

        #endregion
    }
}
