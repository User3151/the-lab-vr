using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// סליידר שגוררים עם קרן של שלט.
// הסליידר הרגיל מחשב את המיקום מנקודה על המסך. כשהקרן מפספסת את הלוח אפילו לפריים אחד,
// XRI נותן לו נקודה בקצה הרחוק של הקרן, והידית קופצת לשם ובחזרה.
// כאן לוקחים את הנקודה שבה הקרן פגעה בלוח בעולם, ומתעלמים מפריים שבו היא לא פגעה בכלל.
// מתאים לסליידר אופקי משמאל לימין, כמו אלה שבהגדרות.
public class RaySlider : Slider
{
    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);   // הבחירה והתחלת הגרירה, כמו בסליידר רגיל
        SetFromRay(eventData);
    }

    public override void OnDrag(PointerEventData eventData)
    {
        if (!IsActive() || !IsInteractable() || eventData.button != PointerEventData.InputButton.Left) return;
        SetFromRay(eventData);
    }

    private void SetFromRay(PointerEventData eventData)
    {
        RaycastResult hit = eventData.pointerCurrentRaycast;
        if (!hit.isValid) return;   // הקרן לא פגעה בלוח בפריים הזה: הידית נשארת במקום

        // האזור שהידית נעה בו. ממירים את נקודת הפגיעה למרחב שלו, ושם x הוא המיקום לאורך הסליידר.
        RectTransform area = handleRect != null ? handleRect.parent as RectTransform : fillRect.parent as RectTransform;
        Vector3 local = area.InverseTransformPoint(hit.worldPosition);
        normalizedValue = Mathf.InverseLerp(area.rect.xMin, area.rect.xMax, local.x);
    }
}
