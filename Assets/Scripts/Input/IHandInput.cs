using UnityEngine;

// החוזה. כל מקור קלט יד - מצלמה, שלט, הקלטה - חייב לענות על שלוש השאלות.
// המשחק שואל דרך החוזה ולא יודע מי עונה לו.
public interface IHandInput
{
    bool IsTracked { get; }     // האם רואים יד בכלל
    Vector3 Position { get; }   // איפה כף היד בעולם
    bool IsGrabbing { get; }    // האם היא צובטת עכשיו
}