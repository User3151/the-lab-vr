using System.Collections.Generic;
using UnityEngine;

// המוח של המנחה: מאזין לצעדים בחדר ומחליט מה להגיד ומתי.
// הוא לא מכיר אף חידה בקוד. כל הטקסטים, הרמזים ונקודות העניין מוגדרים ב-Inspector לכל צעד.
// כמה צעדים יכולים להיות פעילים יחד (רשימת הבדיקות), ולכן הרמזים מתחלקים ביניהם.
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
        public Hint[] hints;                   // מסודרים מהעדין לישיר
        public CoachLine[] solved;

        [System.NonSerialized] public int nextHint;
    }

    [SerializeField] private CoachSpeech speech;
    [SerializeField] private CoachDrone drone;
    [SerializeField] private float focusSeconds = 6f;

    [Header("Script")]
    [SerializeField] private CoachLine[] opening;
    [SerializeField] private StepCoaching[] steps;
    [SerializeField] private CoachLine[] ending;

    [Header("Reactions")]
    [SerializeField] private CoachLine[] pressureAlarm;
    [SerializeField] private float reactionCooldown = 8f;

    private readonly List<StepCoaching> active = new();
    private float idleSeconds;
    private bool openingSaid;
    private float lastReactionTime = float.NegativeInfinity;
    private int alarmLineIndex;

    private void OnEnable()
    {
        PuzzleStep.AnyActivated += HandleStepStarted;
        PuzzleStep.AnySolved += HandleStepSolved;
        PuzzleSequencer.OnAllSolved += HandleAllSolved;
        ValvePuzzle.OnAlarm += HandlePressureAlarm;
    }

    private void OnDisable()
    {
        PuzzleStep.AnyActivated -= HandleStepStarted;
        PuzzleStep.AnySolved -= HandleStepSolved;
        PuzzleSequencer.OnAllSolved -= HandleAllSolved;
        ValvePuzzle.OnAlarm -= HandlePressureAlarm;
    }

    // הפתיחה לפני הכול. לא ידוע אם Start של ה-Sequencer ירוץ לפני או אחרי זה, ולכן הבדיקה בשני המקומות.
    private void Start() => SayOpeningOnce();

    private void SayOpeningOnce()
    {
        if (openingSaid) return;
        openingSaid = true;
        speech.Say(opening);
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
        speech.Say(coaching.intro);
        drone.FocusOn(coaching.focus, focusSeconds);
    }

    private void HandleStepSolved(PuzzleStep step)
    {
        StepCoaching coaching = FindCoaching(step);
        if (coaching == null) return;

        active.Remove(coaching);
        speech.Say(coaching.solved);

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

    // רמזים מדורגים: הזמן נספר רק כשהמנחה שותק, כדי שלא ידבר ברצף
    private void Update()
    {
        StepCoaching target = NextToHint();
        if (target == null || speech.IsSpeaking) return;

        idleSeconds += Time.deltaTime;

        Hint hint = target.hints[target.nextHint];
        if (idleSeconds < hint.afterIdleSeconds) return;

        speech.Say(hint.line);
        drone.FocusOn(hint.focus != null ? hint.focus : target.focus, focusSeconds);
        target.nextHint++;
        idleSeconds = 0f;   // הרמז הבא נמדד מסוף הרמז הזה
    }

    // בין כל הצעדים הפעילים, זה שקיבל הכי מעט רמזים. כך כל משימה מקבלת קודם את הרמז העדין שלה,
    // ורק אחר כך מגיעים הרמזים הישירים. בשוויון, לפי הסדר ב-Inspector.
    private StepCoaching NextToHint()
    {
        StepCoaching best = null;
        foreach (StepCoaching coaching in active)
        {
            if (coaching.hints == null || coaching.nextHint >= coaching.hints.Length) continue;
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
