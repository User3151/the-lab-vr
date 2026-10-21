using UnityEngine;

// הצלילים של אירועי המשחק: רשימת הבטיחות, חפצים שנכנסים לשקע, האש ואזעקת הלחץ.
// מאזין לאירועים שהפאזלים כבר משדרים, כך שהם לא צריכים לדעת שיש סאונד.
public class GameAudio : MonoBehaviour
{
    [System.Serializable]
    private class SocketSound
    {
        public ValidatedSocket socket;
        public AudioClip clip;
    }

    [Header("Checklist")]
    [SerializeField] private AudioSource interfaceSource;   // ליד מסוף הנעילה
    [SerializeField] private AudioClip taskSolved;
    [SerializeField] private AudioClip taskBroken;

    [Header("Sockets")]
    [SerializeField] private SocketSound[] sockets;
    [SerializeField] private float quietSeconds = 1f;   // בטעינה השקעים מתמלאים בחפצים שמתחילים בהם

    [Header("Fire")]
    [SerializeField] private AudioSource fireLoop;
    [SerializeField] private AudioClip fireOut;

    [Header("Pressure")]
    [SerializeField] private AudioSource pressureAlarm;   // לופ, ליד שעון הלחץ
    [SerializeField] private AudioClip vent;
    [SerializeField] private float alarmSeconds = 3f;

    private void OnEnable()
    {
        PuzzleStep.AnySolved += HandleSolved;
        PuzzleStep.AnyUnsolved += HandleUnsolved;
        ValidatedSocket.OnFilled += HandleFilled;
        Fire.OnExtinguished += HandleExtinguished;
        ValvePuzzle.OnAlarm += HandleAlarm;
    }

    private void OnDisable()
    {
        PuzzleStep.AnySolved -= HandleSolved;
        PuzzleStep.AnyUnsolved -= HandleUnsolved;
        ValidatedSocket.OnFilled -= HandleFilled;
        Fire.OnExtinguished -= HandleExtinguished;
        ValvePuzzle.OnAlarm -= HandleAlarm;
    }

    // הקבוצה נפתרת יחד עם המשימה האחרונה בה. בלי הסינון היה נשמע צליל כפול.
    private void HandleSolved(PuzzleStep step)
    {
        if (step is PuzzleGroup) return;
        interfaceSource.PlayOneShot(taskSolved);
    }

    private void HandleUnsolved(PuzzleStep step)
    {
        if (step is PuzzleGroup) return;
        interfaceSource.PlayOneShot(taskBroken);
    }

    private void HandleFilled(ValidatedSocket socket, Transform item)
    {
        if (Time.timeSinceLevelLoad < quietSeconds) return;

        foreach (SocketSound entry in sockets)
        {
            if (entry.socket != socket) continue;
            AudioSource.PlayClipAtPoint(entry.clip, socket.transform.position);
            return;
        }
    }

    private void HandleExtinguished(Fire fire)
    {
        fireLoop.Stop();
        AudioSource.PlayClipAtPoint(fireOut, fire.transform.position);
    }

    // הלחץ עבר את הקו האדום: הגז משתחרר בבת אחת והאזעקה מצפצפת כמה שניות
    private void HandleAlarm()
    {
        AudioSource.PlayClipAtPoint(vent, pressureAlarm.transform.position);
        pressureAlarm.Play();
        CancelInvoke(nameof(StopAlarm));
        Invoke(nameof(StopAlarm), alarmSeconds);
    }

    private void StopAlarm() => pressureAlarm.Stop();
}
