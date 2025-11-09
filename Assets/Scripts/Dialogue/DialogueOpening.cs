using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class DialogueOpening : MonoBehaviour
{
    [SerializeField] private TextAsset dialogueData;
    [SerializeField] private DialogueManager dialogueManager;

    void Start()
    {
        LoadOpeningDialogue();
    }

    void LoadOpeningDialogue()
    {
        if (dialogueData == null)
        {
            Debug.LogError("DialogueData CSV 파일이 연결되지 않음");
            return;
        }

        // CSV 파싱
        string[] lines = dialogueData.text.Split(new[] { "\r\n", "\n" }, System.StringSplitOptions.None);
        List<DialogueMeta> candidates = new List<DialogueMeta>();

        for (int i = 1; i < lines.Length; i++)
        {
            string[] cells = Regex.Split(lines[i], ",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");
            if (cells.Length < 3) continue;

            int id = int.Parse(cells[0]);
            string characterName = cells[1].Trim();
            string fileName = cells[2].Trim();

            if (id >= 70 && id <= 73)
                candidates.Add(new DialogueMeta(id, characterName, fileName));
        }

        if (candidates.Count == 0)
        {
            Debug.LogWarning("ID 70~73 구간에 해당하는 대화가 없습니다.");
            return;
        }

        // 무작위 선택
        var selected = candidates[Random.Range(0, candidates.Count)];

        // DialogueManager에 파일 로드 요청
        dialogueManager.LoadDialogueCSV(selected.fileName, selected.characterName);

        DialogueUI.Instance.ShowDialogue(1);
        
    }

    private class DialogueMeta
    {
        public int id;
        public string fileName;
        public string characterName;
        public DialogueMeta(int id, string characterName, string fileName)
        {
            this.id = id;
            this.fileName = fileName;
            this.characterName = characterName;
        }
    }
}