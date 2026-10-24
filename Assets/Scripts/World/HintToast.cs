using UnityEngine;
using UnityEngine.InputSystem;

// הערה קצרה מול העיניים, למשל "לחץ X כדי לפתוח את הלוח".
// מופיעה כשקוראים ל-Show, ונעלמת אחרי כמה שניות או כשהשחקן לוחץ על הכפתור שהיא מזכירה.
// הטקסט עצמו כתוב בסצנה, כך שאותו סקריפט מתאים לכל הערה.
public class HintToast : MonoBehaviour
{
    [SerializeField] private Transform head;          // המצלמה של השחקן
    [SerializeField] private GameObject panel;        // מה שנדלק ונכבה

    [Header("Placement")]
    [SerializeField] private float distance = 0.7f;
    [SerializeField] private float heightOffset = -0.2f;
    [SerializeField] private float smoothTime = 0.15f;

    [Header("Timing")]
    [SerializeField] private float showSeconds = 12f;

    [Header("Dismiss")]
    [SerializeField] private string[] dismissBindings =
    {
        "<XRController>{LeftHand}/primaryButton",
        "<XRController>{RightHand}/primaryButton",
        "<Keyboard>/3",
    };

    private InputAction dismissAction;
    private Vector3 velocity;
    private float hideTime;

    private void Awake()
    {
        if (head == null && Camera.main != null) head = Camera.main.transform;

        dismissAction = new InputAction("DismissHint", InputActionType.Button);
        foreach (string binding in dismissBindings) dismissAction.AddBinding(binding);

        panel.SetActive(false);
    }

    private void OnEnable()
    {
        dismissAction.performed += HandleDismiss;
        dismissAction.Enable();
    }

    private void OnDisable()
    {
        dismissAction.performed -= HandleDismiss;
        dismissAction.Disable();
    }

    public void Show()
    {
        panel.SetActive(true);
        hideTime = Time.time + showSeconds;
        transform.position = TargetPosition();   // מופיע ישר במקום, בלי לעוף אליו מרחוק
        velocity = Vector3.zero;
    }

    private void HandleDismiss(InputAction.CallbackContext context) => panel.SetActive(false);

    private void LateUpdate()
    {
        if (!panel.activeSelf) return;

        if (Time.time >= hideTime)
        {
            panel.SetActive(false);
            return;
        }

        transform.position = Vector3.SmoothDamp(transform.position, TargetPosition(), ref velocity, smoothTime);
        transform.rotation = Quaternion.LookRotation(transform.position - head.position);
    }

    // לפני השחקן לפי הכיוון שהוא מסתכל אליו על המישור, קצת מתחת לעיניים
    private Vector3 TargetPosition()
    {
        Vector3 forward = Vector3.ProjectOnPlane(head.forward, Vector3.up).normalized;
        if (forward.sqrMagnitude < 0.001f) forward = Vector3.forward;
        return head.position + forward * distance + Vector3.up * heightOffset;
    }
}
