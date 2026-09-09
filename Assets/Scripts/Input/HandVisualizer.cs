using UnityEngine;

// מציג את מצב היד. לא יודע כלום על מצלמות או על MediaPipe -
// הוא מדבר רק דרך החוזה, ולכן יעבוד בדיוק אותו דבר מול שלט או הקלטה.
public class HandVisualizer : MonoBehaviour
{
    [SerializeField] private MonoBehaviour handSource;   // חייב לממש IHandInput
    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private Color idleColor = new Color(0.4f, 0.8f, 1f);
    [SerializeField] private Color grabColor = new Color(1f, 0.63f, 0.19f);

    private IHandInput hand;

    private void Awake()
    {
        // Unity לא יודעת לגרור ממשק לשדה בעורך.
        // לכן גוררים MonoBehaviour רגיל, וממירים אותו לחוזה כאן.
        hand = handSource as IHandInput;

        if (hand == null)
        {
            Debug.LogError("HandVisualizer: the object in Hand Source does not implement IHandInput!");
            enabled = false;
        }
    }

    private void Update()
    {
        // אין יד בתמונה - מסתירים את הכדור במקום להשאיר אותו תקוע
        if (!hand.IsTracked)
        {
            if (targetRenderer.enabled) targetRenderer.enabled = false;
            return;
        }

        if (!targetRenderer.enabled) targetRenderer.enabled = true;

        transform.position = hand.Position;
        targetRenderer.material.color = hand.IsGrabbing ? grabColor : idleColor;
    }
}