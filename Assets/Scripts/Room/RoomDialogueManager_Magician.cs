using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.InputSystem;
using DG.Tweening;

public class RoomDialogueManager_Magician : RoomDialogueManager
{
    void Awake()
    {
        Inst = this;
        input = new InputSystem_Actions();
    }

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

    void Start()
    {
        if(DataManager.Inst.characterSO.bossClear == false)
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
                text = "앞에 무대로 이어지는 긴 길이 있다.",
                speakerSprite = null,
                sound = null
            };
            DialogueLine startLine3 = new DialogueLine()
            {
                speakerName = "",
                text = "무대 위에서 누군가 앨리스를 기다리고 있는 것 같다.",
                speakerSprite = null,
                sound = null
            };
            RoomPlayer.Inst.isInteractable = false;
            SceneChangeManager.Inst.SceneFadeIn(() => {
                currentDialogueLines = new List<DialogueLine>() { startLine1, startLine2, startLine3 };
                currentLineIndex = -1;
                RoomPlayer.Inst.isInteractable = false;
                ShowNextDialogue(new InputAction.CallbackContext());
            });
        }
        else
        {
            DialogueLine startLine1 = new DialogueLine()
            {
                speakerName = "",
                text = "마술사를 이겼다...!",
                speakerSprite = null,
                sound = null
            };
            DialogueLine startLine2 = new DialogueLine()
            {
                speakerName = "",
                text = "관객들은 모두 사라지고, 무대 뒤편의 커튼이 열렸다.",
                speakerSprite = null,
                sound = null
            };
            DialogueLine startLine3 = new DialogueLine()
            {
                speakerName = "",
                text = "왠지 모르게 무대 뒤편으로 가야만 할 것 같다.",
                speakerSprite = null,
                sound = null
            };
            RoomPlayer.Inst.isInteractable = false;
            SceneChangeManager.Inst.SceneFadeIn(() => {
                currentDialogueLines = new List<DialogueLine>() { startLine1, startLine2, startLine3 };
                currentLineIndex = -1;
                RoomPlayer.Inst.isInteractable = false;
                ShowNextDialogue(new InputAction.CallbackContext());
            });
        }
    }
}
