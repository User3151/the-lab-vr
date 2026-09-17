using UnityEngine;

// משימת בטיחות: לכבות את האש שהנתיך השרוף הצית.
// אפשר לכבות אותה עוד לפני שהחשמל חוזר. המשימה פשוט תסומן ברגע שרשימת הבדיקות נפתחת.
public class FirePuzzle : PuzzleStep
{
    [SerializeField] private Fire fire;

    private void OnEnable()  => Fire.OnExtinguished += HandleExtinguished;
    private void OnDisable() => Fire.OnExtinguished -= HandleExtinguished;

    // האש אולי כבר כבתה לפני שהגיע התור: בודקים ברגע ההפעלה
    protected override void OnActivated()
    {
        if (fire.IsOut) Solve();
    }

    // אם המשימה עוד לא פעילה, Solve מתעלם, ו-OnActivated יתפוס את זה אחר כך
    private void HandleExtinguished(Fire extinguished)
    {
        if (extinguished != fire) return;
        Solve();
    }

    protected override void OnSolved() => Debug.Log("Safety task solved: fire out");
}
