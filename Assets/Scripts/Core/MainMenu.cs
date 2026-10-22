using UnityEngine;
using UnityEngine.UI;

// התפריט הראשי: מעבר בין המסך הראשי להגדרות, יציאה מהמשחק, ועוצמות שנשמרות בין הפעלות.
// MASTER הוא AudioListener.volume ומשפיע על הכול. מוזיקה וקול מכפילים את העוצמה המקורית של המקורות שלהם.
public class MainMenu : MonoBehaviour
{
    const string MasterKey = "volume.master";
    const string MusicKey = "volume.music";
    const string VoiceKey = "volume.voice";

    [Header("Panels")]
    [SerializeField] GameObject mainPanel;
    [SerializeField] GameObject settingsPanel;
    [SerializeField] Button settingsButton;
    [SerializeField] Button backButton;
    [SerializeField] Button exitButton;

    [Header("Volume")]
    [SerializeField] Slider masterSlider;
    [SerializeField] Slider musicSlider;
    [SerializeField] Slider voiceSlider;
    [SerializeField] AudioSource[] musicSources;
    [SerializeField] AudioSource[] voiceSources;

    float[] musicBase;   // העוצמות שנקבעו בסצנה. הסליידר מכפיל אותן
    float[] voiceBase;

    void Awake()
    {
        musicBase = BaseVolumes(musicSources);
        voiceBase = BaseVolumes(voiceSources);

        // SetValueWithoutNotify: מציבים את הערך השמור בלי להפעיל את האירוע של הסליידר
        masterSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat(MasterKey, 1f));
        musicSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat(MusicKey, 1f));
        voiceSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat(VoiceKey, 1f));
        Apply();
        ShowMain();
    }

    void OnEnable()
    {
        settingsButton.onClick.AddListener(ShowSettings);
        backButton.onClick.AddListener(ShowMain);
        exitButton.onClick.AddListener(Exit);
        masterSlider.onValueChanged.AddListener(HandleSlider);
        musicSlider.onValueChanged.AddListener(HandleSlider);
        voiceSlider.onValueChanged.AddListener(HandleSlider);
    }

    void OnDisable()
    {
        settingsButton.onClick.RemoveListener(ShowSettings);
        backButton.onClick.RemoveListener(ShowMain);
        exitButton.onClick.RemoveListener(Exit);
        masterSlider.onValueChanged.RemoveListener(HandleSlider);
        musicSlider.onValueChanged.RemoveListener(HandleSlider);
        voiceSlider.onValueChanged.RemoveListener(HandleSlider);
    }

    void ShowMain()
    {
        settingsPanel.SetActive(false);
        mainPanel.SetActive(true);
        PlayerPrefs.Save();   // חוזרים מההגדרות: זה הרגע לשמור לדיסק
    }

    void ShowSettings()
    {
        mainPanel.SetActive(false);
        settingsPanel.SetActive(true);
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

    void Exit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
