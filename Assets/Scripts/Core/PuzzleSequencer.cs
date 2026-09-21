using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// מכונת המצבים של החדר: מפעילה צעד אחד בכל פעם, לפי הסדר.
// המצב הוא האינדקס של הצעד הנוכחי. אחרי הצעד האחרון, החדר נפתר.
// צעד יכול להיות קבוצה (PuzzleGroup) שמריצה כמה משימות במקביל, והסדר הכללי נשאר ליניארי.
//
// הסדר יכול גם לחזור אחורה: אם צעד שכבר נפתר התקלקל, כל הצעדים שאחריו יוצאים מהמעקב
// והתור חוזר אליו. כשהוא ייפתר שוב, השרשרת ממשיכה משם כרגיל.
public class PuzzleSequencer : MonoBehaviour
{
    [SerializeField] private List<PuzzleStep> steps = new();

    // לחיבור בסצנה, דרך ה-Inspector: מה קורה כשהכול נפתר (הדלת)
    [SerializeField] private UnityEvent onAllSolved;

    // לחיבור מקוד: המאמן מאזין לזה. התחלה ופתרון של צעדים מוכרזים ב-PuzzleStep עצמו.
    public static event System.Action OnAllSolved;

    private int currentIndex = -1;

    private bool Finished => currentIndex >= steps.Count;

    // נרשמים לכל הצעדים פעם אחת, מראש. צעד שלא הגיע תורו לא יכול להיפתר או להתקלקל,
    // כך שהאירועים שלו פשוט לא יגיעו עד שיגיע.
    private void Start()
    {
        foreach (PuzzleStep step in steps)
        {
            step.Solved += HandleStepSolved;
            step.Unsolved += HandleStepUnsolved;
        }
        AdvanceTo(0);
    }

    private void OnDestroy()
    {
        foreach (PuzzleStep step in steps)
        {
            if (step == null) continue;
            step.Solved -= HandleStepSolved;
            step.Unsolved -= HandleStepUnsolved;
        }
    }

    private void AdvanceTo(int index)
    {
        currentIndex = index;

        if (Finished)
        {
            Debug.Log("All puzzles solved");
            OnAllSolved?.Invoke();
            onAllSolved.Invoke();
            return;
        }

        PuzzleStep step = steps[currentIndex];
        Debug.Log($"Step {currentIndex + 1}/{steps.Count} started: {step.DisplayName}");

        // צעד שכבר מתקיים נפתר מיד בתוך Activate, ו-HandleStepSolved ימשיך הלאה
        step.Activate();
    }

    // רק הצעד הנוכחי מקדם את השרשרת
    private void HandleStepSolved(PuzzleStep step)
    {
        if (steps.IndexOf(step) != currentIndex) return;
        AdvanceTo(currentIndex + 1);
    }

    private void HandleStepUnsolved(PuzzleStep step)
    {
        // אחרי שהדלת נפתחה המשחק נגמר, ושום קלקול כבר לא סוגר אותה
        if (Finished) return;

        int index = steps.IndexOf(step);
        if (index < 0 || index >= currentIndex) return;

        // מהסוף להתחלה: הצעד המאוחר ביותר יוצא ראשון, כמו שנכנס אחרון
        for (int i = currentIndex; i > index; i--)
            steps[i].Deactivate();

        currentIndex = index;
        Debug.Log($"Step {index + 1}/{steps.Count} broke again: back to {step.DisplayName}");
    }
}
