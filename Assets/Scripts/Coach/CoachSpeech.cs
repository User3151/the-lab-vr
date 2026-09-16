using System.Collections.Generic;
using TMPro;
using UnityEngine;

// הפה של המנחה: תור של משפטים, כתובית מעל הרחפן, והשמעת הקול.
// משפט נשאר על המסך כאורך ההקלטה, ובלי הקלטה לפי אורך הטקסט.
public class CoachSpeech : MonoBehaviour
{
    [SerializeField] private TMP_Text subtitle;
    [SerializeField] private AudioSource voice;

    [Header("Timing")]
    [SerializeField] private float secondsPerCharacter = 0.06f;   // קצב קריאה כשאין הקלטה
    [SerializeField] private float minSeconds = 2f;
    [SerializeField] private float pauseAfterLine = 0.4f;

    private readonly Queue<CoachLine> queue = new();
    private CoachLine current;
    private float currentEndTime;

    // המנחה "באמצע דיבור" גם כשמשפטים מחכים בתור. הרמזים לא יקטעו אותו.
    public bool IsSpeaking => current != null || queue.Count > 0;

    private Transform viewer;

    private void Awake()
    {
        subtitle.gameObject.SetActive(false);
        if (Camera.main != null) viewer = Camera.main.transform;
    }

    // הכתובית תמיד פונה לשחקן, גם כשהרחפן עצמו מסתכל על נקודת עניין.
    // טקסט נקרא נכון כשהצד הקדמי שלו פונה הרחק מהמצלמה.
    private void LateUpdate()
    {
        if (viewer == null || !subtitle.gameObject.activeSelf) return;
        subtitle.transform.rotation = Quaternion.LookRotation(subtitle.transform.position - viewer.position);
    }

    public void Say(IEnumerable<CoachLine> lines)
    {
        if (lines == null) return;
        foreach (CoachLine line in lines)
            if (line != null && !string.IsNullOrWhiteSpace(line.text)) queue.Enqueue(line);
    }

    public void Say(CoachLine line) => Say(new[] { line });

    // לתגובות דחופות (אזעקה): מבטל את מה שבתור ומדבר מיד
    public void Interrupt(CoachLine line)
    {
        queue.Clear();
        current = null;
        voice.Stop();
        Say(line);
    }

    private void Update()
    {
        if (current != null && Time.time >= currentEndTime)
        {
            current = null;
            if (queue.Count == 0) subtitle.gameObject.SetActive(false);
        }

        if (current == null && queue.Count > 0) StartLine(queue.Dequeue());
    }

    private void StartLine(CoachLine line)
    {
        current = line;
        subtitle.text = line.text;
        subtitle.gameObject.SetActive(true);

        float duration;
        if (line.clip != null)
        {
            voice.clip = line.clip;
            voice.Play();
            duration = line.clip.length;
        }
        else
        {
            duration = Mathf.Max(minSeconds, line.text.Length * secondsPerCharacter);
        }

        currentEndTime = Time.time + duration + pauseAfterLine;
    }
}
