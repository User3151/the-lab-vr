using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

// שקע שמקבל רק חפץ עם SocketKey מתאים.
// יורש מ-XRSocketInteractor: ההצמדה, ההחזקה וההדגשה כבר קיימות, אנחנו מוסיפים רק סינון.
public class ValidatedSocket : XRSocketInteractor
{
    [Header("Validation")]
    [SerializeField] private string requiredKeyId = "fuse";

    // מכריז כשהחפץ הנכון נכנס. הפאזל יאזין לזה ולא יצטרך להכיר את השקע.
    public static event System.Action<ValidatedSocket> OnFilled;

    private bool reportedFilled;

    // Hover: בלי הבדיקה, השקע היה מציג תצוגה מקדימה לכל חפץ שמתקרב, גם כזה שלא ייכנס
    public override bool CanHover(IXRHoverInteractable interactable)
    {
        return base.CanHover(interactable) && HasMatchingKey(interactable);
    }

    // Select: השער האמיתי. XRI שואל את זה לפני שהוא מצמיד חפץ לשקע.
    public override bool CanSelect(IXRSelectInteractable interactable)
    {
        return base.CanSelect(interactable) && HasMatchingKey(interactable);
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);

        if (reportedFilled) return;
        reportedFilled = true;

        OnFilled?.Invoke(this);
        Debug.Log($"{name} accepted {args.interactableObject.transform.name}");
    }

    // שני סוגי ה-interactable יורשים מ-IXRInteractable, אז פונקציה אחת מכסה את שניהם
    private bool HasMatchingKey(IXRInteractable interactable)
    {
        return interactable.transform.TryGetComponent(out SocketKey key)
            && key.KeyId == requiredKeyId;
    }
}
