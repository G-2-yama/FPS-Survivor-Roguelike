using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TutorialTasks", menuName = "Tutorial/課題の一覧")]
public class TutorialTaskList : ScriptableObject
{
    [Tooltip("上から順に出す課題")]
    [SerializeField] private List<TutorialTask> tasks = new List<TutorialTask>();

    [Tooltip("すべての課題を終えたあとに出す文")]
    [TextArea(2, 4)]
    [SerializeField] private string finishedText;

    public IReadOnlyList<TutorialTask> Tasks => tasks;
    public string FinishedText => finishedText;
}
