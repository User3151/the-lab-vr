using UnityEngine;

// הגוף של המנחה: רחפן שמרחף ליד השחקן, מסתכל עליו, ועף להצביע על נקודות עניין.
// אין לו Collider: הוא לא חוסם את השחקן ולא תפיסה של חפצים.
// במקום Collider, הוא בודק בעצמו שהמקום שהוא עף אליו פנוי.
public class CoachDrone : MonoBehaviour
{
    [SerializeField] private Transform head;            // המצלמה של השחקן

    [Header("Follow")]
    [SerializeField] private float sideOffset = 0.75f;  // ימינה מהשחקן
    [SerializeField] private float heightOffset = 0.12f;
    [SerializeField] private float forwardOffset = 1.45f;
    [SerializeField] private float smoothTime = 0.6f;

    [Header("Point of interest")]
    [SerializeField] private float distanceFromFocus = 0.7f;   // כמה קרוב לנקודה, בצד של השחקן
    [SerializeField] private float heightAboveFocus = 0.3f;

    [Header("Obstacles")]
    [SerializeField] private LayerMask obstacleMask = ~0;      // מה נחשב מכשול
    [SerializeField] private float clearanceRadius = 0.18f;    // חצי הרוחב של הרחפן
    [SerializeField] private float minDistanceFromHead = 0.5f; // גם כשצפוף, הוא לא נכנס לפנים שלך

    [Header("Hover")]
    [SerializeField] private float bobAmplitude = 0.03f;
    [SerializeField] private float bobFrequency = 0.5f;

    [Header("Eye")]
    [SerializeField] private Light eyeLight;
    [SerializeField] private Color calmColor = new(0.3f, 0.9f, 1f);

    private Transform focus;
    private float focusUntil;
    private Vector3 velocity;
    private float flashUntil;

    private void Awake()
    {
        if (head == null && Camera.main != null) head = Camera.main.transform;
    }

    // עף לנקודת עניין לכמה שניות, ואז חוזר לשחקן
    public void FocusOn(Transform target, float seconds)
    {
        if (target == null) return;
        focus = target;
        focusUntil = Time.time + seconds;
    }

    // העין משנה צבע לרגע, למשל אדום באזעקה
    public void FlashEye(Color color, float seconds)
    {
        if (eyeLight == null) return;
        eyeLight.color = color;
        flashUntil = Time.time + seconds;
    }

    private void LateUpdate()
    {
        if (head == null) return;

        bool focusing = focus != null && Time.time < focusUntil;
        Vector3 target = focusing ? FocusPosition() : FollowPosition();
        target.y += Mathf.Sin(Time.time * Mathf.PI * 2f * bobFrequency) * bobAmplitude;

        // המקום המבוקש עלול להיות בתוך קיר. מקצרים אותו עד למקום הפנוי הקרוב ביותר.
        target = PullOutOfObstacles(target);

        transform.position = Vector3.SmoothDamp(transform.position, target, ref velocity, smoothTime);

        // מסתכל על מה שהוא מראה, או על השחקן
        Vector3 lookAt = focusing ? focus.position : head.position;
        Vector3 toLook = lookAt - transform.position;
        if (toLook.sqrMagnitude > 0.0001f)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(toLook), Time.deltaTime * 4f);

        if (eyeLight != null && Time.time >= flashUntil) eyeLight.color = calmColor;
    }

    // לפני השחקן ובצד, לפי הכיוון שהוא מסתכל אליו על המישור (בלי הטיית ראש למעלה/למטה)
    private Vector3 FollowPosition()
    {
        Vector3 forward = Vector3.ProjectOnPlane(head.forward, Vector3.up).normalized;
        if (forward.sqrMagnitude < 0.001f) forward = Vector3.forward;
        Vector3 right = Vector3.Cross(Vector3.up, forward);

        return head.position + right * sideOffset + Vector3.up * heightOffset + forward * forwardOffset;
    }

    // ליד הנקודה, בצד שפונה לשחקן, קצת מעליה: כך השחקן רואה את שניהם יחד
    private Vector3 FocusPosition()
    {
        Vector3 toPlayer = Vector3.ProjectOnPlane(head.position - focus.position, Vector3.up).normalized;
        return focus.position + toPlayer * distanceFromFocus + Vector3.up * heightAboveFocus;
    }

    // הקו מהראש של השחקן אל המקום המבוקש הוא קו ראייה: אם הוא חוסם, אין טעם לעוף לשם.
    // SphereCast ולא Raycast, כדי שגוף הרחפן כולו יישאר בצד הפנוי של הקיר.
    // Trigger מתעלמים ממנו: השקעים בחדר הם Trigger, והם לא מכשול.
    private Vector3 PullOutOfObstacles(Vector3 target)
    {
        Vector3 fromHead = target - head.position;
        float distance = fromHead.magnitude;
        if (distance < 0.001f) return target;

        Vector3 direction = fromHead / distance;

        if (!Physics.SphereCast(head.position, clearanceRadius, direction, out RaycastHit hit,
                distance, obstacleMask, QueryTriggerInteraction.Ignore))
            return target;

        // נעצרים לפני המכשול, אבל לא מתקרבים לשחקן מדי
        float allowed = Mathf.Max(minDistanceFromHead, hit.distance - clearanceRadius);
        return head.position + direction * allowed;
    }
}
