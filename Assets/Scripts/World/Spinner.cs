using UnityEngine;

// מסובב אובייקט סביב ציר, עם האצה והאטה במקום קפיצה למהירות מלאה.
// משמש למאוורר של האוורור ולרוטור של הצנטריפוגה.
public class Spinner : MonoBehaviour
{
    [SerializeField] private Vector3 localAxis = Vector3.up;
    [SerializeField] private float topSpeed = 720f;       // מעלות לשנייה
    [SerializeField] private float rampSeconds = 1.5f;    // זמן ההאצה מאפס למהירות מלאה
    [SerializeField] private bool spinOnStart;

    private float speed;
    private float targetSpeed;

    public bool IsStopped => speed == 0f && targetSpeed == 0f;

    private void Start()
    {
        if (spinOnStart) StartSpinning();
    }

    public void StartSpinning() => targetSpeed = topSpeed;
    public void StopSpinning() => targetSpeed = 0f;

    private void Update()
    {
        float acceleration = topSpeed / Mathf.Max(rampSeconds, 0.01f);
        speed = Mathf.MoveTowards(speed, targetSpeed, acceleration * Time.deltaTime);
        if (speed != 0f) transform.Rotate(localAxis, speed * Time.deltaTime, Space.Self);
    }
}
