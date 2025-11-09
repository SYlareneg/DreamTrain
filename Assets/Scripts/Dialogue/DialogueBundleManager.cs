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
        Debug.Log($"[BundleManager] Selected dialogue: {selected.bundleName} (FileID: {selected.connectedFileID})");
        dialogueBundle.SetActive(false);

        DialogueManager.Instance.LoadDialogueCSV(selected.connectedFileID.ToString(), "Vampire");
        DialogueUI.Instance.ShowDialogue(1);
    }
}
