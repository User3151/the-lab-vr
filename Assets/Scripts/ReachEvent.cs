using UnityEngine;

// חבילת המידע על נגיעה אחת. לא MonoBehaviour.
// כל מאזין מקבל את כל החבילה ולוקח ממנה מה שהוא צריך.
public struct ReachEvent
{
    public int round;              // מספר הסבב, מתחיל מ-0
    public float reachRatio;       // הקושי: באיזה אחוז מאורך הזרוע ישבה המטרה
    public float reachDistance;    // המדידה: מרחק מהגוף לכף היד ברגע המגע, במטרים
    public Vector3 handPosition;   // איפה הייתה היד
    public Vector3 targetPosition; // איפה הייתה המטרה
    public float time;             // שניות מתחילת הריצה ברגע המגע
}