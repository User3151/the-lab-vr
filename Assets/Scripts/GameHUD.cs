using TMPro;
using UnityEngine;

// לוח ניקוד חי. מוצב פעם אחת בהתחלה, בצד, כדי שלא יסתיר את השער.
public class GameHUD : MonoBehaviour
{
    [SerializeField] private Transform bodyReference;
    [SerializeField] private TMP_Text hudText;
    [SerializeField] private float distanceFromPlayer = 2.5f;
    [SerializeField] private float heightOffset = 0.6f;
    [SerializeField] private float sideOffset = -1.3f;   // שלילי = שמאלה

    private void Start()
    {
        PlaceInWorld();
    }

    private void PlaceInWorld()
    {
        Vector3 forward = bodyReference.forward;
        forward.y = 0f;
        forward = forward.normalized;

        // Cross מחזיר וקטור ניצב לשניים. up ו-forward נותנים את הכיוון שמימין.
        Vector3 right = Vector3.Cross(Vector3.up, forward);

        transform.position = bodyReference.position
                           + forward * distanceFromPlayer
                           + right * sideOffset
                           + Vector3.up * heightOffset;

        transform.rotation = Quaternion.LookRotation(-forward);
    }

    public void Refresh(int score, int streak, int bestStreak)
    {
        hudText.text = $"SCORE  {score}\nSTREAK  x{streak}\nBEST  x{bestStreak}";
    }
}