using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

// לוקח מקור קלט יד ומזין ממנו את הרִיג של XRI:
// מיקום לאובייקט היד, ולחיצת תפיסה לאינטראקטור.
// זה הגשר היחיד בין המצלמה למערכת האינטראקציה.
public class XRHandDriver : MonoBehaviour
{
    [SerializeField] private MonoBehaviour handSource;      // חייב לממש IHandInput
    [SerializeField] private NearFarInteractor interactor;  // האינטראקטור של היד הזאת

    private IHandInput hand;

    private void Awake()
    {
        hand = handSource as IHandInput;

        if (hand == null)
        {
            Debug.LogError("XRHandDriver: Hand Source does not implement IHandInput!");
            enabled = false;
            return;
        }

        if (interactor == null)
        {
            Debug.LogError("XRHandDriver: interactor is not assigned in the Inspector!");
            enabled = false;
        }
    }

    private void Update()
    {
        if (!hand.IsTracked)
        {
            // אין יד בתמונה - משחררים במפורש, אחרת עצם יישאר תקוע ביד
            interactor.selectInput.QueueManualState(false, 0f);
            return;
        }

        transform.position = hand.Position;

        // מזין את XRI ישירות. הוא לא יודע ולא אכפת לו שזה לא כפתור אמיתי.
        interactor.selectInput.QueueManualState(hand.IsGrabbing, hand.IsGrabbing ? 1f : 0f);
    }
}