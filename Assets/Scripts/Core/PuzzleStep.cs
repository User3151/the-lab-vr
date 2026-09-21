using UnityEngine;

// צעד אחד בשרשרת החידות.
// הצעד יודע מתי התנאי שלו מתקיים ומה קורה אז. הוא לא יודע מה בא אחריו, זה התפקיד של ה-Sequencer (או של PuzzleGroup).
//
// פתרון הוא מצב ולא רגע: משימה שנפתרה ממשיכה להיבדק, ואם השחקן מקלקל אותה בחזרה היא חוזרת להיות לא פתורה.
// לכן IsActive נשאר true גם אחרי הפתרון. הוא אומר "הגיע התור של הצעד והוא במעקב",
// ו-IsSolved אומר "התנאי מתקיים עכשיו".
public abstract class PuzzleStep : MonoBehaviour
{
    [SerializeField] private string displayName = "Puzzle";

    public event System.Action<PuzzleStep> Solved;
    public event System.Action<PuzzleStep> Unsolved;

    // לחיבור מקוד: המאמן והמסוף מאזינים לכל הצעדים בחדר, גם לכאלה שרצים במקביל בתוך קבוצה
    public static event System.Action<PuzzleStep> AnyActivated;
    public static event System.Action<PuzzleStep> AnySolved;
    public static event System.Action<PuzzleStep> AnyUnsolved;

    public string DisplayName => displayName;
    public bool IsActive { get; private set; }
    public bool IsSolved { get; private set; }

    // ה-Sequencer קורא לזה כשמגיע תורו של הצעד
    public void Activate()
    {
        if (IsActive) return;
        IsActive = true;

        // מכריזים לפני OnActivated: קבוצה מפעילה בתוכו את המשימות שלה,
        // והמאמן צריך לשמוע קודם על הקבוצה ורק אחר כך על המשימות
        AnyActivated?.Invoke(this);
        OnActivated();
    }

    // ה-Sequencer קורא לזה כשצעד קודם התקלקל והתור חוזר אחורה.
    // הצעד יוצא מהמעקב ומאבד את הפתרון שלו, ויופעל מחדש כשהתור יחזור אליו.
    public void Deactivate()
    {
        if (!IsActive) return;
        IsActive = false;
        IsSolved = false;
        OnDeactivated();
    }

    // תת-המחלקה קוראת לזה כשהתנאי שלה מתקיים.
    // צעד שעוד לא הגיע תורו מתעלם, כך שאי אפשר לפתור את השרשרת מהסוף.
    protected void Solve()
    {
        if (!IsActive || IsSolved) return;
        IsSolved = true;

        OnSolved();

        // קודם ההכרזה הכללית ורק אז האירוע של המופע: האירוע של המופע יכול לפתור קבוצה
        // או לקדם את ה-Sequencer, והמשפט "המשימה נפתרה" צריך להגיע לפני "הכול נפתר"
        AnySolved?.Invoke(this);
        Solved?.Invoke(this);
    }

    // תת-המחלקה קוראת לזה כשתנאי שכבר התקיים הפסיק להתקיים.
    // הצעד נשאר פעיל, כדי שיוכל להיפתר שוב.
    protected void Unsolve()
    {
        if (!IsActive || !IsSolved) return;
        IsSolved = false;

        OnUnsolved();

        // אותו סדר כמו בפתרון: קודם המסוף והמאמן, ורק אחר כך הקבוצה וה-Sequencer
        AnyUnsolved?.Invoke(this);
        Unsolved?.Invoke(this);
    }

    // נקודות הרחבה לתת-המחלקות
    protected virtual void OnActivated() { }
    protected virtual void OnDeactivated() { }
    protected abstract void OnSolved();
    protected virtual void OnUnsolved() { }
}
