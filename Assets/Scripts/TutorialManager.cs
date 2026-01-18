using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using System;
using DG.Tweening;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Inst;
    private void Awake()
    {
        Inst = this;
        input = new InputSystem_Actions();
    }

    [SerializeField] GameObject tutorialBox;
    [SerializeField] TMP_Text tutorialText;
    [SerializeField] GameObject hideScreen;
    [SerializeField] TMP_Text hideScreenTitle;
    [SerializeField] TMP_Text hideScreenText;
    [SerializeField] GameObject pointerIcon;
    private InputSystem_Actions input;
    public static Action nextTutorial;
    public static Action nextTutorial_Button;
    public static Vector3 rouletteButtonPos = new Vector3(3.18f, -3.49f, 0f);
    public static Vector3 endTurnButtonPos = new Vector3(22.57f, -1.06f, 0f);
    public static Vector3 rightCardPos = new Vector3(12f, -13f, 0f);

    public int tutorialTurn;
    public int tutorialStep;
    public bool cardActivate = false;
    public string activateCardName = "";
    public bool rouletteActivate = false;
    public bool endTurnActivate = false;

    private void OnEnable()
    {
        input.Player.Enable();
        input.Player.Dialogue.performed += ShowNextTutorial;
    }

    private void OnDisable()
    {
        input.Player.Disable();
        input.Player.Dialogue.performed -= ShowNextTutorial;
    }

    public void ShowNextTutorial(InputAction.CallbackContext context)
    {
        nextTutorial?.Invoke();
    }

    public void ShowNextTutorial_Button()
    {
        HideTutorialScreen();
        nextTutorial = null;
        nextTutorial_Button?.Invoke();
        nextTutorial_Button = null;
        // TurnManager.Inst.isLoading = false;
    }

    public void ShowTutorialBox(int turn, int step)
    {
        TurnManager.Inst.isLoading = true;
        tutorialTurn = turn;
        tutorialStep = step;
        switch(turn)
        {
            case 1:
                switch(step)
                {
                    case 1:
                        tutorialText.text = "앨리스, 룰렛을 다루는 방법은 잊지 않았지?\n우선 공격 룰렛이 적 앞(12시 방향)에 오도록 룰렛을 회전시켜보자!";
                        nextTutorial = () =>
                        {
                            HideTutorialBox();
                            ShowTutorialScreen(1, 1);
                        };
                        break;
                    case 2:
                        tutorialText.text = "잘했어! 이제 공격 룰렛이 적 앞에 위치하고 있으니 룰렛을 발동시키면 적에게 공격을 할 수 있어.\n룰렛 중앙의 버튼을 눌러 룰렛의 힘을 발동시켜보자!";
                        nextTutorial = () =>
                        {
                            HideTutorialBox();
                            ShowTutorialScreen(1, 2);
                        };
                        break;
                    case 3:
                        tutorialText.text = "멋져! 룰렛을 발동시키니 적에게 공격이 들어갔어!\n하지만 이제 코스트를 모두 소모해서 더 이상 할 수 있는 행동이 없어. 턴 종료 버튼을 눌러 턴을 종료하자!";
                        nextTutorial = () =>
                        {
                            HideTutorialBox();
                            ShowTutorialScreen(1, 3);
                        };
                        break;
                }
                break;
            case 2:
                switch(step)
                {
                    case 1:
                        tutorialText.text = "이런! 상대가 공격을 준비하고 있어. 방어 룰렛을 이용해서 공격을 막아볼까?";
                        nextTutorial = () =>
                        {
                            HideTutorialBox();
                            ShowTutorialScreen(2, 1);
                        };
                        break;
                    case 2:
                        tutorialText.text = "좋아! 이제 턴을 종료하고 적이 공격하더라도 방어도가 공격으로 인한 피해를 막아줄거야.";
                        nextTutorial = () =>
                        {
                            HideTutorialBox();

                            GameObject pointer = Instantiate(pointerIcon, endTurnButtonPos, Utils.QI);
                            pointer.SetActive(true);

                            Action setNextTutorial_2_2 = null;
                            setNextTutorial_2_2 = () =>
                            {
                                TurnManager.OnPlayerTurnEnd -= setNextTutorial_2_2;
                                endTurnActivate = false;
                                Destroy(pointer);
                                nextTutorial = null;
                                nextTutorial_Button = null;
                            };
                            TurnManager.OnPlayerTurnEnd += setNextTutorial_2_2;
                            endTurnActivate = true;
                        };
                        break;
                }
                break;
            case 3:
                switch(step)
                {
                    case 1:
                        tutorialText.text = "이런! 적이 트리거 상태가 되었잖아!";
                        nextTutorial = () =>
                        {
                            HideTutorialBox();
                            ShowTutorialScreen(3, 1);
                        };
                        break;
                    case 2:
                        tutorialText.text = "더 많은 방어도를 올릴 방법이 필요해. 숨기 카드를 활용해보자.";
                        nextTutorial = () =>
                        {
                            HideTutorialBox();
                            nextTutorial = null;
                            nextTutorial_Button = null;

                            GameObject cardExample = Instantiate(CardManager.Inst.myCards.Find(c => c.item.name == "숨기").gameObject, new Vector3(7.5f, -14f, 0f), Utils.QI);
                            Destroy(cardExample.GetComponent<Card>());
                            Destroy(cardExample.GetComponent<Order>());
                            var cardSR = cardExample.GetComponentsInChildren<SpriteRenderer>();
                            foreach(var sr in cardSR)
                            {
                                sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 0.7f);
                            }
                            cardExample.SetActive(false);

                            Sequence exampleCardSequence = DOTween.Sequence();
                            exampleCardSequence.Append(cardExample.transform.DOMove(Vector3.zero, 0.5f).SetEase(Ease.OutQuad))
                                .AppendInterval(0.5f)
                                .AppendCallback(() =>
                                {
                                    cardExample.transform.position = new Vector3(7.5f, -14f, 0f);
                                }).SetLoops(-1);

                            GameObject pointer = Instantiate(pointerIcon, rightCardPos, Utils.QI);
                            pointer.SetActive(true);

                            cardActivate = true;
                            activateCardName = "숨기";
                            cardExample.SetActive(true);
                            exampleCardSequence.Play();

                            Action<Card> setNextTutorial_3_2_card = null;
                            setNextTutorial_3_2_card = (card) =>
                            {
                                TurnManager.OnUseCard -= setNextTutorial_3_2_card;
                                cardActivate = false;
                                activateCardName = "";
                                Destroy(cardExample);
                                exampleCardSequence.Kill();
                                Destroy(pointer);
                                pointer = Instantiate(pointerIcon, rouletteButtonPos, Utils.QI);
                                pointer.SetActive(true);
                                rouletteActivate = true;
                            };
                            TurnManager.OnUseCard += setNextTutorial_3_2_card;

                            Action setNextTutorial_3_2_roulette = null;
                            int tempCounter = 0;
                            setNextTutorial_3_2_roulette = () =>
                            {
                                tempCounter++;
                                if (tempCounter < 2) return;
                                HideTutorialBox();
                                TurnManager.OnRouletteActivate -= setNextTutorial_3_2_roulette;
                                rouletteActivate = false;
                                Destroy(pointer);
                                pointer = Instantiate(pointerIcon, endTurnButtonPos, Utils.QI);
                                pointer.SetActive(true);
                                endTurnActivate = true;
                            };
                            TurnManager.OnRouletteActivate += setNextTutorial_3_2_roulette;

                            Action setNextTutorial_3_2_endTurn = null;
                            setNextTutorial_3_2_endTurn = () =>
                            {
                                TurnManager.OnPlayerTurnEnd -= setNextTutorial_3_2_endTurn;
                                Destroy(pointer);
                                endTurnActivate = false;
                            };
                            TurnManager.OnPlayerTurnEnd += setNextTutorial_3_2_endTurn;
                        };
                        break;
                }
                break;
            case 4:
                switch(step)
                {
                    case 1:
                        tutorialText.text = "이번엔 우리가 실력을 보여줄 차례야. 아무 카드나 사용해서 트리거를 발동시켜봐!";
                        nextTutorial = () =>
                        {
                            HideTutorialBox();
                            ShowTutorialScreen(4, 1);
                        };
                        break;
                    case 2:
                        tutorialText.text = "좋아! 이제 끝을 내볼까?";
                        nextTutorial = () =>
                        {
                            HideTutorialBox();
                            nextTutorial = null;
                            nextTutorial_Button = null;

                            GameObject pointer = Instantiate(pointerIcon, rouletteButtonPos, Utils.QI);
                            pointer.SetActive(true);

                            Action setNextTutorial_4_2 = null;
                            setNextTutorial_4_2 = () =>
                            {
                                TurnManager.OnRouletteActivate -= setNextTutorial_4_2;
                                rouletteActivate = false;
                                Destroy(pointer);
                                DataManager.Inst.characterSO.isTutorial = false;
                                TurnManager.Inst.isLoading = false;
                            };
                            TurnManager.OnRouletteActivate += setNextTutorial_4_2;
                            rouletteActivate = true;
                        };
                        break;
                }
                break;
            default:
                tutorialText.text = "튜토리얼이 끝났습니다. 행운을 빕니다!";
                hideScreen.SetActive(false);
                break;
        }
        tutorialBox.SetActive(true);
    }

    public void HideTutorialBox()
    {
        tutorialBox.SetActive(false);
    }

    public void ShowTutorialScreen(int turn, int step)
    {
        hideScreen.SetActive(true);
        switch(turn)
        {
            case 1:
                switch(step)
                {
                    case 1:
                        hideScreenTitle.text = "카드의 사용";
                        hideScreenText.text = "손패에서 카드를 클릭해 손패 밖으로 드래그하면 카드를 사용할 수 있습니다!";

                        GameObject cardExample = Instantiate(CardManager.Inst.myCards.Find(c => c.item.name == "회전 카드 2").gameObject, new Vector3(7.5f, -14f, 0f), Utils.QI);
                        Destroy(cardExample.GetComponent<Card>());
                        Destroy(cardExample.GetComponent<Order>());
                        var cardSR = cardExample.GetComponentsInChildren<SpriteRenderer>();
                        foreach(var sr in cardSR)
                        {
                            sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 0.7f);
                        }
                        cardExample.SetActive(false);

                        Sequence exampleCardSequence = DOTween.Sequence();
                        exampleCardSequence.Append(cardExample.transform.DOMove(Vector3.zero, 0.5f).SetEase(Ease.OutQuad))
                            .AppendInterval(0.5f)
                            .AppendCallback(() =>
                            {
                                cardExample.transform.position = new Vector3(7.5f, -14f, 0f);
                            }).SetLoops(-1);

                        GameObject pointer = Instantiate(pointerIcon, rightCardPos, Utils.QI);
                        pointer.SetActive(false);

                        Action<Card> setNextTutorial_1_1 = null;
                        setNextTutorial_1_1 = (card) =>
                        {
                            ShowTutorialBox(1, 2);
                            TurnManager.OnUseCard -= setNextTutorial_1_1;
                            cardActivate = false;
                            activateCardName = "";
                            Destroy(cardExample);
                            exampleCardSequence.Kill();
                            Destroy(pointer);
                        };
                        TurnManager.OnUseCard += setNextTutorial_1_1;

                        nextTutorial_Button = () =>
                        {
                            cardActivate = true;
                            activateCardName = "회전 카드 2";
                            cardExample.SetActive(true);
                            exampleCardSequence.Play();
                            pointer.SetActive(true);
                        };
                        break;
                    case 2:
                        hideScreenTitle.text = "룰렛의 발동";
                        hideScreenText.text = "룰렛 중앙의 버튼을 클릭하면 룰렛의 효과가 발동합니다.\n\n적 앞(12시 방향)의 룰렛은 적에게, 앨리스 앞(6시 방향)의 룰렛은 앨리스에게 효과를 적용합니다.\n\n룰렛을 발동하는 데에는 코스트가 소모되며, 같은 룰렛을 연속으로 발동하려면 더 많은 코스트가 필요합니다.";
                        
                        pointer = Instantiate(pointerIcon, rouletteButtonPos, Utils.QI);
                        pointer.SetActive(false);

                        Action setNextTutorial_1_2 = null;
                        int tempCounter = 0;
                        setNextTutorial_1_2 = () =>
                        {
                            tempCounter++;
                            if (tempCounter < 2) return;
                            ShowTutorialBox(1, 3);
                            TurnManager.OnRouletteActivate -= setNextTutorial_1_2;
                            rouletteActivate = false;
                            Destroy(pointer);
                        };
                        TurnManager.OnRouletteActivate += setNextTutorial_1_2;
                        nextTutorial_Button = () =>
                        {
                            rouletteActivate = true;
                            pointer.SetActive(true);
                        };
                        break;
                    case 3:
                        hideScreenTitle.text = "코스트, 턴 종료";
                        hideScreenText.text = "룰렛을 발동하거나 카드를 사용하면 코스트가 소모되며, 코스트가 부족하면 더 이상 행동할 수 없습니다.\n\n코스트를 모두 소모하여 턴을 종료할 준비가 됐다면, 턴 종료 버튼을 눌러 턴을 종료하세요!";

                        pointer = Instantiate(pointerIcon, endTurnButtonPos, Utils.QI);
                        pointer.SetActive(false);

                        Action setNextTutorial_1_3 = null;
                        setNextTutorial_1_3 = () =>
                        {
                            TurnManager.OnPlayerTurnEnd -= setNextTutorial_1_3;
                            endTurnActivate = false;
                            Destroy(pointer);
                        };
                        TurnManager.OnPlayerTurnEnd += setNextTutorial_1_3;
                        nextTutorial_Button = () =>
                        {
                            endTurnActivate = true;
                            pointer.SetActive(true);
                        };
                        break;
                }
                break;
            case 2:
                switch(step)
                {
                    case 1:
                        hideScreenTitle.text = "방어 룰렛 사용";
                        hideScreenText.text = "적의 공격을 막기 위해 방어 룰렛을 사용해보세요!\n\n방어 룰렛을 앨리스 앞에 위치시키고 발동시켜 적의 공격을 막아낼 수 있습니다.\n\n회전 카드 3을 사용하여 방어 룰렛을 앨리스 앞에 위치시켜 보세요!";

                        GameObject cardExample = Instantiate(CardManager.Inst.myCards.Find(c => c.item.name == "회전 카드 3").gameObject, new Vector3(7.5f, -14f, 0f), Utils.QI);
                        Destroy(cardExample.GetComponent<Card>());
                        Destroy(cardExample.GetComponent<Order>());
                        var cardSR = cardExample.GetComponentsInChildren<SpriteRenderer>();
                        foreach(var sr in cardSR)
                        {
                            sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 0.7f);
                        }
                        cardExample.SetActive(false);

                        Sequence exampleCardSequence = DOTween.Sequence();
                        exampleCardSequence.Append(cardExample.transform.DOMove(Vector3.zero, 0.5f).SetEase(Ease.OutQuad))
                            .AppendInterval(0.5f)
                            .AppendCallback(() =>
                            {
                                cardExample.transform.position = new Vector3(7.5f, -14f, 0f);
                            }).SetLoops(-1);

                        GameObject pointer = Instantiate(pointerIcon, rightCardPos, Utils.QI);
                        pointer.SetActive(false);

                        nextTutorial_Button = () =>
                        {
                            cardActivate = true;
                            activateCardName = "회전 카드 3";
                            cardExample.SetActive(true);
                            exampleCardSequence.Play();
                            pointer.SetActive(true);
                        };

                        Action<Card> setNextTutorial_2_1_card = null;
                        setNextTutorial_2_1_card = (card) =>
                        {
                            TurnManager.OnUseCard -= setNextTutorial_2_1_card;
                            cardActivate = false;
                            activateCardName = "";
                            Destroy(cardExample);
                            exampleCardSequence.Kill();
                            Destroy(pointer);
                            pointer = Instantiate(pointerIcon, rouletteButtonPos, Utils.QI);
                            pointer.SetActive(true);
                            rouletteActivate = true;
                        };

                        Action setNextTutorial_2_1_roulette = null;
                        int tempCounter = 0;
                        setNextTutorial_2_1_roulette = () =>
                        {
                            tempCounter++;
                            if (tempCounter < 2) return;
                            ShowTutorialBox(2, 2);
                            TurnManager.OnRouletteActivate -= setNextTutorial_2_1_roulette;
                            rouletteActivate = false;
                            Destroy(pointer);
                        };
                        
                        TurnManager.OnRouletteActivate += setNextTutorial_2_1_roulette;
                        TurnManager.OnUseCard += setNextTutorial_2_1_card;
                        break;
                }
                break;
            case 3:
                switch(step)
                {
                    case 1:
                        hideScreenTitle.text = "적의 트리거";
                        hideScreenText.text = "적들은 특정한 조건을 만족하면 트리거 상태가 됩니다.\n\n트리거 상태가 된 적은 평소보다 강력한 행동을 사용할 수 있으니 주의해야 합니다.";
                        nextTutorial_Button = () =>
                        {
                            ShowTutorialBox(3, 2);
                        };
                        break;
                }
                break;
            case 4:
                switch(step)
                {
                    case 1:
                        hideScreenTitle.text = "앨리스의 트리거";
                        hideScreenText.text = "앨리스 또한 특정한 조건을 만족하면 룰렛을 트리거시킬 수 있습니다. 앨리스가 장착한 꿈 조각에 따라 트리거의 조건과 효과가 달라집니다!";
                        
                        nextTutorial_Button = () =>
                        {
                            GameObject cardExample = Instantiate(CardManager.Inst.myCards[CardManager.Inst.myCards.Count - 1].gameObject, new Vector3(7.5f, -14f, 0f), Utils.QI);
                            Destroy(cardExample.GetComponent<Card>());
                            Destroy(cardExample.GetComponent<Order>());
                            var cardSR = cardExample.GetComponentsInChildren<SpriteRenderer>();
                            foreach(var sr in cardSR)
                            {
                                sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 0.7f);
                            }
                            cardExample.SetActive(false);

                            Sequence exampleCardSequence = DOTween.Sequence();
                            exampleCardSequence.Append(cardExample.transform.DOMove(Vector3.zero, 0.5f).SetEase(Ease.OutQuad))
                                .AppendInterval(0.5f)
                                .AppendCallback(() =>
                                {
                                    cardExample.transform.position = new Vector3(7.5f, -14f, 0f);
                                }).SetLoops(-1);

                            GameObject pointer = Instantiate(pointerIcon, rightCardPos, Utils.QI);
                            pointer.SetActive(false);

                            cardActivate = true;
                            activateCardName = "tutorial_allcards";
                            cardExample.SetActive(true);
                            exampleCardSequence.Play();
                            pointer.SetActive(true);

                            Action<Card> setNextTutorial_4_1_card = null;
                            setNextTutorial_4_1_card = (card) =>
                            {
                                TurnManager.OnUseCard -= setNextTutorial_4_1_card;
                                cardActivate = false;
                                activateCardName = "";
                                Destroy(cardExample);
                                exampleCardSequence.Kill();
                                Destroy(pointer);
                                ShowTutorialBox(4, 2);
                            };
                            TurnManager.OnUseCard += setNextTutorial_4_1_card;
                        };
                        break;
                }
                break;
            default:
                hideScreenTitle.text = "튜토리얼 완료!";
                hideScreenText.text = "튜토리얼이 끝났습니다. 행운을 빕니다!";
                break;
        }
    }

    public void HideTutorialScreen()
    {
        hideScreen.SetActive(false);
    }
}
