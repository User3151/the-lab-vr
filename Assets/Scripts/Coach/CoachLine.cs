using UnityEngine;

// משפט אחד של המנחה: הטקסט שמופיע בכתובית, והקלטת הקול שלו (אם יש).
[System.Serializable]
public class CoachLine
{
    [TextArea(2, 4)] public string text;
    public AudioClip clip;
}
