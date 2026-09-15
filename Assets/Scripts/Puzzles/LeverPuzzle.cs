using UnityEngine;

// חידה 4: מושכים את הידית הראשית בשתי ידיים.
// עד שמגיע תורה אין לה חשמל, והיא לא זזה בכלל.
public class LeverPuzzle : PuzzleStep
{
    [SerializeField] private TwoHandedLever lever;

    // כבויה מההתחלה, כדי שאי אפשר יהיה לפתור את החדר מהסוף
    private void Awake() => lever.enabled = false;

    private void OnEnable()  => TwoHandedLever.OnPulled += HandlePulled;
    private void OnDisable() => TwoHandedLever.OnPulled -= HandlePulled;

    // ה-Sequencer הגיע לצעד הזה: החשמל חוזר לידית
    protected override void OnActivated() => lever.enabled = true;

    private void HandlePulled(TwoHandedLever pulled)
    {
        if (pulled != lever) return;
        Solve();
    }

    protected override void OnSolved() => Debug.Log("Puzzle 4 solved: lever pulled");
}
