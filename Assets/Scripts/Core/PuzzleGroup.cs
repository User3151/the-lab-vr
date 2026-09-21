using System.Collections.Generic;
using UnityEngine;

// צעד שמורכב מכמה משימות שרצות במקביל, כמו רשימת בדיקות בטיחות.
// כשהקבוצה מופעלת, כל המשימות נפתחות יחד והשחקן בוחר את הסדר. הקבוצה נפתרת כשכולן נפתרו,
// וחוזרת להיות לא פתורה ברגע שאחת מהן התקלקלה.
public class PuzzleGroup : PuzzleStep
{
    [SerializeField] private List<PuzzleStep> tasks = new();

    public IReadOnlyList<PuzzleStep> Tasks => tasks;

    protected override void OnActivated()
    {
        // נרשמים לכולן לפני שמפעילים: משימה שכבר מתקיימת נפתרת מיד בתוך Activate.
        // לא מתנתקים אחרי פתרון: משימה יכולה להתקלקל ולהיפתר שוב, וצריך לשמוע את שניהם.
        foreach (PuzzleStep task in tasks)
        {
            task.Solved += HandleTaskChanged;
            task.Unsolved += HandleTaskChanged;
        }
        foreach (PuzzleStep task in tasks) task.Activate();

        Refresh();
    }

    // הקבוצה יצאה מהמעקב (צעד לפניה התקלקל): המשימות שלה יוצאות איתה
    protected override void OnDeactivated()
    {
        foreach (PuzzleStep task in tasks)
        {
            task.Solved -= HandleTaskChanged;
            task.Unsolved -= HandleTaskChanged;
            task.Deactivate();
        }
    }

    private void HandleTaskChanged(PuzzleStep task) => Refresh();

    // המצב של הקבוצה נגזר תמיד מהמשימות ולא נשמר בנפרד, כך שהוא לא יכול להיתקע.
    // Solve ו-Unsolve מתעלמים כשהמצב כבר נכון, אז מותר לקרוא לזה כמה פעמים שרוצים.
    private void Refresh()
    {
        if (AllDone()) Solve();
        else Unsolve();
    }

    private bool AllDone()
    {
        foreach (PuzzleStep task in tasks)
            if (!task.IsSolved) return false;
        return true;
    }

    protected override void OnSolved() => Debug.Log($"{DisplayName} complete");
    protected override void OnUnsolved() => Debug.Log($"{DisplayName} incomplete again");
}
