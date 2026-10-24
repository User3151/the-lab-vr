using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

// גלגל שמסתובב סביב ציר אחד, בין אפס לזווית מקסימלית, ומפרסם ערך רציף בין 0 ל-1.
// אפשר לסובב אותו יותר מסיבוב שלם: הרבה סיבוב עבור שינוי קטן, וכך השליטה עדינה.
public class ConstrainedDial : XRBaseInteractable
{
    [Header("Axis")]
    [SerializeField] private Vector3 localAxis = Vector3.forward;   // הציר שסביבו הגלגל מסתובב
    [SerializeField] private float maxAngle = 540f;                 // סיבוב וחצי מסגור לפתוח

    [Header("Grip")]
    [SerializeField] private float deadZoneRadius = 0.03f;          // יד קרובה מדי למרכז לא נספרת

    [Header("Detents")]
    [SerializeField] private float snapAngle = 0f;   // 0 = סיבוב חלק. אחרת הגלגל ננעל בקפיצות של הזווית הזו

    // הערך הרציף. מי שצריך אותו קורא אותו, אין אירוע.
    public float Value => SnappedAngle / maxAngle;

    // היד זזה חלק, אבל מה שמוצג ומדווח קופץ לנקודת העצירה הקרובה, כמו מתקן אופטי עם קליקים
    private float SnappedAngle => snapAngle > 0f
        ? Mathf.Clamp(Mathf.Round(currentAngle / snapAngle) * snapAngle, 0f, maxAngle)
        : currentAngle;

    private Quaternion restLocalRotation;
    private float currentAngle;
    private float lastHandAngle;
    private bool hasLastHandAngle;

    protected override void Awake()
    {
        base.Awake();
        restLocalRotation = transform.localRotation;
    }

    // תפיסה חדשה: שוכחים את הזווית הקודמת, כדי שהגלגל לא יקפוץ
    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);
        hasLastHandAngle = false;
    }

    public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
    {
        if (updatePhase != XRInteractionUpdateOrder.UpdatePhase.Dynamic) return;
        if (!isSelected) return;

        if (!TryGetHandAngle(interactorsSelecting[0].transform.position, out float handAngle))
        {
            hasLastHandAngle = false;
            return;
        }

        // מצטברים רק את השינוי מהפריים הקודם.
        // DeltaAngle מטפל במעבר מ-179 ל-181-, ולכן אפשר לסובב יותר מסיבוב שלם.
        if (hasLastHandAngle)
        {
            float delta = Mathf.DeltaAngle(lastHandAngle, handAngle);
            currentAngle = Mathf.Clamp(currentAngle + delta, 0f, maxAngle);
        }

        lastHandAngle = handAngle;
        hasLastHandAngle = true;

        transform.localRotation = restLocalRotation * Quaternion.AngleAxis(SnappedAngle, localAxis);
    }

    // קופץ מיד לערך (0 עד 1), למשל מכסה שננעל כשהוא כמעט סגור.
    // ProcessInteractable מסובב רק כשמחזיקים, לכן מסובבים כאן בעצמנו.
    public void SnapTo(float value)
    {
        currentAngle = Mathf.Clamp01(value) * maxAngle;
        hasLastHandAngle = false;
        transform.localRotation = restLocalRotation * Quaternion.AngleAxis(SnappedAngle, localAxis);
    }

    // הזווית של היד סביב הציר, ביחס לכיוון קבוע.
    // מחזיר false כשהיד כמעט על הציר, כי שם כל רעד קטן הוא קפיצה גדולה בזווית.
    private bool TryGetHandAngle(Vector3 handPosition, out float angle)
    {
        Quaternion restWorld = transform.parent != null
            ? transform.parent.rotation * restLocalRotation
            : restLocalRotation;

        Vector3 axis = restWorld * localAxis.normalized;

        // כיוון הייחוס חייב להיות לא מקביל לציר
        Vector3 reference = Mathf.Abs(localAxis.normalized.y) < 0.9f ? Vector3.up : Vector3.forward;
        reference = Vector3.ProjectOnPlane(restWorld * reference, axis);

        Vector3 toHand = Vector3.ProjectOnPlane(handPosition - transform.position, axis);
        angle = Vector3.SignedAngle(reference, toHand, axis);

        return toHand.magnitude >= deadZoneRadius;
    }
}
