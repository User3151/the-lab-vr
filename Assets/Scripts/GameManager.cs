using UnityEngine;
using UnityEngine.InputSystem;

// סופר ניקוד ורצף. מאזין בלבד - לא יודע איך הכוכב או השער עובדים.
public class GameManager : MonoBehaviour
{
    [Header("Scoring")]
    [SerializeField] private int basePoints = 10;
    [SerializeField] private float reachBonusPerMeter = 20f;   // תפסת רחוק = יותר נקודות
    [SerializeField] private GameHUD hud;

    private int score = 0;
    private int streak = 0;
    private int bestStreak = 0;
    private float lastReachDistance = 0f;

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

    private void Start()
    {
        UpdateHud();
    }

    // רק זוכרים כמה רחוק הושטת. הבונוס יינתן רק אם תפגע.
    private void HandleGrabbed(ReachEvent e)
    {
        lastReachDistance = e.reachDistance;
    }

    private void HandleGateHit(Vector3 position)
    {
        streak++;
        bestStreak = Mathf.Max(bestStreak, streak);

        int reachBonus = Mathf.RoundToInt(lastReachDistance * reachBonusPerMeter);
        int points = (basePoints + reachBonus) * streak;
        score += points;

        Debug.Log($"HIT! +{points} (base {basePoints} + reach {reachBonus}) x{streak} | Score: {score}");
        UpdateHud();
    }

    private void HandleMissed()
    {
        streak = 0;
        Debug.Log($"MISS. Streak reset. Score: {score}");
        UpdateHud();
    }

    private void UpdateHud()
    {
        if (hud != null)
        {
            hud.Refresh(score, streak, bestStreak);
        }
        else
        {
            Debug.LogWarning("GameManager: hud is not assigned in the Inspector!");
        }
    }
}