using UnityEngine;

// מצלמה לסרטון: מה שמופיע במסך המחשב, לא בקסדה.
// עין אחת של הקסדה רואה פחות לצד של האף, ולכן במראה הרגילה הצדדים נחתכים.
// המצלמה הזו רחבה יותר, עוקבת אחרי הראש ברכות ומחליקה את הרעידות הקטנות, כך שנעים לצפות בסרטון.
[RequireComponent(typeof(Camera))]
public class SpectatorCamera : MonoBehaviour
{
    [SerializeField] private Transform head;                  // המצלמה של הקסדה
    [SerializeField] private float horizontalFov = 105f;      // רוחב שדה הראייה בסרטון, במעלות
    [SerializeField] private float positionSmoothTime = 0.08f;
    [SerializeField] private float rotationSharpness = 8f;    // כמה מהר הסיבוב משיג את הראש. גבוה = פחות החלקה
    [SerializeField] private bool levelHorizon = true;        // הטיה של הראש לצד לא מטה את כל הסרטון
    [SerializeField] private float snapDistance = 1f;         // קפיצה גדולה מזה (מעבר בין חדרים) לא מוחלקת

    private Camera cam;
    private Vector3 velocity;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        cam.stereoTargetEye = StereoTargetEyeMask.None;   // מצייר רק למסך המחשב, לא לקסדה
        if (head == null && Camera.main != null) head = Camera.main.transform;
    }

    private void OnEnable() => Snap();

    // LateUpdate: אחרי שהקסדה כבר עדכנה את מיקום הראש בפריים הזה
    private void LateUpdate()
    {
        if (head == null) return;

        // Unity מגדיר זווית ראייה אנכית. מחשבים אותה מהרוחב שרוצים ומהיחס של המסך.
        cam.fieldOfView = Camera.HorizontalToVerticalFieldOfView(horizontalFov, cam.aspect);

        // GameFlow מעביר את השחקן מחדר התפריט למעבדה: שם קופצים, לא מחליקים 40 מטר
        if ((head.position - transform.position).sqrMagnitude > snapDistance * snapDistance)
        {
            Snap();
            return;
        }

        transform.position = Vector3.SmoothDamp(transform.position, head.position, ref velocity, positionSmoothTime);

        // 1 פחות Exp: אותה החלקה בכל קצב פריימים, 60 או 120
        float t = 1f - Mathf.Exp(-rotationSharpness * Time.deltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, TargetRotation(), t);
    }

    private void Snap()
    {
        if (head == null) return;
        transform.SetPositionAndRotation(head.position, TargetRotation());
        velocity = Vector3.zero;
    }

    // בלי ההטיה לצדדים (roll): אותו כיוון מבט, אבל "למעלה" תמיד למעלה
    private Quaternion TargetRotation()
    {
        if (!levelHorizon) return head.rotation;

        Vector3 forward = head.forward;
        if (Mathf.Abs(Vector3.Dot(forward, Vector3.up)) > 0.99f) return head.rotation;   // מבט ישר למעלה או למטה
        return Quaternion.LookRotation(forward, Vector3.up);
    }
}
