using UnityEngine;
using UnityEngine.Events;

// משימת בטיחות: שטיפת האוויר בחנקן.
// השסתום קובע לאן הלחץ שואף, אבל הלחץ מגיע לשם כמו קפיץ: סיבוב מהיר עובר את המטרה.
// מעבר לקו האדום מפעיל אזעקה. צריך לסגור את השסתום עד הסוף ולנסות שוב, לאט יותר.
// גם אחרי הפתרון הלחץ ממשיך להיבדק: הזזת השסתום מחוץ לתחום מחזירה את המשימה לאדום.
public class ValvePuzzle : PuzzleStep
{
    [Header("Parts")]
    [SerializeField] private ConstrainedDial valve;
    [SerializeField] private PressureGauge gauge;

    // מה קורה בעולם כשהלחץ יציב (בסצנה: מאוורר האוורור מתחיל להסתובב)
    [SerializeField] private UnityEvent onStabilized;

    // מה קורה כשהלחץ יוצא שוב מהתחום אחרי שכבר התייצב (בסצנה: המאוורר נעצר)
    [SerializeField] private UnityEvent onDestabilized;

    [Header("Target")]
    [SerializeField] private float greenMin = 0.6f;
    [SerializeField] private float greenMax = 0.7f;
    [SerializeField] private float redLine = 0.9f;
    [SerializeField] private float holdSeconds = 2f;

    // כמה זמן הלחץ צריך להיות מחוץ לתחום עד שהמשימה מתבטלת.
    // הקפיץ רועד לרגע סביב הקצה, ובלי זה המאוורר היה נעצר ומתחיל מכל רעידה.
    [SerializeField] private float leaveGraceSeconds = 0.5f;

    [Header("Pressure spring")]
    [SerializeField] private float stiffness = 20f;   // כמה חזק הלחץ נמשך אל השסתום
    [SerializeField] private float damping = 3f;      // נמוך יותר = חריגה גדולה יותר מעבר למטרה

    // מכריז כשהלחץ חצה את הקו האדום. המנחה מגיב לזה, והחידה לא צריכה להכיר אותו.
    public static event System.Action OnAlarm;

    private float pressure;
    private float pressureVelocity;
    private float holdTimer;
    private float outTimer;
    private bool venting;

    // אין כאן אירועים: הלחץ משתנה כל הזמן, אז דוגמים אותו בכל פריים.
    // אירוע מתאים לרגע ("נמשך", "נכנס"). ערך רציף מתאים לדגימה.
    private void Update()
    {
        // עד שמגיע התור של החידה, המד כבוי והטיימרים לא סופרים
        if (!IsActive) return;

        UpdatePressure();

        bool inBand = !venting && pressure >= greenMin && pressure <= greenMax;
        gauge.Show(pressure, inBand, venting);

        if (!IsSolved)
        {
            // כדי להיפתר צריך להחזיק בתחום ברציפות. יציאה ממנו מאפסת את הספירה.
            holdTimer = inBand ? holdTimer + Time.deltaTime : 0f;
            if (holdTimer >= holdSeconds) Solve();
        }
        else
        {
            // כדי להתבטל צריך להישאר מחוץ לתחום. חזרה אליו מאפסת את הספירה.
            outTimer = inBand ? 0f : outTimer + Time.deltaTime;
            if (outTimer >= leaveGraceSeconds) Unsolve();
        }
    }

    private void UpdatePressure()
    {
        // בזמן אזעקה, הלחץ משתחרר לאפס, לא משנה מה השסתום אומר
        float target = venting ? 0f : valve.Value;

        // קפיץ עם ריסון: תאוצה לכיוון היעד, וחיכוך שמאט את המהירות
        pressureVelocity += (target - pressure) * stiffness * Time.deltaTime;
        pressureVelocity -= pressureVelocity * damping * Time.deltaTime;
        pressure += pressureVelocity * Time.deltaTime;

        if (!venting && pressure > redLine)
        {
            venting = true;
            OnAlarm?.Invoke();
            Debug.Log("Pressure alarm: venting");
        }

        // האזעקה נפסקת רק אחרי שהשחקן סגר את השסתום כמעט עד הסוף
        if (venting && valve.Value < 0.05f && pressure < 0.05f)
        {
            venting = false;
            Debug.Log("Pressure reset");
        }
    }

    protected override void OnSolved()
    {
        outTimer = 0f;
        onStabilized.Invoke();
        Debug.Log("Safety task solved: air purge stable");
    }

    protected override void OnUnsolved()
    {
        holdTimer = 0f;   // כדי להיפתר שוב צריך להחזיק שתי שניות מההתחלה
        onDestabilized.Invoke();
        Debug.Log("Safety task broken: air purge pressure lost");
    }
}
