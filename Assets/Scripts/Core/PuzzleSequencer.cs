using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// מכונת המצבים של החדר: מפעילה צעד אחד בכל פעם, לפי הסדר.
// המצב הוא האינדקס של הצעד הפעיל. אחרי הצעד האחרון, החדר נפתר.
public class PuzzleSequencer : MonoBehaviour
{
    [SerializeField] private List<PuzzleStep> steps = new();

    // לחיבור בסצנה, דרך ה-Inspector: מה קורה כשהכול נפתר (הדלת)
    [SerializeField] private UnityEvent onAllSolved;

    // לחיבור מקוד: המאמן יאזין לאלה בלי להכיר אף חידה ספציפית
    public static event System.Action<PuzzleStep> OnStepStarted;
    public static event System.Action<PuzzleStep> OnStepSolved;
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
        OnStepStarted?.Invoke(step);
        Debug.Log($"Step {currentIndex + 1}/{steps.Count} started: {step.DisplayName}");

        step.Activate();
    }

    private void HandleStepSolved(PuzzleStep step)
    {
        step.Solved -= HandleStepSolved;
        OnStepSolved?.Invoke(step);
        AdvanceTo(currentIndex + 1);
    }
}
