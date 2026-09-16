using UnityEngine;

// מד לחץ: רק מציג. הוא לא יודע מה נחשב "טוב", רק מה ביקשו ממנו להראות.
public class PressureGauge : MonoBehaviour
{
    [Header("Needle")]
    [SerializeField] private Transform needle;
    [SerializeField] private float emptyAngle = 120f;    // זווית המחוג בלחץ 0, סביב Z המקומי
    [SerializeField] private float fullAngle = -120f;    // זווית המחוג בלחץ מלא

    [Header("Status light")]
    [SerializeField] private Light statusLight;
    [SerializeField] private Color unstableColor = Color.red;
    [SerializeField] private Color stableColor = Color.green;
    [SerializeField] private float alarmBlinkHz = 4f;

    // בלי חשמל הנורה כבויה
    private void Awake() => statusLight.enabled = false;

    public void Show(float pressure, bool stable, bool alarm)
    {
        float angle = Mathf.Lerp(emptyAngle, fullAngle, Mathf.Clamp01(pressure));
        needle.localRotation = Quaternion.Euler(0f, 0f, angle);

        statusLight.color = stable ? stableColor : unstableColor;

        // באזעקה הנורה מהבהבת: דולקת בחצי הראשון של כל מחזור
        statusLight.enabled = !alarm || Mathf.Repeat(Time.time * alarmBlinkHz, 1f) < 0.5f;
    }
}
