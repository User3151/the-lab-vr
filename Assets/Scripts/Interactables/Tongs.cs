using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

// מלקחיים: מחזיקים אותם ביד, ולוחצים על ההדק כדי לסגור אותם.
// בקצה יש שקע קטן. כשהמלקחיים סגורים הוא פעיל ותופס את מה שבין הלסתות, וכשפותחים הוא משחרר.
// דגימות מסוכנות נמצאות בשכבת אינטראקציה שרק השקע הזה מגיע אליה, ולכן אי אפשר להרים אותן ביד.
[RequireComponent(typeof(XRGrabInteractable))]
public class Tongs : MonoBehaviour
{
    [Header("Parts")]
    [SerializeField] private XRSocketInteractor tip;
    [SerializeField] private Transform armLeft;
    [SerializeField] private Transform armRight;

    [Header("Jaws")]
    [SerializeField] private float openAngle = 9f;     // כמה כל זרוע פתוחה החוצה, במעלות
    [SerializeField] private float closedAngle = 1f;   // סגורות: הרווח בקצה בערך ברוחב של בקבוקון
    [SerializeField] private float jawSpeed = 60f;     // מעלות לשנייה

    private XRGrabInteractable grab;
    private bool closed;
    private float angle;

    private void Awake()
    {
        grab = GetComponent<XRGrabInteractable>();
        angle = openAngle;
        tip.socketActive = false;
    }

    private void OnEnable()
    {
        grab.activated.AddListener(OnSqueeze);
        grab.deactivated.AddListener(OnRelax);
        grab.selectExited.AddListener(OnDropped);
    }

    private void OnDisable()
    {
        grab.activated.RemoveListener(OnSqueeze);
        grab.deactivated.RemoveListener(OnRelax);
        grab.selectExited.RemoveListener(OnDropped);
    }

    private void OnSqueeze(ActivateEventArgs args) => SetClosed(true);
    private void OnRelax(DeactivateEventArgs args) => SetClosed(false);

    // מי שעוזב את המלקחיים עוזב גם את מה שהם מחזיקים
    private void OnDropped(SelectExitEventArgs args) => SetClosed(false);

    // השקע הכבוי משחרר את מה שהוא מחזיק, ו-XRI מפיל אותו למקום: לשקע במעמד, אם הוא מעליו
    private void SetClosed(bool value)
    {
        closed = value;
        tip.socketActive = value;
    }

    // הזרועות רק מראות מה קורה: כל אחת מסתובבת החוצה סביב הציר שלה, בכיוון ההפוך לשנייה
    private void Update()
    {
        float target = closed ? closedAngle : openAngle;
        angle = Mathf.MoveTowards(angle, target, jawSpeed * Time.deltaTime);
        armLeft.localRotation = Quaternion.Euler(0f, -angle, 0f);
        armRight.localRotation = Quaternion.Euler(0f, angle, 0f);
    }
}
