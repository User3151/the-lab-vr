using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

// ידית כבדה על ציר: זזה רק כששתי ידיים אוחזות בה יחד.
// אם יד אחת משחררת, הידית חוזרת למעלה. אם מושכים אותה עד הסוף, היא ננעלת.
public class TwoHandedLever : XRBaseInteractable
{
    [Header("Hinge")]
    [SerializeField] private Vector3 localHingeAxis = Vector3.right;   // הציר שסביבו הידית מסתובבת
    [SerializeField] private float maxAngle = 80f;                     // משיכה מלאה, במעלות

    [Header("Feel")]
    [SerializeField] private float returnSpeed = 60f;          // מעלות לשנייה כשאין שתי ידיים
    [SerializeField] private float pulledAtPercent = 0.95f;    // מאיזה אחוז מהמהלך נחשב משוך

    // מכריז כשהידית נמשכה עד הסוף. הפאזל יאזין לזה ולא יצטרך להכיר את הידית.
    public static event System.Action<TwoHandedLever> OnPulled;

    private Quaternion restLocalRotation;
    private float currentAngle;
    private float grabHandsAngle;    // זווית הידיים ברגע שהיד השנייה הצטרפה
    private float grabLeverAngle;    // זווית הידית באותו רגע
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
            // הרגע שבו היד השנייה הצטרפה: זוכרים את ההפרש, כדי שהידית לא תקפוץ אל הידיים
            if (!wasTwoHanded)
            {
                grabHandsAngle = HandsAngle();
                grabLeverAngle = currentAngle;
            }

            currentAngle = Mathf.Clamp(grabLeverAngle + HandsAngle() - grabHandsAngle, 0f, maxAngle);
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

    // הזווית של נקודת האמצע בין שתי הידיים, סביב ציר הידית.
    // ההטלה על המישור מבטלת תנועה לאורך הציר עצמו: הזזה לצדדים לא נספרת.
    private float HandsAngle()
    {
        Vector3 midpoint = (interactorsSelecting[0].transform.position
                          + interactorsSelecting[1].transform.position) * 0.5f;

        Quaternion restWorld = transform.parent != null
            ? transform.parent.rotation * restLocalRotation
            : restLocalRotation;

        Vector3 axis = restWorld * localHingeAxis.normalized;
        Vector3 up = Vector3.ProjectOnPlane(restWorld * Vector3.up, axis);
        Vector3 toHands = Vector3.ProjectOnPlane(midpoint - transform.position, axis);

        return Vector3.SignedAngle(up, toHands, axis);
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
