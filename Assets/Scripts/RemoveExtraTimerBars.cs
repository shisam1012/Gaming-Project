using UnityEngine;

namespace GamingProject
{
    /// <summary>
    /// Simple script to remove unwanted timer bars.
    /// Just add this to any object and it will clean up extra timer bars.
    /// </summary>
    public class RemoveExtraTimerBars : MonoBehaviour
    {
        [ContextMenu("Remove Extra Timer Bars")]
        public void CleanupTimerBars()
        {
            Debug.Log("[RemoveExtraTimerBars] Removing unwanted timer bars...");
            
            // Find and remove SimpleTimerBar objects
            GameObject[] simpleBars = GameObject.FindGameObjectsWithTag("Untagged");
            foreach (GameObject obj in simpleBars)
            {
                if (obj.name.Contains("SimpleTimerBar") || obj.name.Contains("EMERGENCY_TIMER_BAR") || 
                    obj.name.Contains("DirectTimerBar") || obj.name.Contains("ForceTimerBar"))
                {
                    Debug.Log($"[RemoveExtraTimerBars] Removing: {obj.name}");
                    DestroyImmediate(obj);
                }
            }
            
            // Alternative: Find all objects in scene
            var allObjects = FindObjectsByType<Transform>(FindObjectsSortMode.None);
            foreach (Transform t in allObjects)
            {
                if (t.name.Contains("TimerBar") && !t.name.Contains("UI") && t.gameObject != this.gameObject)
                {
                    Debug.Log($"[RemoveExtraTimerBars] Found and removing timer bar: {t.name}");
                    DestroyImmediate(t.gameObject);
                }
            }
            
            Debug.Log("[RemoveExtraTimerBars] Cleanup complete!");
        }
        
        void Start()
        {
            // Auto-remove on start
            Invoke(nameof(CleanupTimerBars), 0.5f);
        }
    }
}