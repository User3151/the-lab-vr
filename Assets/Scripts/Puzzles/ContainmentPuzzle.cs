using UnityEngine;

// משימת בטיחות: להחזיר את הדגימות המסוכנות לאחסון הקירור ולסגור את הדלת.
// כל שקע מקבל רק את הדגימה בצבע שלו (דרך SocketKey), כך שהמיון הוא חלק מהמשימה.
// ההסגר הוא מצב: פתיחת הדלת או הוצאת דגימה מחזירות את המשימה לאדום.
public class ContainmentPuzzle : PuzzleStep
{
    [Header("Parts")]
    [SerializeField] private ValidatedSocket[] slots;
    [SerializeField] private ConstrainedDial fridgeDoor;
    [SerializeField] private float doorClosedBelow = 0.05f;

    [Header("Feedback")]
    [SerializeField] private Light statusLight;
    [SerializeField] private Color unsafeColor = Color.red;
    [SerializeField] private Color safeColor = Color.green;

    // בלי חשמל נורת הסטטוס כבויה
    private void Awake() => statusLight.enabled = false;

    protected override void OnActivated()
    {
        statusLight.color = unsafeColor;
        statusLight.enabled = true;
    }

    // "הכול בפנים והדלת סגורה" הוא מצב ולא רגע, אז דוגמים אותו בכל פריים,
    // גם אחרי הפתרון, כדי לתפוס את הרגע שבו הוא מפסיק להתקיים
    private void Update()
    {
        if (!IsActive) return;

        bool contained = IsContained();
        if (contained && !IsSolved) Solve();
        else if (!contained && IsSolved) Unsolve();
    }

    private bool IsContained()
    {
        if (fridgeDoor.Value > doorClosedBelow) return false;

        foreach (ValidatedSocket slot in slots)
            if (!slot.hasSelection) return false;

        return true;
    }

    protected override void OnSolved()
    {
        statusLight.color = safeColor;
        Debug.Log("Safety task solved: samples contained");
    }

    protected override void OnUnsolved()
    {
        statusLight.color = unsafeColor;
        Debug.Log("Safety task broken: containment open");
    }
}
