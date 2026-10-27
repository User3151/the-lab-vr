using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

// אובייקט שנשלף לאורך ציר אחד בלבד, בין שני גבולות.
// יורש מ-XRBaseInteractable: XRI נותן את מחזור החיים, התנועה היא שלנו.
// אפשר למשוך אותו ביד, או להזיז אותו מקוד דרך SnapTo (למשל PressToOpen, שפותח בלחיצת כפתור).
[RequireComponent(typeof(Rigidbody))]
public class ConstrainedSlider : XRBaseInteractable
{
    [Header("Axis")]
    [SerializeField] private Vector3 localAxis = Vector3.forward;  // כיוון השליפה, במרחב המקומי
    [SerializeField] private float minOffset = 0f;                 // סגור
    [SerializeField] private float maxOffset = 0.5f;               // פתוח לגמרי

    [Header("Open")]
    [SerializeField] private float openAtPercent = 0.9f;   // מאיזה אחוז מהמהלך נחשב פתוח

    // מכריז כשנפתח. הפאזל יאזין לזה ולא יצטרך להכיר את המגירה.
    public static event System.Action<ConstrainedSlider> OnOpened;

    // כמה המגירה פתוחה, מ-0 (סגורה) עד 1 (פתוחה לגמרי). אותו רעיון כמו Value של ConstrainedDial.
    public float Value => maxOffset > minOffset ? (currentOffset - minOffset) / (maxOffset - minOffset) : 0f;

    private Vector3 startLocalPosition;
    private float currentOffset;
    private float appliedOffset;      // המיקום האחרון שנשלח לפיזיקה
    private float grabDelta;          // ההפרש בין היד למגירה ברגע התפיסה
    private bool reportedOpen;
    private Rigidbody body;

    // חובה לקרוא ל-base. XRI נרשם למנהל האינטראקציה שם.
    protected override void Awake()
    {
        base.Awake();
        startLocalPosition = transform.localPosition;

        // קינמטי: אנחנו מזיזים אותו, לא כוח המשיכה.
        // אבל דרך MovePosition, כך שהפיזיקה יודעת שהוא זז ודוחפת מה שעליו.
        body = GetComponent<Rigidbody>();
        body.isKinematic = true;
        body.interpolation = RigidbodyInterpolation.Interpolate;
    }

    // רגע התפיסה: זוכרים את ההיסט כדי שהמגירה לא תקפוץ אל היד
    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);
        grabDelta = currentOffset - ProjectOntoAxis(args.interactorObject.transform.position);
    }

    // מזיז את המגירה לערך (0 עד 1) בלי יד.
    // לא מזיזים כאן: התנועה קורית בצעד הפיזיקה הבא, בדיוק כמו כשמושכים ביד.
    public void SnapTo(float value)
    {
        currentOffset = Mathf.Lerp(minOffset, maxOffset, Mathf.Clamp01(value));
    }

    // XRI קורא לזה בכל פריים, גם כשאף אחד לא מחזיק, במקום Update.
    public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
    {
        // Fixed ולא Dynamic: תנועה פיזיקלית חייבת לקרות בקצב של מנוע הפיזיקה
        if (updatePhase != XRInteractionUpdateOrder.UpdatePhase.Fixed) return;

        // יד מחזיקה: היא קובעת את המיקום. בלי יד: נשאר מה ש-SnapTo קבע.
        if (isSelected)
        {
            float handOnAxis = ProjectOntoAxis(interactorsSelecting[0].transform.position);
            currentOffset = Mathf.Clamp(handOnAxis + grabDelta, minOffset, maxOffset);
        }

        // שום דבר לא השתנה: לא שולחים לפיזיקה שוב את אותו מיקום
        if (Mathf.Approximately(currentOffset, appliedOffset)) return;
        appliedOffset = currentOffset;

        // MovePosition מקבל מיקום עולמי, ולכן ממירים מהמקומי
        Vector3 targetLocal = startLocalPosition + localAxis.normalized * currentOffset;
        Vector3 targetWorld = transform.parent != null
            ? transform.parent.TransformPoint(targetLocal)
            : targetLocal;

        body.MovePosition(targetWorld);

        CheckOpen();
    }

    // הלב של האילוץ: כמה רחוק נקודה נמצאת לאורך הציר שלנו, ותו לא.
    // כל תנועה בכיוונים האחרים פשוט לא נספרת.
    private float ProjectOntoAxis(Vector3 worldPoint)
    {
        Transform parent = transform.parent;

        Vector3 origin = parent != null
            ? parent.TransformPoint(startLocalPosition)
            : startLocalPosition;

        Vector3 axis = parent != null
            ? parent.TransformDirection(localAxis.normalized)
            : localAxis.normalized;

        return Vector3.Dot(worldPoint - origin, axis);
    }

    private void CheckOpen()
    {
        if (reportedOpen) return;

        float travel = maxOffset - minOffset;
        if (travel <= 0f) return;

        if ((currentOffset - minOffset) / travel >= openAtPercent)
        {
            reportedOpen = true;
            OnOpened?.Invoke(this);
            Debug.Log($"{name} is fully open");
        }
    }
}
