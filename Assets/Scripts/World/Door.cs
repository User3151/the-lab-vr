using System.Collections;
using UnityEngine;

// דלת על ציר. מסתובבת פעם אחת כשקוראים ל-Open.
// האובייקט הזה הוא הציר עצמו. הלוח הוא אובייקט בן, מוזז הצידה כך שהקצה שלו יושב על הציר.
public class Door : MonoBehaviour
{
    [SerializeField] private float openAngle = 100f;   // מעלות סביב ציר Y
    [SerializeField] private float openSeconds = 2f;

    private bool opened;

    public void Open()
    {
        if (opened) return;
        opened = true;
        StartCoroutine(Swing());
    }

    private IEnumerator Swing()
    {
        Quaternion closed = transform.localRotation;
        Quaternion open = closed * Quaternion.Euler(0f, openAngle, 0f);

        for (float t = 0f; t < openSeconds; t += Time.deltaTime)
        {
            // SmoothStep: מאיץ בהתחלה ומאט בסוף, כמו דלת כבדה
            float k = Mathf.SmoothStep(0f, 1f, t / openSeconds);
            transform.localRotation = Quaternion.Slerp(closed, open, k);
            yield return null;
        }

        transform.localRotation = open;
        Debug.Log($"{name} opened");
    }
}
