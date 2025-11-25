using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using HallControll.SO;

public class DialogueManager : DialogueManagerBase
{
    [Header("Button Mode Specific")]
    public GameObject dialogueBundlePanel;
    public Button[] dialogueButtons;   
    public GameObject rerollButton;

    // 부모의 추상 메서드 구현
    public override void ShowDialogueSelectionPanel()
    {
        // 대화 UI 끄고, 선택 패널 켜기
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (dialogueBundlePanel != null) dialogueBundlePanel.SetActive(true);

        // 번들 랜덤 선택
        List<DialogueBundle> available = dialogueBundles.FindAll(b => !b.isBanned);
        List<DialogueBundle> selected = new List<DialogueBundle>();

        int count = Mathf.Min(3, available.Count);
        while (selected.Count < count)
        {
            int idx = Random.Range(0, available.Count);
            selected.Add(available[idx]);
            available.RemoveAt(idx);
        }

        // 버튼 세팅
        for (int i = 0; i < dialogueButtons.Length; i++)
        {
            if (i < selected.Count)
            {
                DialogueBundle bundle = selected[i];
                dialogueButtons[i].gameObject.SetActive(true);
                dialogueButtons[i].GetComponentInChildren<TMPro.TextMeshProUGUI>().text = bundle.bundleName;

                // 클릭 리스너 재설정
                dialogueButtons[i].onClick.RemoveAllListeners();
                dialogueButtons[i].onClick.AddListener(() =>
                {
                    StartDialogueByBundle(bundle);
                });
            }
            else
            {
                dialogueButtons[i].gameObject.SetActive(false);
            }
        }

        if (rerollButton != null) rerollButton.SetActive(true);
    }

    private void StartDialogueByBundle(DialogueBundle bundle)
    {
        if (dialogueBundlePanel != null) dialogueBundlePanel.SetActive(false);
        if (rerollButton != null) rerollButton.SetActive(false);

        // 데이터 로드 (없으면 Resources에서 찾기)
        if (dialogueDataCSV == null) dialogueDataCSV = Resources.Load<TextAsset>("Dialogues/DialogueData");

        if (dialogueDataCSV == null)
        {
            Debug.LogError("[DialogueManager] DialogueData.csv is missing.");
            return;
        }

        (string character, string fileName) = FindCharacterAndFileName(dialogueDataCSV.text, bundle.connectedFileID.ToString());

        if (!string.IsNullOrEmpty(fileName))
        {
            StartDialogue(DialogueMode.Main, fileName, character);
        }
        else
        {
            Debug.LogError($"[DialogueManager] Info not found for FileID: {bundle.connectedFileID}");
        }
    }
}