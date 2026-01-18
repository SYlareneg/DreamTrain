using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.InputSystem;
using DG.Tweening;
using System;

[System.Serializable]
public class DialogueLine
{
    public string speakerName;
    public string text;
    public Sprite speakerSprite;
    public AudioClip sound;
}

public class RoomDialogueManager : MonoBehaviour
{
    public static RoomDialogueManager Inst;
    void Awake()
    {
        Inst = this;
        input = new InputSystem_Actions();
    }

    [SerializeField] GameObject dialoguePanel;
    [SerializeField] GameObject dialogue;
    [SerializeField] float dialogueAnchorMaxPivotX = 0.79f;
    [SerializeField] GameObject dialogueSpeakerName;
    [SerializeField] GameObject dialogueText;
    [SerializeField] GameObject dialogueSpeakerSprite;
    [SerializeField] AudioSource dialogueAudioSource;
    [SerializeField] Image dialogueDone;

    public List<DialogueLine> currentDialogueLines;
    public int currentLineIndex = 0;

    private InputSystem_Actions input;
    Coroutine typingCoroutine;
    Sequence doneBlinkSequence;
    public static Action OnDialogueEnd;

    private void OnEnable()
    {
        input.Player.Enable();
        input.Player.Dialogue.performed += ShowNextDialogue;

        doneBlinkSequence = DOTween.Sequence()
            .Append(dialogueDone.DOFade(0.3f, 0.5f))
            .Append(dialogueDone.DOFade(1f, 0.5f))
            .SetLoops(-1);

        doneBlinkSequence.Pause();
    }


    private void OnDisable()
    {
        input.Player.Disable();
        input.Player.Dialogue.performed -= ShowNextDialogue;
        doneBlinkSequence.Kill();
        OnDialogueEnd = null;
    }
    
    public void ShowDialogue(DialogueLine line)
    {
        dialoguePanel.SetActive(true);
        dialogueSpeakerName.GetComponent<TMP_Text>().text = line.speakerName;
        dialogueSpeakerSprite.GetComponent<Image>().sprite = line.speakerSprite;
        if(line.speakerSprite == null)
        {
            dialogueSpeakerSprite.SetActive(false);
            dialogue.GetComponent<RectTransform>().anchorMax = new Vector2(1f, 0f);
        }
        else
        {
            dialogueSpeakerSprite.SetActive(true);
            dialogue.GetComponent<RectTransform>().anchorMax = new Vector2(dialogueAnchorMaxPivotX, 0f);
        }
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        } 
        typingCoroutine = StartCoroutine(ShowTextGradually(line.text, 0.02f));

        if(line.sound != null)
        {
            dialogueAudioSource.clip = line.sound;
            dialogueAudioSource.Play();
        }
    }

    public IEnumerator ShowTextGradually(string fullText, float delayPerCharacter)
    {
        dialogueText.GetComponent<TMP_Text>().text = fullText;
        dialogueText.GetComponent<TMP_Text>().maxVisibleCharacters = 0;
        doneBlinkSequence.Pause();
        dialogueDone.color = new Color(dialogueDone.color.r, dialogueDone.color.g, dialogueDone.color.b, 0.3f);
        foreach(char c in fullText)
        {
            dialogueText.GetComponent<TMP_Text>().maxVisibleCharacters++;
            yield return new WaitForSeconds(delayPerCharacter);
        }
        dialogueDone.color = new Color(dialogueDone.color.r, dialogueDone.color.g, dialogueDone.color.b, 1f);
        doneBlinkSequence.Restart();
        typingCoroutine = null;
    }

    public void HideDialogue()
    {
        dialoguePanel.SetActive(false);
        RoomPlayer.Inst.isInteractable = true;
        OnDialogueEnd?.Invoke();
    }

    public void ShowDialogueList(List<DialogueLine> lines)
    {
        if(lines == null || lines.Count == 0)
        {
            Debug.Log("No dialogue lines to show.");
            RoomPlayer.Inst.isInteractable = true;
            return;
        }

        currentDialogueLines = lines;
        currentLineIndex = 0;
        ShowDialogue(lines[0]);
    }

    public void SkipDialogue()
    {
        HideDialogue();
    }

    public void ShowNextDialogue(InputAction.CallbackContext context)
    {
        if(currentDialogueLines == null || currentDialogueLines.Count == 0)
        {
            return;
        }
        if(typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            dialogueText.GetComponent<TMP_Text>().maxVisibleCharacters = dialogueText.GetComponent<TMP_Text>().text.Length;
            dialogueDone.color = new Color(dialogueDone.color.r, dialogueDone.color.g, dialogueDone.color.b, 1f);
            doneBlinkSequence.Restart();
            typingCoroutine = null;
            return;
        }
        currentLineIndex++;
        if(currentDialogueLines.Count > 0 && currentLineIndex < currentDialogueLines.Count)
        {
            ShowDialogue(currentDialogueLines[currentLineIndex]);
        }
        else
        {
            HideDialogue();
            currentDialogueLines = null;
            currentLineIndex = 0;
        }
    }

    void Start()
    {
        DialogueLine startLine1 = new DialogueLine()
        {
            speakerName = "앨리스",
            text = "...여긴 어디지?",
            speakerSprite = null,
            sound = null
        };
        DialogueLine startLine2 = new DialogueLine()
        {
            speakerName = "",
            text = "주위를 둘러보는게 좋을 것 같다.",
            speakerSprite = null,
            sound = null
        };
        SceneChangeManager.Inst.SceneFadeIn(() => { 
            RoomPlayer.Inst.isInteractable = false;
            currentDialogueLines = new List<DialogueLine>() { startLine1, startLine2 };
            currentLineIndex = -1;
            ShowNextDialogue(new InputAction.CallbackContext());
        });
    }
}
