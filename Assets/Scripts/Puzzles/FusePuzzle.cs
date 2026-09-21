using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

// שלב 1: מוציאים את הפיוז השרוף, מכניסים פיוז תקין, והחשמל חוזר.
// החידה מכירה רק את השקע. מה שקורה כשהחשמל חוזר (התאורה) מחובר בסצנה.
//
// זה הצעד היחיד שלא מתבטל, בכוונה: הפיוז התקין ננעל בשקע ברגע שהחשמל חוזר.
// הוצאה שלו הייתה צריכה לכבות את החדר ולסגור את כל הרשימה, וזה לא מוסיף כלום למשחק.
public class FusePuzzle : PuzzleStep
{
    [Header("Input")]
    [SerializeField] private ValidatedSocket fuseSocket;

    // מה קורה בחדר כשהחשמל חוזר (בסצנה: LabLighting.PowerOn)
    [SerializeField] private UnityEvent onPowerRestored;

    private Fuse installed;

    // אירוע static חי יותר זמן מהאובייקט, ובלי ניתוק הוא ימשיך לקרוא לאובייקט שנמחק.
    private void OnEnable()  => ValidatedSocket.OnFilled += HandleFilled;
    private void OnDisable() => ValidatedSocket.OnFilled -= HandleFilled;

    private void HandleFilled(ValidatedSocket socket, Transform inserted)
    {
        // האירוע משותף לכל השקעים בחדר, אז מגיבים רק לשקע שלנו
        if (socket != fuseSocket) return;

        // פיוז שרוף נכנס פיזית לשקע בלי להעביר חשמל. אין אור, ואין פתרון.
        if (!inserted.TryGetComponent(out Fuse fuse) || fuse.IsBurnt)
        {
            Debug.Log("Dead fuse in the socket: no power");
            return;
        }

        // זוכרים איזה פיוז נכנס לפני Solve, כי OnSolved צריך לנעול אותו
        installed = fuse;
        Solve();
    }

    protected override void OnSolved()
    {
        LockInSocket(installed);
        onPowerRestored.Invoke();
        Debug.Log("Power restored");
    }

    // לא מכבים את ה-XRGrabInteractable: אינטראקטבל כבוי משתחרר מכל מי שמחזיק בו, גם מהשקע,
    // והפיוז היה נופל מהקופסה. במקום זה מוסיפים לו מסנן בחירה שמאשר רק את השקע,
    // כך שהשקע ממשיך להחזיק אותו ויד כבר לא יכולה לקחת אותו.
    private void LockInSocket(Fuse fuse)
    {
        if (fuse == null || !fuse.TryGetComponent(out XRGrabInteractable grab)) return;

        IXRSelectInteractor socket = fuseSocket;
        grab.selectFilters.Add(new XRSelectFilterDelegate((interactor, _) => interactor == socket));
    }
}
