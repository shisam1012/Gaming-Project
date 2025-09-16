using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TimerUI_Slider : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private LevelManager levelManager;
    [SerializeField] private Slider slider;
    [SerializeField] private TextMeshProUGUI timeLabel;

    private float totalTime = 60f;
    private bool ticking;

    private void Awake()
    {
        if (!levelManager) levelManager = FindFirstObjectByType<LevelManager>();
    }

    private void OnEnable()
    {
        if (!levelManager) return;
        levelManager.onTimerStart.AddListener(HandleTimerStart);
        levelManager.onTimerEnd.AddListener(HandleTimerEnd);
        levelManager.onTimeUp.AddListener(HandleTimeUp);
    }
    private void OnDisable()
    {
        if (!levelManager) return;
        levelManager.onTimerStart.RemoveListener(HandleTimerStart);
        levelManager.onTimerEnd.RemoveListener(HandleTimerEnd);
        levelManager.onTimeUp.RemoveListener(HandleTimeUp);
    }

    private void Update()
    {
        if (!ticking || levelManager == null || slider == null) return;

        float left = levelManager.TimeLeft;
        slider.value = Mathf.Clamp(left, 0f, totalTime);

        if (timeLabel)
        {
            int secs = Mathf.CeilToInt(left);
            int mm = secs / 60;
            int ss = secs % 60;
            timeLabel.text = $"{mm:00}:{ss:00}";
        }
    }

    public void HandleTimerStart(float initial)
    {
        totalTime = Mathf.Max(1f, initial);
        if (slider)
        {
            slider.minValue = 0f;
            slider.maxValue = totalTime;
            slider.value = totalTime;
        }
        ticking = true;
        Update();
    }

    public void HandleTimerEnd()
    {
        ticking = false;
        if (slider) slider.value = Mathf.Clamp(levelManager.TimeLeft, 0f, totalTime);
    }

    public void HandleTimeUp()
    {
        ticking = false;
        if (slider) slider.value = 0f;
    }
}
