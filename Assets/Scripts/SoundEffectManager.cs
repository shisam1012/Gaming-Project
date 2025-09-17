using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GamingProject
{
    public class SoundEffectManager : MonoBehaviour
    {
        private static SoundEffectManager _instance;
        public static SoundEffectManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<SoundEffectManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("SoundEffectManager");
                        _instance = go.AddComponent<SoundEffectManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        [System.Serializable]
        public class SoundEffect
        {
            public string name;
            public AudioClip clip;
            [Range(0f, 1f)]
            public float volume = 1f;
            [Range(0.1f, 3f)]
            public float pitch = 1f;
            public bool loop = false;
        }

        [Header("Sound Effects")]
        [SerializeField] private SoundEffect[] soundEffects;
        
        [Header("Audio Sources")]
        [SerializeField] private int audioSourcePoolSize = 5;
        [SerializeField] private AudioSource musicAudioSource;
        
        [Header("Master Volume")]
        [Range(0f, 1f)]
        [SerializeField] private float masterVolume = 1f;
        [Range(0f, 1f)]
        [SerializeField] private float sfxVolume = 1f;
        [Range(0f, 1f)]
        [SerializeField] private float musicVolume = 1f;
        
        private List<AudioSource> audioSourcePool;
        private Dictionary<string, SoundEffect> soundEffectDict;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            InitializeAudioSources();
            InitializeSoundEffectDictionary();
        }

        private void InitializeAudioSources()
        {
            audioSourcePool = new List<AudioSource>();
            
            // Create pool of audio sources for sound effects
            for (int i = 0; i < audioSourcePoolSize; i++)
            {
                GameObject audioSourceObj = new GameObject($"SFX_AudioSource_{i}");
                audioSourceObj.transform.SetParent(transform);
                
                AudioSource audioSource = audioSourceObj.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.loop = false;
                audioSource.ignoreListenerPause = true;
                
                audioSourcePool.Add(audioSource);
            }
            
            // Setup music audio source if not assigned
            if (musicAudioSource == null)
            {
                GameObject musicObj = new GameObject("Music_AudioSource");
                musicObj.transform.SetParent(transform);
                musicAudioSource = musicObj.AddComponent<AudioSource>();
                musicAudioSource.playOnAwake = false;
                musicAudioSource.loop = true;
            }
            
            Debug.Log($"[SoundEffectManager] Initialized with {audioSourcePoolSize} audio sources");
        }

        private void InitializeSoundEffectDictionary()
        {
            soundEffectDict = new Dictionary<string, SoundEffect>();
            
            if (soundEffects != null)
            {
                foreach (var soundEffect in soundEffects)
                {
                    if (!string.IsNullOrEmpty(soundEffect.name) && soundEffect.clip != null)
                    {
                        soundEffectDict[soundEffect.name] = soundEffect;
                    }
                }
            }
            
            Debug.Log($"[SoundEffectManager] Loaded {soundEffectDict.Count} sound effects");
        }

        public void PlaySoundEffect(string soundName)
        {
            if (soundEffectDict.TryGetValue(soundName, out SoundEffect soundEffect))
            {
                PlaySoundEffect(soundEffect);
            }
            else
            {
                Debug.LogWarning($"[SoundEffectManager] Sound effect '{soundName}' not found!");
            }
        }

        public void PlaySoundEffect(SoundEffect soundEffect)
        {
            if (soundEffect?.clip == null)
            {
                Debug.LogWarning("[SoundEffectManager] Cannot play null sound effect!");
                return;
            }

            AudioSource audioSource = GetAvailableAudioSource();
            if (audioSource != null)
            {
                audioSource.clip = soundEffect.clip;
                audioSource.volume = soundEffect.volume * sfxVolume * masterVolume;
                audioSource.pitch = soundEffect.pitch;
                audioSource.loop = soundEffect.loop;
                audioSource.Play();

                Debug.Log($"[SoundEffectManager] Playing sound: {soundEffect.name}");

                if (!soundEffect.loop)
                {
                    StartCoroutine(ReturnAudioSourceToPool(audioSource, soundEffect.clip.length));
                }
            }
            else
            {
                Debug.LogWarning("[SoundEffectManager] No available audio sources in pool!");
            }
        }

        public void PlaySoundEffectOneShot(string soundName)
        {
            if (soundEffectDict.TryGetValue(soundName, out SoundEffect soundEffect))
            {
                AudioSource audioSource = GetAvailableAudioSource();
                if (audioSource != null)
                {
                    float volume = soundEffect.volume * sfxVolume * masterVolume;
                    audioSource.PlayOneShot(soundEffect.clip, volume);
                    
                    Debug.Log($"[SoundEffectManager] Playing one-shot sound: {soundEffect.name}");
                }
            }
        }

        public void StopSoundEffect(string soundName)
        {
            foreach (var audioSource in audioSourcePool)
            {
                if (audioSource.isPlaying && audioSource.clip != null)
                {
                    if (soundEffectDict.TryGetValue(soundName, out SoundEffect soundEffect))
                    {
                        if (audioSource.clip == soundEffect.clip)
                        {
                            audioSource.Stop();
                            Debug.Log($"[SoundEffectManager] Stopped sound: {soundName}");
                        }
                    }
                }
            }
        }

        public void StopAllSoundEffects()
        {
            foreach (var audioSource in audioSourcePool)
            {
                if (audioSource.isPlaying)
                {
                    audioSource.Stop();
                }
            }
            Debug.Log("[SoundEffectManager] Stopped all sound effects");
        }

        public void PlayMusic(AudioClip musicClip, bool loop = true)
        {
            if (musicAudioSource != null && musicClip != null)
            {
                musicAudioSource.clip = musicClip;
                musicAudioSource.volume = musicVolume * masterVolume;
                musicAudioSource.loop = loop;
                musicAudioSource.Play();
                
                Debug.Log($"[SoundEffectManager] Playing music: {musicClip.name}");
            }
        }

        public void StopMusic()
        {
            if (musicAudioSource != null)
            {
                musicAudioSource.Stop();
                Debug.Log("[SoundEffectManager] Stopped music");
            }
        }

        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
            UpdateAllVolumes();
        }

        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
            UpdateAllVolumes();
        }

        public void SetMusicVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);
            if (musicAudioSource != null)
            {
                musicAudioSource.volume = musicVolume * masterVolume;
            }
        }

        private void UpdateAllVolumes()
        {
            // Update music volume
            if (musicAudioSource != null)
            {
                musicAudioSource.volume = musicVolume * masterVolume;
            }
            
            // Update playing sound effects volume
            foreach (var audioSource in audioSourcePool)
            {
                if (audioSource.isPlaying)
                {
                    // Find the original sound effect volume
                    foreach (var kvp in soundEffectDict)
                    {
                        if (audioSource.clip == kvp.Value.clip)
                        {
                            audioSource.volume = kvp.Value.volume * sfxVolume * masterVolume;
                            break;
                        }
                    }
                }
            }
        }

        private AudioSource GetAvailableAudioSource()
        {
            // Find an audio source that's not playing
            foreach (var audioSource in audioSourcePool)
            {
                if (!audioSource.isPlaying)
                {
                    return audioSource;
                }
            }
            
            // If all are busy, use the first one (will interrupt)
            return audioSourcePool.Count > 0 ? audioSourcePool[0] : null;
        }

        private IEnumerator ReturnAudioSourceToPool(AudioSource audioSource, float delay)
        {
            yield return new WaitForSeconds(delay);
            
            if (audioSource != null && !audioSource.loop)
            {
                audioSource.Stop();
                audioSource.clip = null;
            }
        }

        public bool IsSoundEffectPlaying(string soundName)
        {
            if (soundEffectDict.TryGetValue(soundName, out SoundEffect soundEffect))
            {
                foreach (var audioSource in audioSourcePool)
                {
                    if (audioSource.isPlaying && audioSource.clip == soundEffect.clip)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public float GetMasterVolume() => masterVolume;
        public float GetSFXVolume() => sfxVolume;
        public float GetMusicVolume() => musicVolume;

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        // Helper method to add sound effects at runtime
        public void AddSoundEffect(string name, AudioClip clip, float volume = 1f, float pitch = 1f, bool loop = false)
        {
            var newSoundEffect = new SoundEffect
            {
                name = name,
                clip = clip,
                volume = volume,
                pitch = pitch,
                loop = loop
            };
            
            soundEffectDict[name] = newSoundEffect;
            Debug.Log($"[SoundEffectManager] Added sound effect: {name}");
        }
    }
}