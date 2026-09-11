using UnityEngine;
using UnityEngine.InputSystem;

// מחליף בין שליטה בשלט הסימולטור לשליטה ביד מהמצלמה.
// שתי הידיים קיימות בסצנה - רק אחת פעילה בכל רגע.
public class HandModeSwitcher : MonoBehaviour
{
    [SerializeField] private GameObject simulatorHand;
    [SerializeField] private GameObject webcamHand;
    [SerializeField] private Key toggleKey = Key.M;
    [SerializeField] private bool startWithWebcam = false;

    private bool webcamActive;

    private void Start()
    {
        webcamActive = startWithWebcam;
        Apply();
    }

    private void Update()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current[toggleKey].wasPressedThisFrame)
        {
            webcamActive = !webcamActive;
            Apply();
        }
    }

    private void Apply()
    {
        if (simulatorHand != null) simulatorHand.SetActive(!webcamActive);
        if (webcamHand != null) webcamHand.SetActive(webcamActive);

        Debug.Log(webcamActive ? "Input source: WEBCAM HAND" : "Input source: SIMULATOR CONTROLLER");
    }
}