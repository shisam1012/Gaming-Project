using UnityEngine;
using UnityEngine.Audio;

public class AudioBoot : MonoBehaviour
{
    [Header("Optional Mixer")]
    public AudioMixer masterMixer; // שימי את ה-Master Mixer אם יש לך, אחרת השאירי ריק
    [Tooltip("שם הפרמטר החשוף בווליום, למשל MasterVolume")]
    public string masterVolumeParam = "MasterVolume";

    void Awake()
    {
        // ודאי שאין Pause גלובלי ולא ווליום אפס
        AudioListener.pause = false;
        AudioListener.volume = 1f;

#if UNITY_ANDROID
        // באנדרואיד, לפעמים Unity עוצר אודיו כאשר מוגדר Mute. שחרור יזום:
        if (AudioSettings.Mobile.stopAudioOutputOnMute)
        {
            Debug.Log("[AudioBoot] stopAudioOutputOnMute היה פעיל, מנסה לוודא יציאה מא-Stop");
            AudioSettings.Mobile.stopAudioOutputOnMute = false;
            AudioSettings.Mobile.StartAudioOutput();
        }
#endif

        // אם יש Mixer ופרמטר חשוף, אפס ל-0dB
        if (masterMixer && !string.IsNullOrEmpty(masterVolumeParam))
        {
            bool ok = masterMixer.SetFloat(masterVolumeParam, 0f); // 0dB
            Debug.Log("[AudioBoot] Set master volume to 0 dB: " + ok);
        }

        Debug.Log($"[AudioBoot] AudioListener.pause={AudioListener.pause}, AudioListener.volume={AudioListener.volume}");
    }
}
