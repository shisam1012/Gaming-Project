using UnityEngine;



public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource effectsSource;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip matchSound1;
    public AudioClip MatchSound1 => matchSound1;
    [SerializeField] private AudioClip matchSound2;
    public AudioClip MatchSound2 => matchSound2;
    [SerializeField] private AudioClip timeoutSound;
    public AudioClip TimeoutSound => timeoutSound;
    [SerializeField] private AudioClip levelUpSound;
  //  public AudioClip LevelUpSound => levelUpSound;
   // [SerializeField] private AudioClip[] matchSounds; 
    void Awake()
    {

        if (Instance == null)
        {
            Instance = this;
        }

        else
        {
            Destroy(gameObject);
        }

    }


    /* public void PlayRandomMatchSound()
     {
         if (matchSounds.Length > 0)
         {
             int randomIndex = Random.Range(0, matchSounds.Length);
             effectsSource.PlayOneShot(matchSounds[randomIndex]);
         }
     }*/

    public void PlayMatchSound(int num)
    {
        if (num <= 3)
        {
            effectsSource.PlayOneShot(matchSound1);
        }
        else
        {
            effectsSource.PlayOneShot(matchSound2);
        }
    }
    public void PlaySound(AudioClip clip)
    {
        if (clip != null)
        {
            effectsSource.PlayOneShot(clip);
        }
    }       

}