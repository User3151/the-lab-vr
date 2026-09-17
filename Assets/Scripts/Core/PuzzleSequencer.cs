using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// מכונת המצבים של החדר: מפעילה צעד אחד בכל פעם, לפי הסדר.
// המצב הוא האינדקס של הצעד הפעיל. אחרי הצעד האחרון, החדר נפתר.
// צעד יכול להיות קבוצה (PuzzleGroup) שמריצה כמה משימות במקביל, והסדר הכללי נשאר ליניארי.
public class PuzzleSequencer : MonoBehaviour
{
    [SerializeField] private List<PuzzleStep> steps = new();

    // לחיבור בסצנה, דרך ה-Inspector: מה קורה כשהכול נפתר (הדלת)
    [SerializeField] private UnityEvent onAllSolved;

    // לחיבור מקוד: המאמן מאזין לזה. התחלה ופתרון של צעדים מוכרזים ב-PuzzleStep עצמו.
    public static event System.Action OnAllSolved;

    private int currentIndex = -1;

    private void Start() => AdvanceTo(0);

    private void AdvanceTo(int index)
    {
        currentIndex = index;

        if (currentIndex >= steps.Count)
        {
            Debug.Log("All puzzles solved");
            OnAllSolved?.Invoke();
            onAllSolved.Invoke();
            return;
        }

        PuzzleStep step = steps[currentIndex];

        // נרשמים לפני Activate: צעד שכבר מתקיים יכול להיפתר מיד בתוך Activate
        step.Solved += HandleStepSolved;
        Debug.Log($"Step {currentIndex + 1}/{steps.Count} started: {step.DisplayName}");

        step.Activate();
    }

    private void HandleStepSolved(PuzzleStep step)
    {
        step.Solved -= HandleStepSolved;
        AdvanceTo(currentIndex + 1);
    }
}
