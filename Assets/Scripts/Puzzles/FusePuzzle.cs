using System.Collections;
using UnityEngine;

// חידה 1: הנתיך נכנס לשקע, והאור חוזר לחדר.
// הבקר מכיר את השקע ואת האור. השקע לא מכיר אף אחד מהם.
public class FusePuzzle : PuzzleStep
{
    [Header("Input")]
    [SerializeField] private ValidatedSocket fuseSocket;

    [Header("Lights")]
    [SerializeField] private Light roomLight;
    [SerializeField] private float litIntensity = 1f;
    [SerializeField] private float fadeSeconds = 1.5f;

    // אירוע static חי יותר זמן מהאובייקט, ובלי ניתוק הוא ימשיך לקרוא לאובייקט שנמחק.
    private void OnEnable()  => ValidatedSocket.OnFilled += HandleFilled;
    private void OnDisable() => ValidatedSocket.OnFilled -= HandleFilled;

    private void HandleFilled(ValidatedSocket socket)
    {
        // האירוע משותף לכל השקעים בחדר, אז מגיבים רק לשקע שלנו
        if (socket != fuseSocket) return;
        Solve();
    }

    protected override void OnSolved()
    {
        StartCoroutine(FadeLightIn());
        Debug.Log("Puzzle 1 solved: lights on");
    }

    // האור עולה בהדרגה ולא נדלק בבת אחת. בסרטון זה נראה הרבה יותר טוב.
    private IEnumerator FadeLightIn()
    {
        float start = roomLight.intensity;
        for (float t = 0f; t < fadeSeconds; t += Time.deltaTime)
        {
            roomLight.intensity = Mathf.Lerp(start, litIntensity, t / fadeSeconds);
            yield return null;
        }
        roomLight.intensity = litIntensity;
    }
}
