using TMPro;
using UnityEngine;

// משימת בטיחות: לאזן את הצנטריפוגה.
// צנטריפוגה עם מבחנות לא מאוזנות רועדת ועלולה להישבר. צריך לפתוח את המכסה, לסדר את המבחנות
// כך שמרכז המסה שלהן יהיה בציר, ולסגור. אם הסידור מאוזן, היא מסתובבת חלק ונעצרת, והמשימה נפתרת.
public class CentrifugePuzzle : PuzzleStep
{
    [Header("Parts")]
    [SerializeField] private ConstrainedDial lid;
    [SerializeField] private ValidatedSocket[] slots;     // שקעים במרחקים שווים סביב הציר
    [SerializeField] private Spinner rotor;
    [SerializeField] private Transform shakeBody;         // מה שרועד כשהיא לא מאוזנת
    [SerializeField] private TMP_Text display;

    [Header("Rules")]
    [SerializeField] private int requiredTubes = 4;
    [SerializeField] private float lidClosedBelow = 0.05f;   // ערך המכסה (0 סגור, 1 פתוח) שנחשב סגור
    [SerializeField] private float balanceTolerance = 0.01f; // מטרים: כמה רחוק מהציר מרכז המבחנות יכול להיות

    [Header("Feel")]
    [SerializeField] private float shakeAmplitude = 0.006f;
    [SerializeField] private float spinSeconds = 3f;

    private Vector3 shakeRestPosition;
    private float spinEndTime = -1f;

    private void Awake()
    {
        shakeRestPosition = shakeBody.localPosition;
        display.text = "";   // בלי חשמל אין תצוגה
    }

    private void Update()
    {
        if (!IsActive) return;

        // הריצה המאוזנת כבר התחילה: מחכים שתסתיים ואז נפתרים
        if (spinEndTime >= 0f)
        {
            if (Time.time >= spinEndTime) Solve();
            return;
        }

        bool lidClosed = lid.Value <= lidClosedBelow;

        if (!lidClosed)
        {
            StopShaking();
            display.text = $"<color=#FFC400>LID OPEN</color>\nTUBES {LoadedTubes()}/{requiredTubes}";
            return;
        }

        if (IsBalanced())
        {
            StartBalancedRun();
            return;
        }

        // מכסה סגור ולא מאוזן: המכונה מנסה להסתובב ורועדת. זה הרמז הראשון שמשהו לא בסדר.
        shakeBody.localPosition = shakeRestPosition + Random.insideUnitSphere * shakeAmplitude;
        display.text = "<color=#FF3B30>IMBALANCE</color>\nOPEN LID";
    }

    private void StartBalancedRun()
    {
        StopShaking();
        lid.enabled = false;   // נועלים את המכסה בזמן סיבוב, כמו במכשיר אמיתי
        rotor.StartSpinning();
        spinEndTime = Time.time + spinSeconds;
        display.text = "<color=#3DDC84>BALANCED</color>\nTEST RUN";
    }

    // מאוזן = כל המבחנות בפנים, והממוצע של המיקומים שלהן יושב על ציר הסיבוב
    private bool IsBalanced()
    {
        if (LoadedTubes() != requiredTubes) return false;

        Vector3 sum = Vector3.zero;
        foreach (ValidatedSocket slot in slots)
            if (slot.hasSelection) sum += slot.transform.position - rotor.transform.position;

        Vector3 centerOfMass = Vector3.ProjectOnPlane(sum / requiredTubes, rotor.transform.up);
        return centerOfMass.magnitude <= balanceTolerance;
    }

    private int LoadedTubes()
    {
        int count = 0;
        foreach (ValidatedSocket slot in slots)
            if (slot.hasSelection) count++;
        return count;
    }

    private void StopShaking() => shakeBody.localPosition = shakeRestPosition;

    protected override void OnSolved()
    {
        rotor.StopSpinning();
        lid.enabled = true;
        display.text = "<color=#3DDC84>BALANCED</color>\nSAFE";
        Debug.Log("Safety task solved: centrifuge balanced");
    }
}
