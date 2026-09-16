using System.Collections.Generic;
using UnityEngine;

// פולט לייזר: יורה קרן קדימה, מחזיר אותה ממראות, ומצייר את המסלול.
// הקרן מחושבת מחדש בכל פריים, כך שכל סיבוב של מראה משנה אותה מיד.
[RequireComponent(typeof(LineRenderer))]
public class LaserEmitter : MonoBehaviour
{
    [SerializeField] private float maxSegmentLength = 15f;
    [SerializeField] private int maxBounces = 4;

    private LineRenderer line;
    private readonly List<Vector3> points = new();

    private void Awake() => line = GetComponent<LineRenderer>();

    // הקרן נראית רק כשהפולט פעיל
    private void OnEnable()
    {
        if (line == null) line = GetComponent<LineRenderer>();
        line.enabled = true;
    }

    private void OnDisable()
    {
        if (line != null) line.enabled = false;
    }

    private void Update()
    {
        LaserReceiver receiver = Trace(points);
        if (receiver != null) receiver.Receive(LastDirection(points));

        line.positionCount = points.Count;
        for (int i = 0; i < points.Count; i++) line.SetPosition(i, points[i]);
    }

    // מחשב את מסלול הקרן לתוך הרשימה ומחזיר את החיישן שבו היא נעצרה, אם יש כזה.
    // אותה פונקציה משמשת גם את המשחק וגם את התצוגה בעורך, כך שמה שרואים בעריכה הוא מה שיקרה ב-Play.
    private LaserReceiver Trace(List<Vector3> path)
    {
        path.Clear();

        Vector3 origin = transform.position;
        Vector3 direction = transform.forward;
        path.Add(origin);

        for (int bounce = 0; bounce <= maxBounces; bounce++)
        {
            if (!Physics.Raycast(origin, direction, out RaycastHit hit, maxSegmentLength, ~0, QueryTriggerInteraction.Ignore))
            {
                path.Add(origin + direction * maxSegmentLength);   // שום דבר בדרך
                return null;
            }

            path.Add(hit.point);

            if (hit.collider.TryGetComponent(out LaserReceiver receiver)) return receiver;

            // קיר, רהיט או השחקן עצמו בולעים את הקרן. רק מראה ממשיכה אותה.
            if (!hit.collider.TryGetComponent(out LaserMirror _)) return null;

            // זווית פגיעה שווה לזווית החזרה, סביב הנורמל של המשטח
            direction = Vector3.Reflect(direction, hit.normal);

            // מתרחקים מילימטר מהמשטח, כדי שהקרן הבאה לא תפגע שוב באותה נקודה
            origin = hit.point + direction * 0.001f;
        }

        return null;
    }

    private static Vector3 LastDirection(List<Vector3> path) =>
        (path[path.Count - 1] - path[path.Count - 2]).normalized;

#if UNITY_EDITOR
    private readonly List<Vector3> gizmoPoints = new();

    // תצוגה בחלון Scene גם בלי Play: עוזרת לסדר מחדש את המעבדה ולראות מיד אם הקרן עדיין מגיעה לחיישן.
    // שים לב: מכסה סגור בעריכה חוסם את הקרן, כמו במשחק לפני שהוא נפתח.
    private void OnDrawGizmosSelected()
    {
        if (Application.isPlaying) return;

        Physics.SyncTransforms();
        LaserReceiver receiver = Trace(gizmoPoints);

        Gizmos.color = receiver != null ? Color.green : Color.red;
        for (int i = 1; i < gizmoPoints.Count; i++) Gizmos.DrawLine(gizmoPoints[i - 1], gizmoPoints[i]);
    }
#endif
}
