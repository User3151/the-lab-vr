using UnityEngine;

// צעד אחד בשרשרת החידות.
// הצעד יודע מתי הוא נפתר ומה קורה אז. הוא לא יודע מה בא אחריו, זה התפקיד של ה-Sequencer.
public abstract class PuzzleStep : MonoBehaviour
{
    [SerializeField] private string displayName = "Puzzle";

    public event System.Action<PuzzleStep> Solved;

    public string DisplayName => displayName;
    public bool IsActive { get; private set; }
    public bool IsSolved { get; private set; }

    // ה-Sequencer קורא לזה כשמגיע תורו של הצעד
    public void Activate()
    {
        if (IsActive || IsSolved) return;
        IsActive = true;
        OnActivated();
    }

    // תת-המחלקה קוראת לזה כשהתנאי שלה מתקיים.
    // צעד שעוד לא הגיע תורו מתעלם, כך שאי אפשר לפתור את השרשרת מהסוף.
    protected void Solve()
    {
        if (!IsActive || IsSolved) return;
        IsActive = false;
        IsSolved = true;

        OnSolved();
        Solved?.Invoke(this);
    }

    // נקודות הרחבה לתת-המחלקות
    protected virtual void OnActivated() { }
    protected abstract void OnSolved();
}
