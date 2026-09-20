using UnityEngine;
using UnityEngine.Events;

// שלב 1: מוציאים את הפיוז השרוף, מכניסים פיוז תקין, והחשמל חוזר.
// החידה מכירה רק את השקע. מה שקורה כשהחשמל חוזר (התאורה) מחובר בסצנה.
public class FusePuzzle : PuzzleStep
{
    [Header("Input")]
    [SerializeField] private ValidatedSocket fuseSocket;

    // מה קורה בחדר כשהחשמל חוזר (בסצנה: LabLighting.PowerOn)
    [SerializeField] private UnityEvent onPowerRestored;

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

        Solve();
    }

    protected override void OnSolved()
    {
        onPowerRestored.Invoke();
        Debug.Log("Power restored");
    }
}
