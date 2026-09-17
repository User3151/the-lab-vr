using UnityEngine;

// צעד אחד בשרשרת החידות.
// הצעד יודע מתי הוא נפתר ומה קורה אז. הוא לא יודע מה בא אחריו, זה התפקיד של ה-Sequencer (או של PuzzleGroup).
public abstract class PuzzleStep : MonoBehaviour
{
    [SerializeField] private string displayName = "Puzzle";

    public event System.Action<PuzzleStep> Solved;

    // לחיבור מקוד: המאמן והמסוף מאזינים לכל הצעדים בחדר, גם לכאלה שרצים במקביל בתוך קבוצה
    public static event System.Action<PuzzleStep> AnyActivated;
    public static event System.Action<PuzzleStep> AnySolved;

    public string DisplayName => displayName;
    public bool IsActive { get; private set; }
    public bool IsSolved { get; private set; }

    // ה-Sequencer קורא לזה כשמגיע תורו של הצעד
    public void Activate()
    {
        if (IsActive || IsSolved) return;
        IsActive = true;

        // מכריזים לפני OnActivated: קבוצה מפעילה בתוכו את המשימות שלה,
        // והמאמן צריך לשמוע קודם על הקבוצה ורק אחר כך על המשימות
        AnyActivated?.Invoke(this);
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

        // קודם ההכרזה הכללית ורק אז האירוע של המופע: האירוע של המופע יכול לפתור קבוצה
        // או לקדם את ה-Sequencer, והמשפט "המשימה נפתרה" צריך להגיע לפני "הכול נפתר"
        AnySolved?.Invoke(this);
        Solved?.Invoke(this);
    }

    // נקודות הרחבה לתת-המחלקות
    protected virtual void OnActivated() { }
    protected abstract void OnSolved();
}
