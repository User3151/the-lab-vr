using System.Collections;
using UnityEngine;

// התאורה של המעבדה: אורות חירום אדומים ועמומים עד שהחשמל חוזר,
// ואז גופי התאורה בתקרה נדלקים אחד אחרי השני ומהבהבים כמו פלורסנט.
// היא לא יודעת למה החשמל חזר. מי שמחליט (החידה) פשוט קורא ל-PowerOn.
public class LabLighting : MonoBehaviour
{
    // מנורה אחת: האור עצמו, החלק שנראה דולק (משנה חומר בין דולק לכבוי), והצליל שלה
    [System.Serializable]
    public class Lamp
    {
        public Light light;
        public Renderer glow;
        public AudioSource sound;   // מתנגן כשהמנורה מתחילה להבהב. ריק = מנורה שקטה
    }

    [Header("Emergency")]
    [SerializeField] private Lamp[] emergency;
    [SerializeField] private Material emergencyOn;
    [SerializeField] private Material emergencyOff;

    [Header("Ceiling (in switch-on order)")]
    [SerializeField] private Lamp[] ceiling;
    [SerializeField] private Material ceilingOn;
    [SerializeField] private Material ceilingOff;

    [Header("Ambient")]
    [SerializeField] private Color ambientDark = new(0.02f, 0.015f, 0.015f);  // כמעט שחור, שהאדום ישלוט
    [SerializeField] private Color ambientLit = new(0.2f, 0.21f, 0.23f);

    [Header("Power on")]
    [SerializeField] private float blackoutSeconds = 0.5f;        // רגע של חושך מוחלט לפני שהכול נדלק
    [SerializeField] private float delayBetweenFixtures = 0.35f;
    [SerializeField] private int flickers = 2;
    [SerializeField] private float flickerSeconds = 0.06f;

    private bool powered;

    // מצב פתיחה: רק חירום. נקבע כאן ולא בסצנה, כדי שהסצנה תוכל להיות מוארת בזמן עבודה בעורך.
    private void Awake()
    {
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = ambientDark;

        foreach (Lamp lamp in emergency) Set(lamp, true, emergencyOn);
        foreach (Lamp lamp in ceiling) Set(lamp, false, ceilingOff);
    }

    public void PowerOn()
    {
        if (powered) return;
        powered = true;
        StartCoroutine(PowerOnSequence());
    }

    private IEnumerator PowerOnSequence()
    {
        // אורות החירום כבים ברגע שהחשמל הרגיל חוזר
        foreach (Lamp lamp in emergency) Set(lamp, false, emergencyOff);
        yield return new WaitForSeconds(blackoutSeconds);

        RenderSettings.ambientLight = ambientLit;

        // כל גוף מתחיל להבהב בתורו. לא מחכים שהקודם יסיים, כך שההבהובים חופפים קצת.
        foreach (Lamp lamp in ceiling)
        {
            StartCoroutine(FlickerOn(lamp));
            yield return new WaitForSeconds(delayBetweenFixtures);
        }
    }

    // פלורסנט לא נדלק בבת אחת: הוא מהבהב כמה פעמים ורק אז נשאר דולק
    private IEnumerator FlickerOn(Lamp lamp)
    {
        if (lamp.sound != null) lamp.sound.Play();

        for (int i = 0; i < flickers; i++)
        {
            Set(lamp, true, ceilingOn);
            yield return new WaitForSeconds(flickerSeconds);
            Set(lamp, false, ceilingOff);
            yield return new WaitForSeconds(flickerSeconds * 2f);
        }
        Set(lamp, true, ceilingOn);
    }

    private static void Set(Lamp lamp, bool on, Material material)
    {
        if (lamp.light != null) lamp.light.enabled = on;
        if (lamp.glow != null) lamp.glow.sharedMaterial = material;
    }
}
