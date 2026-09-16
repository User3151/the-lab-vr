using UnityEngine;

// חיישן אופטי: נדלק רק מקרן שמגיעה כמעט ישר אל הפנים שלו, כמו חיישן בתוך צינור.
// כך אי אפשר "לרמות" עם קרן אלכסונית ממראה אחת. צריך ליישר את כל המסלול.
public class LaserReceiver : MonoBehaviour
{
    [SerializeField] private float acceptAngle = 15f;   // מעלות מהכיוון הקדמי של החיישן

    private float lastLitTime = float.NegativeInfinity;

    // "מואר" = קיבל קרן בפריים האחרון או בערך. לא תלוי בסדר שבו Unity מריץ את הסקריפטים.
    public bool IsLit => Time.time - lastLitTime < 0.1f;

    public void Receive(Vector3 beamDirection)
    {
        // הקרן צריכה לנוע לתוך החיישן, כלומר נגד הכיוון שהוא פונה אליו
        if (Vector3.Angle(-beamDirection, transform.forward) > acceptAngle) return;
        lastLitTime = Time.time;
    }
}
