using System.Collections.Generic;
using UnityEngine;

// צעד שמורכב מכמה משימות שרצות במקביל, כמו רשימת בדיקות בטיחות.
// כשהקבוצה מופעלת, כל המשימות נפתחות יחד והשחקן בוחר את הסדר. הקבוצה נפתרת כשכולן נפתרו.
public class PuzzleGroup : PuzzleStep
{
    [SerializeField] private List<PuzzleStep> tasks = new();

    public IReadOnlyList<PuzzleStep> Tasks => tasks;

    protected override void OnActivated()
    {
        // נרשמים לכולן לפני שמפעילים: משימה שכבר מתקיימת נפתרת מיד בתוך Activate
        foreach (PuzzleStep task in tasks) task.Solved += HandleTaskSolved;
        foreach (PuzzleStep task in tasks) task.Activate();

        SolveIfAllDone();
    }

    private void HandleTaskSolved(PuzzleStep task)
    {
        task.Solved -= HandleTaskSolved;
        SolveIfAllDone();
    }

    private void SolveIfAllDone()
    {
        foreach (PuzzleStep task in tasks)
            if (!task.IsSolved) return;
        Solve();
    }

    protected override void OnSolved() => Debug.Log($"{DisplayName} complete");
}
