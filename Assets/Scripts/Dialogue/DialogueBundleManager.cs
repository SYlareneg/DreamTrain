using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueBundleManager : MonoBehaviour
{
    [Header("Dialogue Bundle List (5 total)")]
    [SerializeField] private List<HallControll.SO.DialogueBundle> dialogueBundles;

    [Header("UI References")]
    [SerializeField] private GameObject dialogueBundle;
    [SerializeField] private Button[] dialogueButtons;
    [SerializeField] private TextMeshProUGUI[] dialogueButtonTexts;

    private List<HallControll.SO.DialogueBundle> availableBundles = new();

    private void Awake()
    {
        dialogueBundle.SetActive(false);
    }

    public void ShowDialogueChoices()
    {
        availableBundles = dialogueBundles.FindAll(b => !b.isBanned);

        List<HallControll.SO.DialogueBundle> randomBundles = new();
        while (randomBundles.Count < 3 && availableBundles.Count > 0)
        {
            int index = Random.Range(0, availableBundles.Count);
            randomBundles.Add(availableBundles[index]);
            availableBundles.RemoveAt(index);
        }

        // 패널 표시 + 버튼 세팅
        dialogueBundle.SetActive(true);

        for (int i = 0; i < dialogueButtons.Length; i++)
        {
            if (i < randomBundles.Count)
            {
                var bundle = randomBundles[i];
                dialogueButtonTexts[i].text = bundle.bundleName;
                dialogueButtons[i].gameObject.SetActive(true);

                // 클릭 시 해당 소재의 대화 파일 로드
                dialogueButtons[i].onClick.RemoveAllListeners();
                dialogueButtons[i].onClick.AddListener(() => OnDialogueSelected(bundle));
            }
            else
            {
                dialogueButtons[i].gameObject.SetActive(false);
            }
        }
    }
    private void OnDialogueSelected(HallControll.SO.DialogueBundle selected)
    {
    }

// CSV 문자열에서 FileID로 FileName을 찾는 함수
    private string FindFileNameByID(string csvText, string targetID)
    {
        try
        {
            string[] lines = csvText.Split('\n');

            // 첫 줄이 헤더라고 가정 → i = 1부터 시작
            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (string.IsNullOrEmpty(line)) continue;

                string[] parts = line.Split(',');
                if (parts.Length < 2) continue;

                string id = parts[0].Trim();
                string fileName = parts[1].Trim();

                if (id == targetID)
                    return fileName;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[BundleManager] Error reading CSV content: {ex.Message}");
        }

        return null;
    }
}
