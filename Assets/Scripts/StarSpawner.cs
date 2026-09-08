using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

// יושב על הכוכב ומנהל את מחזור החיים שלו:
// מרחף -> נתפס -> נזרק -> נפתר (פגע או נפל) -> מרחף מחדש.
public class StarSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform bodyReference;   // המצלמה (הראש)

    [Header("Spawn envelope")]
    [SerializeField] private float armLength = 0.65f;
    [SerializeField] private float minReachRatio = 0.55f;   // הכי קרוב לגוף
    [SerializeField] private float maxReachRatio = 0.95f;   // הכי רחוק
    [SerializeField] private float minHeightBias = -0.45f;  // הכי נמוך
    [SerializeField] private float maxHeightBias = 0.25f;   // הכי גבוה
    [SerializeField] private float sideAngle = 45f;         // רוחב הקשת לצדדים

    [Header("Flight")]
    [SerializeField] private float missHeight = 0.2f;    // מתחת לגובה הזה - נפל
    [SerializeField] private float maxFlightTime = 4f;   // אחרי כך שניות מוותרים
    [SerializeField] private float respawnDelay = 0.5f;

    [Header("Look")]
    [SerializeField] private Color starColor = new Color(0.4f, 0.8f, 1f);
    [SerializeField] private float emissionStrength = 3f;

    private Rigidbody rb;
    private Renderer starRenderer;
    private XRGrabInteractable grab;

    private bool isFlying = false;     // באוויר אחרי שחרור
    private bool isResolved = false;   // כבר נספר - מונע ספירה כפולה
    private float throwTime;

    public static event System.Action<ReachEvent> OnStarGrabbed;
    public static event System.Action OnStarMissed;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        starRenderer = GetComponent<Renderer>();
        grab = GetComponent<XRGrabInteractable>();
    }

    private void OnEnable()
    {
        // אלה UnityEvent של XRI - נרשמים עם AddListener ולא עם +=
        grab.selectEntered.AddListener(HandleGrabbed);
        grab.selectExited.AddListener(HandleReleased);
        Gate.OnGateHit += HandleGateHit;
    }

    private void OnDisable()
    {
        grab.selectEntered.RemoveListener(HandleGrabbed);
        grab.selectExited.RemoveListener(HandleReleased);
        Gate.OnGateHit -= HandleGateHit;
    }

    private void Start()
    {
        Respawn();
    }

    private void Update()
    {
        if (!isFlying || isResolved) return;

        bool fellDown = transform.position.y < missHeight;
        bool tookTooLong = Time.time - throwTime > maxFlightTime;

        if (fellDown || tookTooLong)
        {
            isResolved = true;
            OnStarMissed?.Invoke();
            Debug.Log("Star missed");
            StartCoroutine(RespawnAfterDelay());
        }
    }

    // ===== רגע התפיסה: כאן נמדדת ההושטה =====
    private void HandleGrabbed(SelectEnterEventArgs args)
    {
        ReachEvent e = new ReachEvent
        {
            round = 0,
            reachRatio = 0f,
            reachDistance = Vector3.Distance(bodyReference.position, transform.position),
            handPosition = args.interactorObject.transform.position,
            targetPosition = transform.position,
            time = Time.time
        };

        OnStarGrabbed?.Invoke(e);
        Debug.Log($"Star grabbed | Reach: {e.reachDistance:F2} m");
    }

    // ===== רגע השחרור: מכאן הוא באוויר =====
    private void HandleReleased(SelectExitEventArgs args)
    {
        isFlying = true;
        throwTime = Time.time;
    }

    private void HandleGateHit(Vector3 position)
    {
        if (isResolved) return;
        isResolved = true;
        StartCoroutine(RespawnAfterDelay());
    }

    private IEnumerator RespawnAfterDelay()
    {
        yield return new WaitForSeconds(respawnDelay);
        Respawn();
    }

    private void Respawn()
    {
        // 1. נקודה אקראית בתוך מעטפת ההושטה
        float reachRatio = Random.Range(minReachRatio, maxReachRatio);
        float heightBias = Random.Range(minHeightBias, maxHeightBias);
        float angle = Random.Range(-sideAngle, sideAngle);

        Vector3 horizontal = Quaternion.Euler(0f, angle, 0f) * bodyReference.forward;
        horizontal.y = 0f;
        horizontal = horizontal.normalized;

        Vector3 direction = (horizontal + Vector3.up * heightBias).normalized;
        transform.position = bodyReference.position + direction * (reachRatio * armLength);

        // 2. לא קינמטי - כדי שאפשר יהיה לזרוק אותו.
        //    בלי כוח משיכה - ולכן הוא מרחף במקום ולא נופל.
        rb.isKinematic = false;
        rb.useGravity = false;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        isFlying = false;
        isResolved = false;

        starRenderer.material.color = starColor;
        starRenderer.material.SetColor("_EmissionColor", starColor * emissionStrength);
    }
}