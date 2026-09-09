using System.Collections.Generic;
using Mediapipe.Tasks.Components.Containers;
using Mediapipe.Tasks.Vision.HandLandmarker;
using Mediapipe.Unity.Sample.HandLandmarkDetection;
using UnityEngine;

// מקור קלט יד המבוסס על מצלמת רשת.
// מתרגם את פלט MediaPipe לשלוש התשובות של החוזה.
public class WebcamHandInput : MonoBehaviour, IHandInput
{
    [Header("Reference")]
    [SerializeField] private Transform origin;   // המצלמה. היד ממוקמת ביחס אליה

    [Header("Movement plane")]
    [SerializeField] private float planeDistance = 0.6f;   // מרחק המישור מהמצלמה
    [SerializeField] private float planeWidth = 1.2f;      // רוחב שטח התנועה
    [SerializeField] private float planeHeight = 0.9f;     // גובה שטח התנועה
    [SerializeField] private bool mirrorX = false;          // תמונת מראה

    [Header("Pinch")]
    [SerializeField] private float pinchDistance = 0.035f;
    [SerializeField] private float releaseDistance = 0.05f;

    [Header("Smoothing")]
    [SerializeField] private float smoothing = 14f;   // גבוה = מגיב מהר אבל רועד יותר

    // ===== החוזה =====
    public bool IsTracked { get; private set; }
    public Vector3 Position { get; private set; }
    public bool IsGrabbing { get; private set; }

    private const int Wrist = 0, IndexBase = 5, PinkyBase = 17;
    private const int ThumbTip = 4, IndexTip = 8;

    // נכתבים מהחוט של התוסף, נקראים ב-Update
    private volatile bool hasFrame = false;
    private float palmU, palmV;   // מיקום כף היד בתמונה, בין 0 ל-1
    private float pinchGap;       // מרחק אגודל-מורה במטרים

    private bool initialised = false;

    private void OnEnable() { HandLandmarkerRunner.OnResult += HandleResult; }
    private void OnDisable() { HandLandmarkerRunner.OnResult -= HandleResult; }

    // התוסף קורא לזה, אולי מחוט עיבוד אחר.
    // כאן רק מעתיקים מספרים - אסור לגעת בשום דבר של Unity.
    private void HandleResult(HandLandmarkerResult result)
    {
        List<NormalizedLandmarks> screenHands = result.handLandmarks;
        List<Landmarks> worldHands = result.handWorldLandmarks;

        if (screenHands == null || screenHands.Count == 0 ||
            worldHands == null || worldHands.Count == 0)
        {
            hasFrame = false;
            return;
        }

        List<NormalizedLandmark> screen = screenHands[0].landmarks;
        List<Landmark> world = worldHands[0].landmarks;

        if (screen == null || screen.Count <= PinkyBase ||
            world == null || world.Count <= IndexTip)
        {
            hasFrame = false;
            return;
        }

        // מרכז כף היד: ממוצע של שורש כף היד ובסיסי המורה והזרת.
        // יציב הרבה יותר מנקודה בודדת, שזזה עם כל כיפוף אצבע.
        palmU = (screen[Wrist].x + screen[IndexBase].x + screen[PinkyBase].x) / 3f;
        palmV = (screen[Wrist].y + screen[IndexBase].y + screen[PinkyBase].y) / 3f;

        // הצביטה נמדדת בקואורדינטות עולם, כדי שהסף לא ישתנה עם המרחק מהמצלמה
        float dx = world[ThumbTip].x - world[IndexTip].x;
        float dy = world[ThumbTip].y - world[IndexTip].y;
        float dz = world[ThumbTip].z - world[IndexTip].z;
        pinchGap = Mathf.Sqrt(dx * dx + dy * dy + dz * dz);

        hasFrame = true;
    }

    private void Update()
    {
        if (origin == null)
        {
            Debug.LogWarning("WebcamHandInput: origin is not assigned in the Inspector!");
            return;
        }

        IsTracked = hasFrame;
        if (!hasFrame) return;

        // ===== מיקום =====
        float u = mirrorX ? 1f - palmU : palmU;

        // ציר Y בתמונה יורד מלמעלה למטה, וב-Unity הוא עולה. לכן הקצוות הפוכים.
        float x = Mathf.Lerp(-planeWidth * 0.5f, planeWidth * 0.5f, u);
        float y = Mathf.Lerp(planeHeight * 0.5f, -planeHeight * 0.5f, palmV);

        Vector3 target = origin.TransformPoint(new Vector3(x, y, planeDistance));

        if (!initialised)
        {
            Position = target;      // בפריים הראשון קופצים ישר, בלי החלקה מאפס
            initialised = true;
        }
        else
        {
            // החלקה שלא תלויה בקצב הפריימים: אותה תחושה ב-30 וב-60 פריימים
            float t = 1f - Mathf.Exp(-smoothing * Time.deltaTime);
            Position = Vector3.Lerp(Position, target, t);
        }

        // ===== צביטה =====
        if (!IsGrabbing && pinchGap < pinchDistance) IsGrabbing = true;
        else if (IsGrabbing && pinchGap > releaseDistance) IsGrabbing = false;
    }
}