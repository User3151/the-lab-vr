using System.Text;
using TMPro;
using UnityEngine;

// מסוף הסגר ליד הדלת: מציג את רשימת בדיקות הבטיחות, אדום מה שחסר וירוק מה שהושלם.
// הוא רק מציג. הוא לא מחליט כלום, והוא מתעדכן מההכרזות של הצעדים.
// הוא גם המקור היחיד לטקסט הרשימה: הלוח הנייד מציג בדיוק את מה שהוא מפרסם כאן.
public class LockdownTerminal : MonoBehaviour
{
    [SerializeField] private TMP_Text screen;
    [SerializeField] private PuzzleStep power;          // בלי חשמל המסוף כבוי
    [SerializeField] private PuzzleGroup checklist;

    // הטקסט הנוכחי של המסוף. ריק = אין חשמל.
    public string Report { get; private set; } = "";

    // מכריז בכל פעם שהטקסט משתנה. הלוח הנייד מאזין לזה.
    public event System.Action<string> ReportChanged;

    // שלושה רגעים משנים את הרשימה: צעד נפתח, צעד נפתר, וצעד שנפתר התקלקל בחזרה
    private void OnEnable()
    {
        PuzzleStep.AnyActivated += HandleStepChanged;
        PuzzleStep.AnySolved += HandleStepChanged;
        PuzzleStep.AnyUnsolved += HandleStepChanged;
    }

    private void OnDisable()
    {
        PuzzleStep.AnyActivated -= HandleStepChanged;
        PuzzleStep.AnySolved -= HandleStepChanged;
        PuzzleStep.AnyUnsolved -= HandleStepChanged;
    }

    private void Start() => Refresh();

    private void HandleStepChanged(PuzzleStep step) => Refresh();

    private void Refresh()
    {
        Report = BuildReport();
        screen.text = Report;
        ReportChanged?.Invoke(Report);
    }

    private string BuildReport()
    {
        if (!power.IsSolved) return "";

        var text = new StringBuilder();
        text.AppendLine("<b>LAB 7 - LOCKDOWN</b>");
        text.AppendLine();
        AppendRow(text, power);
        foreach (PuzzleStep task in checklist.Tasks) AppendRow(text, task);
        text.AppendLine();

        text.Append(checklist.IsSolved
            ? "<color=#3DDC84>LOCKDOWN LIFTED\nMANUAL RELEASE ENABLED</color>"
            : "<color=#FF3B30>DOOR SEALED</color>");

        return text.ToString();
    }

    private static void AppendRow(StringBuilder text, PuzzleStep step)
    {
        text.AppendLine(step.IsSolved
            ? $"<color=#3DDC84>[ OK ]</color> {step.DisplayName}"
            : $"<color=#FF3B30>[ -- ]</color> {step.DisplayName}");
    }
}
