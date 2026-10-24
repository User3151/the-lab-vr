using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// התפריט הראשי והגדרות. אותו לוח הגדרות משמש בשני מקומות:
// בתפריט הוא נפתח מכפתור SETTINGS, ובאמצע המשחק בכפתור בשלט, מול השחקן.
// MASTER הוא AudioListener.volume ומשפיע על הכול. מוזיקה וקול מכפילים את העוצמה המקורית של המקורות שלהם.
public class MainMenu : MonoBehaviour
{
    const string MasterKey = "volume.master";
    const string MusicKey = "volume.music";
    const string VoiceKey = "volume.voice";

    [Header("Panels")]
    [SerializeField] GameObject mainPanel;          // START / SETTINGS / EXIT בחדר התפריט
    [SerializeField] GameObject settingsScreen;     // לוח ההגדרות, קנבס נפרד
    [SerializeField] GameObject inGameButtons;      // MAIN MENU ו-EXIT GAME, רק באמצע משחק
    [SerializeField] Transform menuSpot;            // איפה ההגדרות עומדות בחדר התפריט

    [Header("Buttons")]
    [SerializeField] Button settingsButton;
    [SerializeField] Button backButton;
    [SerializeField] Button exitButton;
    [SerializeField] Button inGameMenuButton;
    [SerializeField] Button inGameExitButton;

    [Header("In game")]
    [SerializeField] Transform head;
    [SerializeField] float distance = 0.9f;
    [SerializeField] string[] toggleBindings =
    {
        "<XRController>{LeftHand}/secondaryButton",
        "<Keyboard>/escape",
    };

    [Header("Volume")]
    [SerializeField] Slider masterSlider;
    [SerializeField] Slider musicSlider;
    [SerializeField] Slider voiceSlider;
    [SerializeField] AudioSource[] musicSources;
    [SerializeField] AudioSource[] voiceSources;

    float[] musicBase;   // העוצמות שנקבעו בסצנה. הסליידר מכפיל אותן
    float[] voiceBase;
    bool inGame;
    InputAction toggleAction;

    void Awake()
    {
        if (head == null && Camera.main != null) head = Camera.main.transform;

        toggleAction = new InputAction("ToggleSettings", InputActionType.Button);
        foreach (string binding in toggleBindings) toggleAction.AddBinding(binding);

        musicBase = BaseVolumes(musicSources);
        voiceBase = BaseVolumes(voiceSources);

        // SetValueWithoutNotify: מציבים את הערך השמור בלי להפעיל את האירוע של הסליידר
        masterSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat(MasterKey, 1f));
        musicSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat(MusicKey, 1f));
        voiceSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat(VoiceKey, 1f));
        Apply();

        settingsScreen.SetActive(false);
        mainPanel.SetActive(true);
    }

    void OnEnable()
    {
        settingsButton.onClick.AddListener(OpenFromMenu);
        backButton.onClick.AddListener(Close);
        exitButton.onClick.AddListener(Exit);
        inGameMenuButton.onClick.AddListener(BackToMenu);
        inGameExitButton.onClick.AddListener(Exit);
        masterSlider.onValueChanged.AddListener(HandleSlider);
        musicSlider.onValueChanged.AddListener(HandleSlider);
        voiceSlider.onValueChanged.AddListener(HandleSlider);
        toggleAction.performed += HandleToggle;
        toggleAction.Enable();
    }

    void OnDisable()
    {
        settingsButton.onClick.RemoveListener(OpenFromMenu);
        backButton.onClick.RemoveListener(Close);
        exitButton.onClick.RemoveListener(Exit);
        inGameMenuButton.onClick.RemoveListener(BackToMenu);
        inGameExitButton.onClick.RemoveListener(Exit);
        masterSlider.onValueChanged.RemoveListener(HandleSlider);
        musicSlider.onValueChanged.RemoveListener(HandleSlider);
        voiceSlider.onValueChanged.RemoveListener(HandleSlider);
        toggleAction.performed -= HandleToggle;
        toggleAction.Disable();
    }

    // GameFlow קורא לזה: true כשנכנסים למעבדה, false במסך הסיום
    public void SetInGame(bool value)
    {
        inGame = value;
        settingsScreen.SetActive(false);
    }

    void OpenFromMenu()
    {
        mainPanel.SetActive(false);
        settingsScreen.transform.SetPositionAndRotation(menuSpot.position, menuSpot.rotation);
        inGameButtons.SetActive(false);
        settingsScreen.SetActive(true);
    }

    // מול השחקן. קיר קרוב מהמרחק הרגיל מקרב את הלוח, כדי שלא ייבלע בקיר.
    void OpenInGame()
    {
        Vector3 forward = Vector3.ProjectOnPlane(head.forward, Vector3.up).normalized;
        if (forward.sqrMagnitude < 0.001f) forward = Vector3.forward;

        float d = distance;
        if (Physics.Raycast(head.position, forward, out RaycastHit hit, distance, ~0, QueryTriggerInteraction.Ignore))
            d = Mathf.Max(0.4f, hit.distance - 0.1f);

        Vector3 position = head.position + forward * d;
        settingsScreen.transform.SetPositionAndRotation(position, Quaternion.LookRotation(forward));
        inGameButtons.SetActive(true);
        settingsScreen.SetActive(true);
    }

    void Close()
    {
        settingsScreen.SetActive(false);
        if (!inGame) mainPanel.SetActive(true);
        PlayerPrefs.Save();   // יוצאים מההגדרות: זה הרגע לשמור לדיסק
    }

    void HandleToggle(InputAction.CallbackContext context)
    {
        if (!inGame) return;
        if (settingsScreen.activeSelf) Close();
        else OpenInGame();
    }

    // כל שינוי בסליידר נשמע מיד, ונרשם לשמירה
    void HandleSlider(float _)
    {
        Apply();
        PlayerPrefs.SetFloat(MasterKey, masterSlider.value);
        PlayerPrefs.SetFloat(MusicKey, musicSlider.value);
        PlayerPrefs.SetFloat(VoiceKey, voiceSlider.value);
    }

    void Apply()
    {
        AudioListener.volume = masterSlider.value;
        Scale(musicSources, musicBase, musicSlider.value);
        Scale(voiceSources, voiceBase, voiceSlider.value);
    }

    static float[] BaseVolumes(AudioSource[] sources)
    {
        var volumes = new float[sources.Length];
        for (int i = 0; i < sources.Length; i++) volumes[i] = sources[i].volume;
        return volumes;
    }

    static void Scale(AudioSource[] sources, float[] baseVolumes, float factor)
    {
        for (int i = 0; i < sources.Length; i++) sources[i].volume = baseVolumes[i] * factor;
    }

    // טעינה מחדש של הסצנה מציגה את התפריט, כי GameFlow מדלג עליו רק אחרי PLAY AGAIN
    void BackToMenu()
    {
        PlayerPrefs.Save();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void Exit()
    {
        PlayerPrefs.Save();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
