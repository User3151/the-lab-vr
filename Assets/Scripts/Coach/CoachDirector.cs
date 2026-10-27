using System.Collections.Generic;
using UnityEngine;

// המוח של המנחה: מאזין לצעדים בחדר ומחליט מה להגיד ומתי.
// הוא לא מכיר אף חידה בקוד. כל הטקסטים, הרמזים ונקודות העניין מוגדרים ב-Inspector לכל צעד.
// כמה צעדים יכולים להיות פעילים יחד (רשימת הבדיקות), ולכן הרמזים מתחלקים ביניהם.
// שחקן שנתקע לא נשאר לבד: אחרי הרמז האחרון, הרמזים חוזרים מההתחלה כל עוד הוא לא מתקדם.
// כשמשימה נגמרת, המנחה מגיב עליה ומציע את המשימה הפתוחה הקרובה ביותר לשחקן.
// הרחפן טס לנקודת עניין רק כשהמשפט שמדבר עליה מתחיל, ועד אז הוא מסתכל על השחקן.
public class CoachDirector : MonoBehaviour
{
    [System.Serializable]
    public class Hint
    {
        public float afterIdleSeconds = 45f;   // כמה שניות של שקט מאז המשפט הקודם עד הרמז הזה
        public Transform focus;                // לאן הרחפן עף כשהוא אומר אותו
        public CoachLine line;
    }

    [System.Serializable]
    public class StepCoaching
    {
        public PuzzleStep step;
        public Transform focus;
        public CoachLine[] intro;              // למשימות שנפתחות יחד עדיף להשאיר ריק, אחרת הוא ידבר ברצף
        public CoachLine[] nextUp;             // "המשימה הבאה היא..." נאמר כשמשימה אחרת נגמרה וזו עדיין פתוחה
        public Hint[] hints;                   // מסודרים מהעדין לישיר
        public CoachLine[] solved;

        [System.NonSerialized] public int nextHint;   // כמה רמזים כבר נאמרו. ממשיך לעלות גם בסבב החוזר
    }

    [SerializeField] private CoachSpeech speech;
    [SerializeField] private CoachDrone drone;
    [SerializeField] private float focusSeconds = 6f;

    [Header("Script")]
    [SerializeField] private CoachLine[] opening;
    [SerializeField] private StepCoaching[] steps;
    [SerializeField] private CoachLine[] ending;

    [Header("Hints")]
    [SerializeField] private float repeatAfterSeconds = 40f;   // אחרי הסבב הראשון: רמז חוזר כל כמה שניות של שקט

    [Header("Reactions")]
    [SerializeField] private CoachLine[] pressureAlarm;
    [SerializeField] private float reactionCooldown = 8f;

    private readonly List<StepCoaching> active = new();

    // לאן לטוס כשמשפט מסוים מתחיל להישמע. המשפט עצמו הוא המפתח.
    private readonly Dictionary<CoachLine, Transform> focusWhenSaid = new();

    private Transform head;
    private float idleSeconds;
    private bool openingSaid;
    private bool suggestNext;      // משימה נגמרה: בפריים הבא מציעים את הבאה
    private float lastReactionTime = float.NegativeInfinity;
    private int alarmLineIndex;

    private void Awake()
    {
        if (Camera.main != null) head = Camera.main.transform;
    }

    private void OnEnable()
    {
        PuzzleStep.AnyActivated += HandleStepStarted;
        PuzzleStep.AnySolved += HandleStepSolved;
        PuzzleSequencer.OnAllSolved += HandleAllSolved;
        ValvePuzzle.OnAlarm += HandlePressureAlarm;
        speech.LineStarted += HandleLineStarted;
    }

    private void OnDisable()
    {
        PuzzleStep.AnyActivated -= HandleStepStarted;
        PuzzleStep.AnySolved -= HandleStepSolved;
        PuzzleSequencer.OnAllSolved -= HandleAllSolved;
        ValvePuzzle.OnAlarm -= HandlePressureAlarm;
        speech.LineStarted -= HandleLineStarted;
    }

    // הפתיחה לפני הכול. לא ידוע אם Start של ה-Sequencer ירוץ לפני או אחרי זה, ולכן הבדיקה בשני המקומות.
    private void Start() => SayOpeningOnce();

    private void SayOpeningOnce()
    {
        if (openingSaid) return;
        openingSaid = true;
        speech.Say(opening);   // בלי נקודת עניין: בזמן הפתיחה D-7 מסתכל על השחקן
    }

    private void HandleStepStarted(PuzzleStep step)
    {
        SayOpeningOnce();

        StepCoaching coaching = FindCoaching(step);
        if (coaching == null) return;

        coaching.nextHint = 0;
        active.Add(coaching);
        idleSeconds = 0f;

        if (coaching.intro == null || coaching.intro.Length == 0) return;
        SayAt(coaching.intro, coaching.focus);
    }

