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
        dialogueSpeakerName.GetComponentInChildren<TMP_Text>().text = line.speakerName;
        if(line.speakerName == null || line.speakerName == "")
        {
            dialogueSpeakerName.SetActive(false);
        }
        else
        {
            dialogueSpeakerName.SetActive(true);
        }
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        } 
        typingCoroutine = StartCoroutine(ShowTextGradually(line.text, 0.02f));
        dialogueSpeakerSprite.GetComponent<Image>().sprite = line.speakerSprite;
        if(line.speakerSprite == null)
        {
            dialogueSpeakerSprite.SetActive(false);
        }
        else
        {
            dialogueSpeakerSprite.SetActive(true);
        }

        if(line.sound != null)
        {
            dialogueAudioSource.clip = line.sound;
            dialogueAudioSource.Play();
        }
    }

    public IEnumerator ShowTextGradually(string fullText, float delayPerCharacter)
    {
        dialogueText.GetComponentInChildren<TMP_Text>().text = fullText;
        dialogueText.GetComponentInChildren<TMP_Text>().maxVisibleCharacters = 0;
        doneBlinkSequence.Pause();
        dialogueDone.color = new Color(dialogueDone.color.r, dialogueDone.color.g, dialogueDone.color.b, 0.3f);
        foreach(char c in fullText)
        {
            dialogueText.GetComponentInChildren<TMP_Text>().maxVisibleCharacters++;
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
            dialogueText.GetComponentInChildren<TMP_Text>().maxVisibleCharacters = dialogueText.GetComponentInChildren<TMP_Text>().text.Length;
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
}
