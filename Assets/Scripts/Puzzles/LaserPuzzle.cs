using UnityEngine;

// חידה 3: לכייל את הממסר האופטי של הדלת.
// הלייזר נדלק רק אחרי שלחץ הקירור יציב (החידה הקודמת). צריך לסובב את המראות
// כך שהקרן תגיע ישר לחיישן ליד הדלת ותישאר עליו, ואז למנעול הדלת יש חשמל.
public class LaserPuzzle : PuzzleStep
{
    [Header("Parts")]
    [SerializeField] private LaserEmitter emitter;
    [SerializeField] private LaserReceiver receiver;
    [SerializeField] private float holdSeconds = 1f;

    [Header("Feedback")]
    [SerializeField] private Light relayLight;
    [SerializeField] private Color waitingColor = Color.red;
    [SerializeField] private Color poweredColor = Color.green;

    private float litTimer;

    private void Awake()
    {
        // בלי קירור יציב הלייזר לא יורה, והממסר כבוי
        emitter.enabled = false;
        relayLight.enabled = false;
    }

    // תור החידה הגיע: הלייזר נדלק, והנורה האדומה מסמנת איפה החיישן
    protected override void OnActivated()
    {
        emitter.enabled = true;
        relayLight.color = waitingColor;
        relayLight.enabled = true;
    }

    // "מואר" הוא מצב מתמשך ולא רגע, אז דוגמים אותו בכל פריים
    private void Update()
    {
        if (!IsActive) return;

        litTimer = receiver.IsLit ? litTimer + Time.deltaTime : 0f;
        if (litTimer >= holdSeconds) Solve();
    }

    protected override void OnSolved()
    {
        relayLight.color = poweredColor;
        Debug.Log("Puzzle 3 solved: optical relay aligned, door lock has power");
    }
}
