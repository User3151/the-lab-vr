using UnityEngine;

// הצעד האחרון: מושכים את הידית הראשית בשתי ידיים.
// עד שמגיע תורה אין לה חשמל, והיא לא זזה בכלל.
// אם רשימת הבדיקות מתקלקלת לפני שמשכו אותה, החשמל שלה נקטע שוב.
public class LeverPuzzle : PuzzleStep
{
    [SerializeField] private TwoHandedLever lever;

    // כבויה מההתחלה, כדי שאי אפשר יהיה לפתור את החדר מהסוף
    private void Awake() => lever.enabled = false;

    private void OnEnable()  => TwoHandedLever.OnPulled += HandlePulled;
    private void OnDisable() => TwoHandedLever.OnPulled -= HandlePulled;

    // ה-Sequencer הגיע לצעד הזה: החשמל חוזר לידית
    protected override void OnActivated() => lever.enabled = true;

    // משימה ברשימה התקלקלה והתור חזר אחורה: ההסגר חוזר, והידית שוב בלי חשמל.
    // כיבוי של אינטראקטבל ב-XRI משחרר אותו גם מיד שמחזיקה בו ברגע זה.
    protected override void OnDeactivated() => lever.enabled = false;

    private void HandlePulled(TwoHandedLever pulled)
    {
        if (pulled != lever) return;
        Solve();
    }

    protected override void OnSolved() => Debug.Log("Final step solved: lever pulled");
}
