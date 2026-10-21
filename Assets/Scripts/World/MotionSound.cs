using UnityEngine;

// משמיע צלילים לפי התנועה של חפץ: דלת, מכסה, מגירה, גלגל, ידית.
// לא תלוי בסקריפט שמזיז אותו. הוא מודד כמה ה-Transform זז מנקודת המנוחה שלו.
[RequireComponent(typeof(AudioSource))]
public class MotionSound : MonoBehaviour
{
    private enum Measure { Angle, Distance }

    [SerializeField] private Transform target;                 // מה שזז. ריק = האובייקט הזה
    [SerializeField] private Measure measure = Measure.Angle;  // מעלות לדלתות וגלגלים, מטרים למגירות

    [Header("Open / close")]
    [SerializeField] private AudioClip openClip;
    [SerializeField] private AudioClip closeClip;
    [SerializeField] private float openAt = 15f;     // מכאן החפץ נחשב פתוח
    [SerializeField] private float closedAt = 3f;    // מתחת לזה הוא שוב סגור. הפער מונע צליל כפול כשהיד רועדת

    [Header("Loop while moving")]
    [SerializeField] private AudioClip moveLoop;              // למשל חריקה של גלגל
    [SerializeField] private float fullVolumeSpeed = 90f;     // מהירות (ליחידה לשנייה) שבה הלופ בעוצמה מלאה
    [SerializeField] private float volumeFade = 6f;           // כמה מהר העוצמה רודפת אחרי המהירות

    private AudioSource source;
    private Quaternion restRotation;
    private Vector3 restPosition;
    private bool isOpen;
    private float lastAmount;

    private void Awake()
    {
        if (target == null) target = transform;
        source = GetComponent<AudioSource>();
        restRotation = target.localRotation;
        restPosition = target.localPosition;

        // הלופ מתנגן כל הזמן בשקט, והתנועה מגבירה אותו
        if (moveLoop != null)
        {
            source.clip = moveLoop;
            source.loop = true;
            source.volume = 0f;
            source.Play();
        }
    }

    private void Update()
    {
        float amount = Amount();

        if (!isOpen && amount >= openAt)
        {
            isOpen = true;
            Play(openClip);
        }
        else if (isOpen && amount <= closedAt)
        {
            isOpen = false;
            Play(closeClip);
        }

        if (moveLoop != null && Time.deltaTime > 0f)
        {
            float speed = Mathf.Abs(amount - lastAmount) / Time.deltaTime;
            float wanted = Mathf.Clamp01(speed / fullVolumeSpeed);
            source.volume = Mathf.MoveTowards(source.volume, wanted, volumeFade * Time.deltaTime);
        }

        lastAmount = amount;
    }

    // כמה החפץ רחוק מנקודת המנוחה: במעלות או במטרים
    private float Amount()
    {
        if (measure == Measure.Angle) return Quaternion.Angle(restRotation, target.localRotation);
        return Vector3.Distance(restPosition, target.localPosition);
    }

    private void Play(AudioClip clip)
    {
        if (clip == null) return;
        source.pitch = Random.Range(0.95f, 1.05f);   // שינוי קטן כדי שאותו צליל לא יישמע כמו הקלטה חוזרת
        source.PlayOneShot(clip);
    }
}
