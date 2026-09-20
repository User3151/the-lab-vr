using UnityEngine;

// תג על פיוז: תקין או שרוף.
// השקע לא מתעניין בזה, הוא מקבל כל פיוז. החידה היא שבודקת אם הפיוז שהוכנס עובד.
public class Fuse : MonoBehaviour
{
    [SerializeField] private bool burnt;

    public bool IsBurnt => burnt;
}
