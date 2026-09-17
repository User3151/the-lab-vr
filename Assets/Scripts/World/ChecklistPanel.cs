using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

// לוח המשימות הנייד: נדלק בלחיצת כפתור, מופיע מול הפנים, ומציג את מה שהמסוף מפרסם.
// הוא לא מחשב כלום בעצמו, כדי שלא יהיו שתי גרסאות של אותה רשימה.
public class ChecklistPanel : MonoBehaviour
{
    [SerializeField] private Transform head;            // המצלמה של השחקן
    [SerializeField] private GameObject panel;          // מה שנדלק ונכבה
    [SerializeField] private TMP_Text text;
    [SerializeField] private LockdownTerminal terminal;

    [Header("Placement")]
    [SerializeField] private float distance = 0.55f;    // מטרים מהעיניים
    [SerializeField] private float heightOffset = -0.12f;
    [SerializeField] private float smoothTime = 0.08f;  // כמה "רך" הלוח עוקב אחרי הראש

    private InputAction toggleAction;
    private Vector3 velocity;
    private bool visible;

    private void Awake()
    {
        if (head == null && Camera.main != null) head = Camera.main.transform;

        // בונים את הפעולה בקוד: בקר שמאלי, בקר ימני, ומקש במקלדת בשביל הסימולטור
        toggleAction = new InputAction("ToggleChecklist", InputActionType.Button);
        toggleAction.AddBinding("<XRController>{LeftHand}/primaryButton");
        toggleAction.AddBinding("<XRController>{RightHand}/primaryButton");
        toggleAction.AddBinding("<Keyboard>/tab");

        panel.SetActive(false);
    }

    private void OnEnable()
    {
        toggleAction.performed += HandleToggle;
        toggleAction.Enable();

        terminal.ReportChanged += HandleReport;
        HandleReport(terminal.Report);
    }

    private void OnDisable()
    {
        toggleAction.performed -= HandleToggle;
        toggleAction.Disable();

        terminal.ReportChanged -= HandleReport;
    }

    private void HandleToggle(InputAction.CallbackContext context) => Show(!visible);

    private void Show(bool on)
    {
        visible = on;
        panel.SetActive(on);

        // כשהוא נדלק הוא מופיע מיד במקום הנכון, בלי שייעוף אל הפנים מהמקום הקודם
        if (!on) return;
        transform.position = TargetPosition();
        transform.rotation = Quaternion.LookRotation(transform.position - head.position);
        velocity = Vector3.zero;
    }

    private void HandleReport(string report)
    {
        text.text = string.IsNullOrEmpty(report)
            ? "<color=#8A8A8A>NO SIGNAL</color>"
            : report;
    }

    // עוקב רך אחרי הראש: לא נעול אליו (זה מעייף בקסדה) ולא נשאר תקוע באוויר
    private void LateUpdate()
    {
        if (!visible || head == null) return;

        transform.position = Vector3.SmoothDamp(transform.position, TargetPosition(), ref velocity, smoothTime);
        transform.rotation = Quaternion.LookRotation(transform.position - head.position);
    }

    // במרחב של הראש: קדימה ומעט למטה, כך שהלוח תמיד במרכז שדה הראייה
    private Vector3 TargetPosition()
    {
        return head.position + head.rotation * new Vector3(0f, heightOffset, distance);
    }
}
