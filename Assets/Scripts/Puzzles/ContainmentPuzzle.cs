using UnityEngine;

// משימת בטיחות: להחזיר את הדגימות המסוכנות לאחסון הקירור ולסגור את הדלת.
// כל שקע מקבל רק את הדגימה בצבע שלו (דרך SocketKey), כך שהמיון הוא חלק מהמשימה.
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

    // "הכול בפנים והדלת סגורה" הוא מצב ולא רגע, אז דוגמים אותו בכל פריים
    private void Update()
    {
        if (!IsActive) return;
        if (fridgeDoor.Value > doorClosedBelow) return;

        foreach (ValidatedSocket slot in slots)
            if (!slot.hasSelection) return;

        Solve();
    }

    protected override void OnSolved()
    {
        statusLight.color = safeColor;
        fridgeDoor.enabled = false;   // הדלת ננעלת: הדגימות בהסגר
        Debug.Log("Safety task solved: samples contained");
    }
}
