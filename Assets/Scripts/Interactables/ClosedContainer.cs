using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

// קופסה, מקרר, מגירה או מכסה: כל עוד הם סגורים, אי אפשר לתפוס את מה שבפנים.
// נרשם כפילטר בחירה בכל החפצים שאפשר לתפוס, וחוסם בחירה של חפץ שנמצא בתוך נפח הפנים.
// שקעים תמיד מורשים, כי הם מחזיקים את החפצים בפנים. רק ידיים ומלקחיים נחסמים.
public class ClosedContainer : MonoBehaviour, IXRSelectFilter
{
    private enum Measure { Angle, Distance }

    [Header("Door")]
    [SerializeField] private Transform door;                    // החלק שזז: דלת, מכסה, מגירה
    [SerializeField] private Measure measure = Measure.Angle;   // מעלות לדלתות, מטרים למגירות
    [SerializeField] private float openAt = 20f;               // מכאן ומעלה המכל פתוח

    [Header("Inside (local space of this object)")]
    [SerializeField] private Vector3 insideCenter;
    [SerializeField] private Vector3 insideSize = new(0.3f, 0.3f, 0.3f);

    private Quaternion restRotation;
    private Vector3 restPosition;

    public bool canProcess => isActiveAndEnabled;

    private bool IsOpen
    {
        get
        {
            float amount = measure == Measure.Angle
                ? Quaternion.Angle(restRotation, door.localRotation)
                : Vector3.Distance(restPosition, door.localPosition);
            return amount >= openAt;
        }
    }

    private void Awake()
    {
        restRotation = door.localRotation;
        restPosition = door.localPosition;
    }

    private void Start()
    {
        foreach (XRGrabInteractable grab in FindObjectsByType<XRGrabInteractable>(FindObjectsSortMode.None))
            grab.selectFilters.Add(this);
    }

    private void OnDestroy()
    {
        foreach (XRGrabInteractable grab in FindObjectsByType<XRGrabInteractable>(FindObjectsSortMode.None))
            grab.selectFilters.Remove(this);
    }

    public bool Process(IXRSelectInteractor interactor, IXRSelectInteractable interactable)
    {
        if (interactor is ValidatedSocket) return true;
        if (IsOpen) return true;
        return !IsInside(interactable.transform.position);
    }

    // הבדיקה במרחב המקומי, כך שהתיבה מסתובבת וזזה יחד עם המכל
    private bool IsInside(Vector3 worldPoint)
    {
        Vector3 local = transform.InverseTransformPoint(worldPoint) - insideCenter;
        Vector3 half = insideSize * 0.5f;
        return Mathf.Abs(local.x) <= half.x && Mathf.Abs(local.y) <= half.y && Mathf.Abs(local.z) <= half.z;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(insideCenter, insideSize);
    }
}
