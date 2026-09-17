using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

// מטף: מחזיקים אותו ביד ולוחצים על ההדק (Activate ב-XRI) כדי לרסס.
// הריסוס הוא חרוט של בדיקת פגיעה מהפייה קדימה. כל Fire שנמצא בו נחלש.
[RequireComponent(typeof(XRGrabInteractable))]
public class FireExtinguisher : MonoBehaviour
{
    [Header("Spray")]
    [SerializeField] private Transform nozzle;             // הכיוון הקדמי שלו הוא כיוון הריסוס
    [SerializeField] private ParticleSystem sprayEffect;
    [SerializeField] private float range = 2.5f;
    [SerializeField] private float radius = 0.3f;          // רדיוס הכדור שנשלח לאורך הריסוס

    [Header("Haptics")]
    [SerializeField] private float hapticAmplitude = 0.25f;
    [SerializeField] private float hapticInterval = 0.1f;

    private XRGrabInteractable grab;
    private XRBaseInputInteractor sprayingHand;
    private float nextHapticTime;
    private readonly RaycastHit[] hits = new RaycastHit[8];

    private void Awake() => grab = GetComponent<XRGrabInteractable>();

    private void OnEnable()
    {
        grab.activated.AddListener(HandleActivated);
        grab.deactivated.AddListener(HandleDeactivated);
        grab.selectExited.AddListener(HandleReleased);
    }

    private void OnDisable()
    {
        grab.activated.RemoveListener(HandleActivated);
        grab.deactivated.RemoveListener(HandleDeactivated);
        grab.selectExited.RemoveListener(HandleReleased);
        StopSpray();
    }

    private void HandleActivated(ActivateEventArgs args)
    {
        // היד שלוחצת היא זו שתקבל רטט
        sprayingHand = args.interactorObject as XRBaseInputInteractor;
        sprayEffect.Play();
    }

    private void HandleDeactivated(DeactivateEventArgs args) => StopSpray();

    // אם מניחים את המטף כשההדק עוד לחוץ, הריסוס לא ממשיך לבד
    private void HandleReleased(SelectExitEventArgs args) => StopSpray();

    private void StopSpray()
    {
        sprayingHand = null;
        if (sprayEffect != null) sprayEffect.Stop();
    }

    private void Update()
    {
        if (sprayingHand == null) return;

        // SphereCast עם Collide: האש היא Trigger, ובלי זה הבדיקה מדלגת עליה
        int count = Physics.SphereCastNonAlloc(nozzle.position, radius, nozzle.forward, hits, range,
            Physics.DefaultRaycastLayers, QueryTriggerInteraction.Collide);

        for (int i = 0; i < count; i++)
        {
            Fire fire = hits[i].collider.GetComponentInParent<Fire>();
            if (fire != null) fire.Douse(Time.deltaTime);
        }

        // רטט קצר ומתחדש, כדי שהיד תרגיש את הלחץ כל עוד מרססים
        if (Time.time >= nextHapticTime)
        {
            sprayingHand.SendHapticImpulse(hapticAmplitude, hapticInterval);
            nextHapticTime = Time.time + hapticInterval;
        }
    }

    // ב-Scene View רואים את טווח הריסוס כשהמטף נבחר
    private void OnDrawGizmosSelected()
    {
        if (nozzle == null) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(nozzle.position, nozzle.position + nozzle.forward * range);
        Gizmos.DrawWireSphere(nozzle.position + nozzle.forward * range, radius);
    }
}
