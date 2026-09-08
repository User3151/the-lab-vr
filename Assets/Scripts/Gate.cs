using UnityEngine;

// שער האור. אזור טריגר שמחכה שכוכב ייכנס אליו, ואז קופץ רחוק יותר.
public class Gate : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform bodyReference;

    [Header("Placement")]
    [SerializeField] private float startDistance = 2f;    // מרחק בפגיעה הראשונה
    [SerializeField] private float distanceStep = 0.35f;  // כמה מתרחק בכל פגיעה
    [SerializeField] private float maxDistance = 5f;
    [SerializeField] private float minHeight = 1.2f;
    [SerializeField] private float maxHeight = 2f;
    [SerializeField] private float sideAngle = 35f;

    private int hits = 0;

    public static event System.Action<Vector3> OnGateHit;

    private void Start()
    {
        MoveToNewPosition();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Star")) return;

        OnGateHit?.Invoke(transform.position);
        Debug.Log("Gate hit!");

        hits++;
        MoveToNewPosition();
    }

    private void MoveToNewPosition()
    {
        float distance = Mathf.Min(startDistance + distanceStep * hits, maxDistance);
        float height = Random.Range(minHeight, maxHeight);
        float angle = Random.Range(-sideAngle, sideAngle);

        Vector3 horizontal = Quaternion.Euler(0f, angle, 0f) * bodyReference.forward;
        horizontal.y = 0f;
        horizontal = horizontal.normalized;

        // גובה מוחלט מהרצפה, ולכן בונים את המיקום מחדש ולא מוסיפים לגובה הראש
        Vector3 flat = bodyReference.position + horizontal * distance;
        transform.position = new Vector3(flat.x, height, flat.z);
    }
}