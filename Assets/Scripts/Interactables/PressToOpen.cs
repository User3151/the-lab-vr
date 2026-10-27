using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

// פתיחה וסגירה בלחיצת כפתור, במקום למשוך ביד. כמו "Press E to open" במשחקים.
// יושב על אותו אובייקט כמו ConstrainedDial (דלת, מכסה, מפסק) או ConstrainedSlider (מגירה).
// מכוונים אל האובייקט: מופיעה הנחיה עם שם הכפתור. לוחצים: המצב מתהפך והאובייקט זז לבד עד הסוף.
// החידות לא משתנות: הן ממשיכות לקרוא את Value של הדלת או המגירה כמו קודם.
[RequireComponent(typeof(XRBaseInteractable))]
public class PressToOpen : MonoBehaviour
{
    [Header("Motion")]
    [SerializeField] private float openValue = 1f;      // עד כמה לפתוח, מ-0 עד 1 מהמהלך
    [SerializeField] private float seconds = 0.6f;      // משך התנועה
    [SerializeField] private float maxDistance = 3f;    // יד רחוקה מזה לא מקבלת הנחיה ולא פותחת

    [Header("Prompt (shared by every PressToOpen)")]
    [SerializeField] private Transform prompt;          // ההנחיה: רקע וטקסט שמצוירים מעל הכול
    [SerializeField] private TMP_Text promptText;
    [SerializeField] private float promptScalePerMeter = 1f;   // גדלה עם המרחק, כך שתמיד נראית באותו גודל

    [Header("Words")]
    [SerializeField] private string openVerb = "open";
    [SerializeField] private string closeVerb = "close";
    [SerializeField] private string lockedText = "LOCKED";
    [SerializeField] private string leftButton = "L1";
    [SerializeField] private string rightButton = "R1";

    // הנחיה אחת לכל הסצנה. זוכרים מי מציג אותה, כדי ששני אובייקטים לא יריבו עליה.
    private static PressToOpen showing;

    private XRBaseInteractable interactable;
    private ConstrainedDial dial;
    private ConstrainedSlider slider;
    private Transform viewer;
    private bool open;
    private float progress;              // 0 סגור, 1 פתוח, לפני ההחלקה
    private bool cancelSelection;
    private readonly List<IXRSelectFilter> filters = new();

    public bool IsOpen => open;

    private void Awake()
    {
        interactable = GetComponent<XRBaseInteractable>();
        dial = GetComponent<ConstrainedDial>();
        slider = GetComponent<ConstrainedSlider>();
        if (Camera.main != null) viewer = Camera.main.transform;
    }

    private void OnEnable() => interactable.selectEntered.AddListener(HandleSelected);

    private void OnDisable()
    {
        interactable.selectEntered.RemoveListener(HandleSelected);
        if (showing == this) Hide();
    }

    // לחיצה על כפתור התפיסה בזמן שמכוונים אל האובייקט היא "בחירה" ב-XRI.
    // משתמשים בה כלחיצה בלבד: הופכים את המצב, ובסוף הפריים משחררים, כדי שהיד לא תגרור את הדלת.
    // מסנני בחירה עדיין קובעים: המכסה של צנטריפוגה שרצה לא נבחר בכלל, ולכן גם לא נפתח.
    private void HandleSelected(SelectEnterEventArgs args)
    {
        cancelSelection = true;
        if (!interactable.enabled || !InRange(args.interactorObject.transform)) return;
        open = !open;
    }

    private void Update()
    {
        float target = open ? 1f : 0f;
        if (progress == target) return;

        progress = Mathf.MoveTowards(progress, target, Time.deltaTime / seconds);

        // SmoothStep מאט בהתחלה ובסוף, כמו דלת אמיתית
        float value = Mathf.SmoothStep(0f, 1f, progress) * openValue;
        if (dial != null) dial.SnapTo(value);
        if (slider != null) slider.SnapTo(value);
    }

    private void LateUpdate()
    {
        // משחררים כאן ולא בתוך האירוע, כי XRI עדיין באמצע הבחירה כשהאירוע נקרא
        if (cancelSelection)
        {
            cancelSelection = false;
            if (interactable.isSelected)
                interactable.interactionManager.CancelInteractableSelection((IXRSelectInteractable)interactable);
        }

        IXRHoverInteractor hand = HoveringHand();
        if (hand != null && (showing == null || showing == this)) Show(hand);
        else if (hand == null && showing == this) Hide();
    }

    // יד שמכוונת אל האובייקט ונמצאת מספיק קרוב. מפסק כבוי (אחרי שהחשמל חזר) לא מציג כלום.
    private IXRHoverInteractor HoveringHand()
    {
        if (!interactable.enabled) return null;

        foreach (IXRHoverInteractor hand in interactable.interactorsHovering)
            if (InRange(hand.transform)) return hand;
        return null;
    }

    private void Show(IXRHoverInteractor hand)
    {
        showing = this;

        // ההנחיה במרכז החלק שאפשר ללחוץ עליו, למשל החזית של המגירה
        Vector3 point = interactable.colliders.Count > 0 ? interactable.colliders[0].bounds.center : transform.position;
        prompt.position = point;
        if (viewer != null)
        {
            prompt.rotation = Quaternion.LookRotation(point - viewer.position);
            prompt.localScale = Vector3.one * (Vector3.Distance(viewer.position, point) * promptScalePerMeter);
        }

        bool left = hand.handedness == InteractorHandedness.Left;
        promptText.text = IsLocked(hand)
            ? lockedText
            : $"Press <color=#3DDC84>{(left ? leftButton : rightButton)}</color> to {(open ? closeVerb : openVerb)}";
        prompt.gameObject.SetActive(true);
    }

    private void Hide()
    {
        showing = null;
        if (prompt != null) prompt.gameObject.SetActive(false);
    }

    // המסננים הם שמחליטים אם מותר לבחור. אותה שאלה כאן קובעת אם להציג "נעול".
    private bool IsLocked(IXRHoverInteractor hand)
    {
        if (hand is not IXRSelectInteractor selector) return false;

        filters.Clear();
        interactable.selectFilters.GetAll(filters);
        foreach (IXRSelectFilter filter in filters)
            if (filter.canProcess && !filter.Process(selector, interactable)) return true;
        return false;
    }

    private bool InRange(Transform hand) => Vector3.Distance(hand.position, transform.position) <= maxDistance;
}
