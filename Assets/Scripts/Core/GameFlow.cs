using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using Unity.XR.CoreUtils;

// מנהל את מהלך המשחק: מסך פתיחה, זיהוי יציאה מהמעבדה, מסך סיום והתחלה מחדש
public class GameFlow : MonoBehaviour
{
    [Header("Start")]
    [SerializeField] GameObject startScreen;
    [SerializeField] Button startButton;
    [SerializeField] GameObject[] gameplayRoots;   // נדלקים רק אחרי START (הרצף והמאמן)

    [Header("Exit")]
    [SerializeField] Collider exitZone;            // המסדרון שמעבר לדלת

    [Header("End")]
    [SerializeField] GameObject endScreen;
    [SerializeField] Transform endSpawn;           // נקודה על רצפת חדר הסיום, פונה אל המסך
    [SerializeField] TMP_Text timeText;
    [SerializeField] Button restartButton;

    [SerializeField] XROrigin origin;

    [Header("Events")]
    [SerializeField] UnityEvent onGameStarted;     // למשל: מוזיקת הרקע מתחילה
    [SerializeField] UnityEvent onGameEnded;       // למשל: מוזיקת הסיום

    float startTime;
    bool doorOpen;   // כל הרשימה נפתרה, מותר לצאת
    bool finished;

    void Awake()
    {
        // המשחק עוד לא התחיל: הרצף והמאמן כבויים, ה-Start שלהם ירוץ כשיידלקו
        foreach (var go in gameplayRoots) go.SetActive(false);
        endScreen.SetActive(false);
        startScreen.SetActive(true);
    }

    void OnEnable()
    {
        startButton.onClick.AddListener(StartGame);
        restartButton.onClick.AddListener(Restart);
        PuzzleSequencer.OnAllSolved += HandleAllSolved;
    }

    void OnDisable()
    {
        startButton.onClick.RemoveListener(StartGame);
        restartButton.onClick.RemoveListener(Restart);
        PuzzleSequencer.OnAllSolved -= HandleAllSolved;
    }

    void StartGame()
    {
        startScreen.SetActive(false);
        foreach (var go in gameplayRoots) go.SetActive(true);
        startTime = Time.time;
        onGameStarted.Invoke();
    }

    void HandleAllSolved()
    {
        doorOpen = true;
    }

    void Update()
    {
        if (!doorOpen || finished) return;

        // הראש של השחקן בתוך המסדרון = יצא מהמעבדה
        if (exitZone.bounds.Contains(origin.Camera.transform.position))
            EndGame();
    }

    void EndGame()
    {
        finished = true;
        endScreen.SetActive(true);

        // מסובבים את הריג סביב הראש, כך שהשחקן יסתכל לכיוון של נקודת הסיום
        Transform head = origin.Camera.transform;
        Vector3 look = Vector3.ProjectOnPlane(head.forward, Vector3.up);
        float angle = Vector3.SignedAngle(look, endSpawn.forward, Vector3.up);
        origin.transform.RotateAround(head.position, Vector3.up, angle);

        // מזיזים את הריג כך שהראש יעמוד מעל הנקודה, והרגליים על הרצפה שלה
        Vector3 offset = head.position - origin.transform.position;
        origin.transform.position = endSpawn.position - new Vector3(offset.x, 0f, offset.z);
        Physics.SyncTransforms();

        float seconds = Time.time - startTime;
        int minutes = Mathf.FloorToInt(seconds / 60f);
        timeText.text = minutes.ToString("00") + ":" + Mathf.FloorToInt(seconds % 60f).ToString("00");

        onGameEnded.Invoke();
    }

    void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
