using UnityEngine;

// אש קטנה שנחלשת כשמרססים עליה, וחוזרת לגדול אם מפסיקים באמצע.
// היא לא מכירה את המטף: מי שמרסס קורא ל-Douse.
public class Fire : MonoBehaviour
{
    [Header("Visuals")]
    [SerializeField] private ParticleSystem flames;
    [SerializeField] private Light glow;

    [Header("Behaviour")]
    [SerializeField] private float secondsToPutOut = 2.5f;   // ריסוס רציף שנדרש כדי לכבות
    [SerializeField] private float regrowDelay = 1.5f;       // כמה זמן בלי ריסוס עד שהאש חוזרת לגדול
    [SerializeField] private float regrowPerSecond = 0.25f;

    // מכריז כשהאש כבתה לגמרי. המשימה מאזינה לזה.
    public static event System.Action<Fire> OnExtinguished;

    public bool IsOut => strength <= 0f;

    private float strength = 1f;
    private float lastDousedTime = float.NegativeInfinity;
    private float baseEmission;
    private float baseGlow;

    private void Awake()
    {
        baseEmission = flames.emission.rateOverTimeMultiplier;
        baseGlow = glow.intensity;
    }

    // נקרא בכל פריים שבו הריסוס פוגע באש
    public void Douse(float deltaTime)
    {
        if (IsOut) return;

        lastDousedTime = Time.time;
        strength -= deltaTime / secondsToPutOut;

        if (strength <= 0f)
        {
            strength = 0f;
            flames.Stop();
            glow.enabled = false;
            OnExtinguished?.Invoke(this);
            Debug.Log($"{name} extinguished");
        }
    }

    private void Update()
    {
        if (IsOut) return;

        // כיבוי חלקי לא מספיק: אש שלא כובתה עד הסוף חוזרת
        if (Time.time - lastDousedTime > regrowDelay)
            strength = Mathf.Min(1f, strength + regrowPerSecond * Time.deltaTime);

        ParticleSystem.EmissionModule emission = flames.emission;
        emission.rateOverTimeMultiplier = baseEmission * strength;

        // הבהוב עדין של האור בעזרת רעש Perlin, זול יותר מאנימציה
        float flicker = 0.8f + 0.2f * Mathf.PerlinNoise(Time.time * 6f, 0f);
        glow.intensity = baseGlow * strength * flicker;
    }
}
