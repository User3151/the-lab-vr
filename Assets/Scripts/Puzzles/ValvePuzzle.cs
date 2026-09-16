using UnityEngine;

// חידה 2: לייצב את הלחץ.
// השסתום קובע לאן הלחץ שואף, אבל הלחץ מגיע לשם כמו קפיץ: סיבוב מהיר עובר את המטרה.
// מעבר לקו האדום מפעיל אזעקה. צריך לסגור את השסתום עד הסוף ולנסות שוב, לאט יותר.
public class ValvePuzzle : PuzzleStep
{
    [Header("Parts")]
    [SerializeField] private ConstrainedDial valve;
    [SerializeField] private PressureGauge gauge;
    [SerializeField] private Door hatch;

    [Header("Target")]
    [SerializeField] private float greenMin = 0.6f;
    [SerializeField] private float greenMax = 0.7f;
    [SerializeField] private float redLine = 0.9f;
    [SerializeField] private float holdSeconds = 2f;

    [Header("Pressure spring")]
    [SerializeField] private float stiffness = 20f;   // כמה חזק הלחץ נמשך אל השסתום
    [SerializeField] private float damping = 3f;      // נמוך יותר = חריגה גדולה יותר מעבר למטרה

    // מכריז כשהלחץ חצה את הקו האדום. המנחה מגיב לזה, והחידה לא צריכה להכיר אותו.
    public static event System.Action OnAlarm;

    private float pressure;
    private float pressureVelocity;
    private float holdTimer;
    private bool venting;

    // אין כאן אירועים: הלחץ משתנה כל הזמן, אז דוגמים אותו בכל פריים.
    // אירוע מתאים לרגע ("נמשך", "נכנס"). ערך רציף מתאים לדגימה.
    private void Update()
    {
        // עד שמגיע התור של החידה, המד כבוי והטיימר לא סופר
        if (!IsActive) return;

        UpdatePressure();

        bool inBand = !venting && pressure >= greenMin && pressure <= greenMax;
        gauge.Show(pressure, inBand, venting);

        // צריך להחזיק בתחום ברציפות. יציאה ממנו מאפסת את הספירה.
        holdTimer = inBand ? holdTimer + Time.deltaTime : 0f;
        if (holdTimer >= holdSeconds) Solve();
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
        gauge.Show(pressure, true, false);
        hatch.Open();
        Debug.Log("Puzzle 2 solved: hatch open");
    }
}
