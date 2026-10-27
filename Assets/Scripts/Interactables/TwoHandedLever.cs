using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

// ידית כבדה על ציר: זזה רק כששתי ידיים אוחזות בה יחד.
// אם יד אחת משחררת, הידית חוזרת למעלה. אם מושכים אותה עד הסוף, היא ננעלת.
// הידית עוקבת אחרי כמה הידיים זזו לאורך המסלול של הקצה שלה, ולא אחרי הזווית שלהן סביב הציר.
// כך היא זזה אותו דבר בין אם עומדים צמוד אליה ובין אם תופסים אותה בקרן מרחוק.
public class TwoHandedLever : XRBaseInteractable
{
    [Header("Hinge")]
    [SerializeField] private Vector3 localHingeAxis = Vector3.right;   // הציר שסביבו הידית מסתובבת
    [SerializeField] private float maxAngle = 80f;                     // משיכה מלאה, במעלות
    [SerializeField] private float handleLength = 0.62f;               // מהציר עד המוט שתופסים, במטרים

    [Header("Feel")]
    [SerializeField] private float returnSpeed = 60f;          // מעלות לשנייה כשאין שתי ידיים
    [SerializeField] private float pulledAtPercent = 0.95f;    // מאיזה אחוז מהמהלך נחשב משוך
    [SerializeField] private float pullGain = 1.5f;            // כל ס"מ של הידיים מזיז את הקצה 1.5 ס"מ
    [SerializeField] private float leadAngle = 30f;            // מודדים בכיוון שהקצה יזוז בו עוד 30 מעלות

    // מכריז כשהידית נמשכה עד הסוף. הפאזל יאזין לזה ולא יצטרך להכיר את הידית.
    public static event System.Action<TwoHandedLever> OnPulled;

    private Quaternion restLocalRotation;
    private float currentAngle;
    private Vector3 lastMidpoint;    // איפה היו הידיים בפריים הקודם
    private bool wasTwoHanded;
    private bool locked;

    protected override void Awake()
    {
        base.Awake();
        restLocalRotation = transform.localRotation;

        // ברירת המחדל היא Single: יד שנייה "גונבת" את הבחירה מהראשונה, ואף פעם אין שתיים יחד
        selectMode = InteractableSelectMode.Multiple;
    }

    public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
    {
        // Dynamic ולא Fixed כמו במגירה: כאן אין פיזיקה, רק סיבוב.
        // מעדכנים בקצב התמונה כדי שהתנועה תיראה חלקה בקסדה.
        if (updatePhase != XRInteractionUpdateOrder.UpdatePhase.Dynamic) return;
        if (locked) return;

        bool twoHanded = interactorsSelecting.Count >= 2;

        if (twoHanded)
        {
            Vector3 midpoint = (interactorsSelecting[0].transform.position
                              + interactorsSelecting[1].transform.position) * 0.5f;

            // הפריים הראשון עם שתי ידיים רק זוכר איפה הן, כדי שהידית לא תקפוץ.
            // מכאן והלאה מצטברת רק התזוזה מהפריים הקודם.
            if (wasTwoHanded)
                currentAngle = Mathf.Clamp(currentAngle + AngleFromHandMotion(midpoint - lastMidpoint), 0f, maxAngle);

            lastMidpoint = midpoint;
        }
        else
        {
            // קפיץ: בלי שתי ידיים הידית עולה חזרה
            currentAngle = Mathf.MoveTowards(currentAngle, 0f, returnSpeed * Time.deltaTime);
        }

        wasTwoHanded = twoHanded;
        transform.localRotation = restLocalRotation * Quaternion.AngleAxis(currentAngle, localHingeAxis);

        CheckPulled();
    }

    // כמה מעלות שווה תזוזה של הידיים.
    // נספר רק החלק של התזוזה שהולך בכיוון שהקצה של הידית זז בו. הזזה לצדדים לא מזיזה אותה.
    private float AngleFromHandMotion(Vector3 handDelta)
    {
        Quaternion restWorld = transform.parent != null
            ? transform.parent.rotation * restLocalRotation
            : restLocalRotation;

        Vector3 axis = restWorld * localHingeAxis.normalized;

        // הכיוון שבו הקצה זז במנוחה: ניצב גם לציר וגם למוט (מכפלה וקטורית)
        Vector3 restTangent = Vector3.Cross(axis, restWorld * Vector3.up);

        // למעלה, הקצה זז כמעט רק לכיוון השחקן, ומשיכה ישרה למטה לא הייתה עושה כלום.
        // לכן מודדים בכיוון שהוא יזוז בו קצת יותר מאוחר, ולא יותר מ-90 מעלות (ישר למטה).
        float lookAhead = Mathf.Min(currentAngle + leadAngle, 90f);
        Vector3 tangent = Quaternion.AngleAxis(lookAhead, axis) * restTangent;

        float alongPath = Vector3.Dot(handDelta, tangent) * pullGain;

        // אורך קשת = רדיוס כפול זווית ברדיאנים
        return alongPath / handleLength * Mathf.Rad2Deg;
    }

    private void CheckPulled()
    {
        if (currentAngle < maxAngle * pulledAtPercent) return;

        locked = true;
        currentAngle = maxAngle;
        transform.localRotation = restLocalRotation * Quaternion.AngleAxis(maxAngle, localHingeAxis);

        OnPulled?.Invoke(this);
        Debug.Log($"{name} pulled");
    }
}
