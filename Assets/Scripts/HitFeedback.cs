using UnityEngine;

// משוב חושי בלבד. מאזין לשלושה רגעים ולא יודע כלום על אף אחד מהם.
public class HitFeedback : MonoBehaviour
{
    [SerializeField] private AudioClip grabClip;
    [SerializeField] private AudioClip hitClip;
    [SerializeField] private AudioClip missClip;
    [SerializeField] private ParticleSystem burstPrefab;

    private void OnEnable()
    {
        StarSpawner.OnStarGrabbed += HandleGrabbed;
        StarSpawner.OnStarMissed += HandleMissed;
        Gate.OnGateHit += HandleGateHit;
    }

    private void OnDisable()
    {
        StarSpawner.OnStarGrabbed -= HandleGrabbed;
        StarSpawner.OnStarMissed -= HandleMissed;
        Gate.OnGateHit -= HandleGateHit;
    }

    private void HandleGrabbed(ReachEvent e)
    {
        if (grabClip != null) AudioSource.PlayClipAtPoint(grabClip, e.handPosition);
    }

    private void HandleGateHit(Vector3 position)
    {
        if (hitClip != null) AudioSource.PlayClipAtPoint(hitClip, position);

        if (burstPrefab != null) Instantiate(burstPrefab, position, Quaternion.identity);
        else Debug.LogWarning("HitFeedback: burstPrefab is not assigned");
    }

    private void HandleMissed()
    {
        if (missClip != null) AudioSource.PlayClipAtPoint(missClip, transform.position);
    }
}