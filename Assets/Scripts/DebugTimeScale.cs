// FILE: DebugTimeScale_NewInput.cs
using UnityEngine;
using UnityEngine.InputSystem;

public class DebugTimeScale_NewInput : MonoBehaviour
{
    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            Debug.Log("Time.timeScale = " + Time.timeScale);
        }
    }
}