    private void HandleStepSolved(PuzzleStep step)
    {
        StepCoaching coaching = FindCoaching(step);
        if (coaching == null) return;

        active.Remove(coaching);
        speech.Say(coaching.solved);

        // לא מציעים עכשיו: הצעד הבא אולי עוד לא הופעל (רשימת הבדיקות נפתחת רק אחרי החשמל).
        // בפריים הבא כל מה שנפתח כבר ברשימה.
        suggestNext = true;

        // השחקן התקדם: מתחילים לספור מחדש לפני הרמז הבא
        idleSeconds = 0f;
    }

    private void HandleAllSolved() => speech.Say(ending);

    private void HandlePressureAlarm()
    {
        drone.FlashEye(Color.red, 2f);
        if (pressureAlarm == null || pressureAlarm.Length == 0) return;
        if (Time.time - lastReactionTime < reactionCooldown) return;

        lastReactionTime = Time.time;
        speech.Interrupt(pressureAlarm[alarmLineIndex % pressureAlarm.Length]);
        alarmLineIndex++;

        // מי שהפעיל אזעקה עדיין מתקדם: לא ממהרים לתת לו רמז נוסף
        idleSeconds = 0f;
    }

    private void Update()
    {
        if (suggestNext)
        {
            suggestNext = false;
            SuggestNextTask();
        }

        // רמזים מדורגים: הזמן נספר רק כשהמנחה שותק, כדי שלא ידבר ברצף
        StepCoaching target = NextToHint();
        if (target == null || speech.IsSpeaking) return;

        idleSeconds += Time.deltaTime;

        // % (שארית החלוקה) מחזיר להתחלה אחרי הרמז האחרון: 0, 1, 2, 3, 0, 1...
        bool firstRound = target.nextHint < target.hints.Length;
        Hint hint = target.hints[target.nextHint % target.hints.Length];

        // בסבב הראשון כל רמז מחכה לזמן שלו. אחר כך כולם מחכים אותו זמן.
        float wait = firstRound ? hint.afterIdleSeconds : repeatAfterSeconds;
        if (idleSeconds < wait) return;

        SayAt(new[] { hint.line }, hint.focus != null ? hint.focus : target.focus);
        target.nextHint++;
        idleSeconds = 0f;   // הרמז הבא נמדד מסוף הרמז הזה
    }

    // המשפט נכנס לתור אחרי משפט ההצלחה, כך ש-D-7 קודם מברך ואז מכוון
    private void SuggestNextTask()
    {
        StepCoaching next = NearestOpenTask();
        if (next == null) return;

        SayAt(next.nextUp, next.focus);
        idleSeconds = 0f;
    }

    // מכניס משפטים לתור, וזוכר לטוס לנקודה כשהראשון שבהם יתחיל.
    // "הראשון" לפי אותו כלל של CoachSpeech: משפט ריק לא נכנס לתור ולכן גם לא יתחיל אף פעם.
    private void SayAt(CoachLine[] lines, Transform focus)
    {
        if (lines == null) return;
        speech.Say(lines);
        if (focus == null) return;

        foreach (CoachLine line in lines)
        {
            if (line == null || string.IsNullOrWhiteSpace(line.text)) continue;
            focusWhenSaid[line] = focus;
            return;
        }
    }

    // Remove מוציא את הרשומה ומחזיר את הנקודה, כך שכל טיסה קורית פעם אחת
    private void HandleLineStarted(CoachLine line)
    {
        if (focusWhenSaid.Remove(line, out Transform focus))
            drone.FocusOn(focus, focusSeconds);
    }

    // מבין המשימות הפתוחות שיש להן משפט "הבאה בתור", זו שהכי קרובה לשחקן.
    // כך הסרטון זורם בחדר, במקום לשלוח את השחקן הלוך ושוב מקיר לקיר.
    private StepCoaching NearestOpenTask()
    {
        if (head == null) return null;

        StepCoaching best = null;
        float bestDistance = float.MaxValue;
        foreach (StepCoaching coaching in active)
        {
            if (coaching.nextUp == null || coaching.nextUp.Length == 0 || coaching.focus == null) continue;

            // sqrMagnitude: מרחק בריבוע. להשוואה זה מספיק, וחוסך שורש
            float distance = (coaching.focus.position - head.position).sqrMagnitude;
            if (distance < bestDistance)
            {
                best = coaching;
                bestDistance = distance;
            }
        }
        return best;
    }

    // בין כל הצעדים הפעילים, זה שקיבל הכי מעט רמזים. כך כל משימה מקבלת קודם את הרמז העדין שלה,
    // ורק אחר כך מגיעים הרמזים הישירים. בשוויון, לפי הסדר ב-Inspector.
    // צעד בלי רמזים בכלל מדולג. צעד שסיים את הרמזים שלו כבר לא מדולג: הוא מתחיל סבב חדש.
    private StepCoaching NextToHint()
    {
        StepCoaching best = null;
        foreach (StepCoaching coaching in active)
        {
            if (coaching.hints == null || coaching.hints.Length == 0) continue;
            if (best == null || coaching.nextHint < best.nextHint) best = coaching;
        }
        return best;
    }

    private StepCoaching FindCoaching(PuzzleStep step)
    {
        foreach (StepCoaching coaching in steps)
            if (coaching.step == step) return coaching;
        return null;
    }
}
