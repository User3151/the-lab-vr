using UnityEngine;

// המוח של המנחה: מאזין לרצף החידות ומחליט מה להגיד ומתי.
// הוא לא מכיר אף חידה בקוד. כל הטקסטים, הרמזים ונקודות העניין מוגדרים ב-Inspector לכל צעד.
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
        public CoachLine[] intro;
        public Hint[] hints;                   // מסודרים מהעדין לישיר
        public CoachLine[] solved;
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

    private StepCoaching current;
    private int nextHint;
    private float idleSeconds;
    private bool openingSaid;
    private float lastReactionTime = float.NegativeInfinity;
    private int alarmLineIndex;

    private void OnEnable()
    {
        PuzzleSequencer.OnStepStarted += HandleStepStarted;
        PuzzleSequencer.OnStepSolved += HandleStepSolved;
        PuzzleSequencer.OnAllSolved += HandleAllSolved;
        ValvePuzzle.OnAlarm += HandlePressureAlarm;
    }

    private void OnDisable()
    {
        PuzzleSequencer.OnStepStarted -= HandleStepStarted;
        PuzzleSequencer.OnStepSolved -= HandleStepSolved;
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

        current = FindCoaching(step);
        nextHint = 0;
        idleSeconds = 0f;
        if (current == null) return;

        speech.Say(current.intro);
        drone.FocusOn(current.focus, focusSeconds);
    }

    private void HandleStepSolved(PuzzleStep step)
    {
        StepCoaching solved = FindCoaching(step);
        if (solved != null) speech.Say(solved.solved);
        current = null;
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
        if (current == null || current.hints == null || nextHint >= current.hints.Length) return;
        if (speech.IsSpeaking) return;

        idleSeconds += Time.deltaTime;

        Hint hint = current.hints[nextHint];
        if (idleSeconds < hint.afterIdleSeconds) return;

        speech.Say(hint.line);
        drone.FocusOn(hint.focus != null ? hint.focus : current.focus, focusSeconds);
        nextHint++;
        idleSeconds = 0f;   // הרמז הבא נמדד מסוף הרמז הזה
    }

    private StepCoaching FindCoaching(PuzzleStep step)
    {
        foreach (StepCoaching coaching in steps)
            if (coaching.step == step) return coaching;
        return null;
    }
}
