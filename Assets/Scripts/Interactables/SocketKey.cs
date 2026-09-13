using UnityEngine;

// תג זיהוי לחפץ שאמור להיכנס לשקע.
// השקע לא מכיר את הנתיך, רק את המזהה שלו.
public class SocketKey : MonoBehaviour
{
    [SerializeField] private string keyId = "fuse";

    public string KeyId => keyId;
}
