using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using Unity.XR.CoreUtils;

// מנהל את מהלך המשחק: תפריט בחדר נפרד, מעבר למעבדה, זיהוי יציאה, מסך סיום והתחלה מחדש.
// התפריט והסיום נמצאים בחדר חשוך מחוץ למעבדה, כך שאי אפשר להסתובב במעבדה לפני START.
public class GameFlow : MonoBehaviour
{
    // משתנה סטטי לא נמחק כשהסצנה נטענת מחדש: כך PLAY AGAIN מדלג על התפריט
    static bool skipMenu;

    [Header("Menu")]
    [SerializeField] GameObject menuScreen;
    [SerializeField] Button startButton;
    [SerializeField] GameObject[] gameplayRoots;   // נדלקים רק אחרי START (הרצף והמאמן)

    [Header("Places")]
    [SerializeField] XROrigin origin;
    [SerializeField] Transform offstageSpawn;      // הרצפה של החדר החשוך, פונה אל המסך

    [Header("Exit")]
    [SerializeField] Collider exitZone;            // המסדרון שמעבר לדלת

    [Header("End")]
    [SerializeField] GameObject endScreen;
    [SerializeField] TMP_Text timeText;
    [SerializeField] Button restartButton;
    [SerializeField] Button menuButton;

    [Header("Events")]
    [SerializeField] UnityEvent onGameStarted;     // למשל: רעש החדר והאזעקה מתחילים
    [SerializeField] UnityEvent onGameEnded;       // למשל: מוזיקת הסיום

    Vector3 labPosition;   // איפה הריג מונח בסצנה: נקודת ההתחלה במעבדה
    Vector3 labForward;
    float startTime;
    bool doorOpen;         // כל הרשימה נפתרה, מותר לצאת
    bool finished;

    void Awake()
    {
        labPosition = origin.transform.position;
        labForward = origin.transform.forward;

        // המשחק עוד לא התחיל: הרצף והמאמן כבויים, ה-Start שלהם ירוץ כשיידלקו
        foreach (var go in gameplayRoots) go.SetActive(false);
        endScreen.SetActive(false);
        menuScreen.SetActive(true);
        MoveRig(offstageSpawn.position, offstageSpawn.forward);
    }

    void Start()
    {
        if (!skipMenu) return;
        skipMenu = false;
        StartGame();
    }

    void OnEnable()
    {
        startButton.onClick.AddListener(StartGame);
        restartButton.onClick.AddListener(PlayAgain);
        menuButton.onClick.AddListener(BackToMenu);
        PuzzleSequencer.OnAllSolved += HandleAllSolved;
    }

    void OnDisable()
    {
        startButton.onClick.RemoveListener(StartGame);
        restartButton.onClick.RemoveListener(PlayAgain);
        menuButton.onClick.RemoveListener(BackToMenu);
        PuzzleSequencer.OnAllSolved -= HandleAllSolved;
    }

    void StartGame()
    {
        menuScreen.SetActive(false);
        MoveRig(labPosition, labForward);
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
        MoveRig(offstageSpawn.position, offstageSpawn.forward);

        float seconds = Time.time - startTime;
        int minutes = Mathf.FloorToInt(seconds / 60f);
        timeText.text = minutes.ToString("00") + ":" + Mathf.FloorToInt(seconds % 60f).ToString("00");

        onGameEnded.Invoke();
    }

    // מעביר את השחקן כך שהראש יעמוד מעל הנקודה ויסתכל לכיוון שלה, והרגליים על הרצפה שלה
    void MoveRig(Vector3 floorPoint, Vector3 facing)
    {
        Transform head = origin.Camera.transform;
        Vector3 look = Vector3.ProjectOnPlane(head.forward, Vector3.up);
        float angle = Vector3.SignedAngle(look, Vector3.ProjectOnPlane(facing, Vector3.up), Vector3.up);
        origin.transform.RotateAround(head.position, Vector3.up, angle);

        Vector3 offset = head.position - origin.transform.position;
        origin.transform.position = floorPoint - new Vector3(offset.x, 0f, offset.z);
        Physics.SyncTransforms();
    }

    void PlayAgain()
    {
        skipMenu = true;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void BackToMenu()
    {
        skipMenu = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
