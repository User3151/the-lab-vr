using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

// שלב 1: להחזיר את החשמל ללוח.
// פותחים את קופסת הפיוזים, מזהים את הפיוז השחור, מחליפים אותו בפיוז מאותו סוג, ומרימים את המפסק הראשי.
// החשמל חוזר רק כשהכול מתקיים יחד: בכל שקע יש פיוז תקין, והמפסק למעלה.
// מה שקורה כשהחשמל חוזר (התאורה) מחובר בסצנה.
//
// זה הצעד היחיד שלא מתבטל, בכוונה: ברגע שהאור חוזר, הפיוזים ננעלים בשקעים והמפסק נשאר למעלה.
// ביטול היה צריך לכבות את החדר ולסגור את כל הרשימה, וזה לא מוסיף כלום למשחק.
public class FusePuzzle : PuzzleStep
{
    [Header("Input")]
    [SerializeField] private ValidatedSocket[] fuseSockets;
    [SerializeField] private ConstrainedDial mainSwitch;
    [SerializeField] private float switchOnAbove = 0.95f;   // כמה גבוה המפסק צריך להיות כדי להיחשב "למעלה"

    // מה קורה בחדר כשהחשמל חוזר (בסצנה: LabLighting.PowerOn)
    [SerializeField] private UnityEvent onPowerRestored;

    // "כל הפיוזים תקינים והמפסק למעלה" הוא מצב ולא רגע, אז בודקים אותו בכל פריים.
    // זה גם מה שמאפשר לשחקן לטעות: פיוז שרוף שהוחזר, או מפסק שהורם לפני ההחלפה, פשוט לא מספיקים.
    private void Update()
    {
        if (!IsActive || IsSolved) return;
        if (mainSwitch.Value < switchOnAbove) return;

        foreach (ValidatedSocket socket in fuseSockets)
            if (!HoldsLiveFuse(socket)) return;

        Solve();
    }

    // השקע מקבל כל פיוז מהסוג הנכון, גם שרוף, כי שקע לא יודע אם פיוז עובד. החידה היא שבודקת.
    // פיוזים בגודל אחר לא מגיעים לכאן בכלל: השקע מסרב להם לפי ה-SocketKey.
    private static bool HoldsLiveFuse(ValidatedSocket socket)
    {
        if (!socket.hasSelection) return false;
        Transform held = socket.firstInteractableSelected.transform;
        return held.TryGetComponent(out Fuse fuse) && !fuse.IsBurnt;
    }

    protected override void OnSolved()
    {
        foreach (ValidatedSocket socket in fuseSockets) LockInSocket(socket);

        // כיבוי של אינטראקטבל משחרר אותו מהיד, והמפסק נשאר בזווית שבה הוא עומד: למעלה
        mainSwitch.enabled = false;

        onPowerRestored.Invoke();
        Debug.Log("Power restored");
    }

    // לא מכבים את ה-XRGrabInteractable: אינטראקטבל כבוי משתחרר מכל מי שמחזיק בו, גם מהשקע,
    // והפיוז היה נופל מהקופסה. במקום זה מוסיפים לו מסנן בחירה שמאשר רק את השקע שלו.
    private static void LockInSocket(ValidatedSocket socket)
    {
        if (!socket.hasSelection) return;
        if (!socket.firstInteractableSelected.transform.TryGetComponent(out XRGrabInteractable grab)) return;

        IXRSelectInteractor owner = socket;
        grab.selectFilters.Add(new XRSelectFilterDelegate((interactor, _) => interactor == owner));
    }
}
