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
    [SerializeField] GameObject tutorialScreen;
    [SerializeField] GameObject pointerIcon;
    [SerializeField] GameObject rouletteButton;
    [SerializeField] GameObject rouletteButtonCost;
    [SerializeField] GameObject endTurnButton;
    private InputSystem_Actions input;
    public static Action nextTutorial;
    public static Action nextTutorial_Button;
    public static Vector3 rouletteButtonPos = new Vector3(3.18f, -3.49f, 0f);
    public static Vector3 endTurnButtonPos = new Vector3(22.57f, -1.06f, 0f);
    public static Vector3 rightCardPos = new Vector3(12f, -13f, 0f);

    public int tutorialStage;
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
        Debug.Log("Show Next Tutorial");
        nextTutorial?.Invoke();
    }

    public void ShowNextTutorial_Button()
    {
        Debug.Log("Show Next Tutorial Button");
        HideTutorialScreen();
        nextTutorial_Button?.Invoke();
    }

    public void ShowTutorialBox(int stage, int turn, int step)
    {
        if(stage == 0)
        {
            hideScreen.SetActive(false);
            tutorialBox.SetActive(true);
            tutorialText.text = "좋아! 이제 혼자서도 할 수 있겠지?\n행운을 빌어!";
            nextTutorial = () =>
            {
                HideTutorialBox();
                TurnManager.Inst.isLoading = false;
            };
            return;
        }
        TurnManager.Inst.isLoading = true;
        tutorialStage = stage;
        tutorialTurn = turn;
        tutorialStep = step;
        if(stage == 1)
        {
            switch(step)
            {
                case 1:
                    tutorialText.text = "앨리스, 룰렛을 다루는 방법은 잊지 않았지?\n우선 공격 룰렛이 적 앞(12시 방향)에 오도록 룰렛을 회전시켜보자!";
                    nextTutorial = () =>
                    {
                        HideTutorialBox();
                        ShowTutorialScreen(1, 1, 1);
                    };
                    break;
                case 2:
                    tutorialText.text = "잘했어! 이제 공격 룰렛이 적 앞에 위치하고 있으니 룰렛을 발동시키면 적에게 공격을 할 수 있어.\n룰렛 중앙의 버튼을 눌러 룰렛의 힘을 발동시켜보자!";
                    nextTutorial = () =>
                    {
                        HideTutorialBox();
                        ShowTutorialScreen(1, 1, 2);
                    };
                    break;
                case 3:
                    tutorialText.text = "멋져! 룰렛을 발동시키니 적에게 공격이 들어갔어!";
                    nextTutorial = () =>
                    {
                        HideTutorialBox();
                        ShowTutorialBox(1, 1, 4);
                    };
                    break;
                case 4:
                    tutorialText.text = "잠깐, 그러고 보니 상대도 공격을 준비하고 있잖아?";
                    nextTutorial = () =>
                    {
                        HideTutorialBox();
                        ShowTutorialScreen(1, 1, 4);
                    };
                    break;
                case 5:
                    tutorialText.text = "상대의 공격을 방어하기 위해 방어 룰렛을 앨리스 앞(6시 방향)에 위치시키고 발동시켜보자!";
                    nextTutorial = () =>
                    {
                        HideTutorialBox();
                        SetNextTutorial_Card("3칸 회전", false);

                        Action<Card> onUseCard = null;
                        onUseCard = (card) =>
                        {
                            SetNextTutorial_Roulette(false);
                            Action onRouletteActivate = null;
                            onRouletteActivate = () =>
                            {
                                ShowTutorialBox(1, 1, 6);
                                TurnManager.OnRouletteActivate -= onRouletteActivate;
                            };
                            TurnManager.OnRouletteActivate += onRouletteActivate;
                            TurnManager.OnUseCard -= onUseCard;
                        };
                        TurnManager.OnUseCard += onUseCard;
                    };
                    break;
                case 6:
                    tutorialText.text = "완벽해! 이제 턴을 종료하고 적이 공격하더라도 방어도가 공격으로 인한 피해를 막아줄거야.";
                    nextTutorial = () =>
                    {
                        HideTutorialBox();
                        SetNextTutorial_EndTurn(false);
                        Action onPlayerTurnEnd = null;
                        onPlayerTurnEnd = () =>
                        {
                            TurnManager.OnPlayerTurnEnd -= onPlayerTurnEnd;
                        };
                        TurnManager.OnPlayerTurnEnd += onPlayerTurnEnd;
                    };
                    break;
            }
        }
        else if(stage == 2)
        {
            switch (turn)
            {
                case 1:
                    switch(step)
                    {
                        case 1:
                            tutorialText.text = "앨리스, 이제 몸은 좀 풀렸어?\n이번에 상대할 적은 이전 적과 달리 꽤나 강력한 것 같아.";
                            nextTutorial = () =>
                            {
                                HideTutorialBox();
                                ShowTutorialBox(2, 1, 2);
                            };
                            break;
                        case 2:
                            tutorialText.text = "적에게 맞서기 위해서는 우리도 룰렛의 힘을 더 이끌어 내야 해! 지금부터 그 방법을 알려줄게.";
                            nextTutorial = () =>
                            {
                                HideTutorialBox();
                                ShowTutorialScreen(2, 1, 2);
                            };
                            break;
                        case 3:
                            tutorialText.text = "방금 적 앞에 부여한 발톱 룰렛은 처음엔 강한 데미지를 주지만 발동할수록 약해지는 특징이 있어.";
                            nextTutorial = () =>
                            {
                                HideTutorialBox();
                                ShowTutorialBox(2, 1, 4);
                            };
                            break;
                        case 4:
                            tutorialText.text = "룰렛 버튼을 눌러서 발톱 룰렛을 발동시켜 보자!";
                            nextTutorial = () =>
                            {
                                HideTutorialBox();
                                SetNextTutorial_Roulette(false);
                                Action onRouletteActivate = null;
                                onRouletteActivate = () =>
                                {
                                    ShowTutorialBox(2, 1, 5);
                                    TurnManager.OnRouletteActivate -= onRouletteActivate;
                                };
                                TurnManager.OnRouletteActivate += onRouletteActivate;
                            };
                            break;
                        case 5:
                            tutorialText.text = "좋아! 발톱 룰렛이 적에게 큰 피해를 주었어!";
                            nextTutorial = () =>
                            {
                                HideTutorialBox();
                                ShowTutorialBox(2, 1, 6);
                            };
                            break;
                        case 6:
                            tutorialText.text = "하지만 이제 행동력이 부족해서 더이상 할 수 있는 행동이 없네... 턴을 종료해야겠어.";
                            nextTutorial = () =>
                            {
                                HideTutorialBox();
                                SetNextTutorial_EndTurn(false);
                                Action onPlayerTurnEnd = null;
                                onPlayerTurnEnd = () =>
                                {
                                    TurnManager.OnPlayerTurnEnd -= onPlayerTurnEnd;
                                };
                                TurnManager.OnPlayerTurnEnd += onPlayerTurnEnd;
                            };
                            break;
                    }
                    break;
                case 2:
                    switch(step)
                    {
                        case 1:
                            tutorialText.text = "이런! 상대가 제법 강한 공격을 준비하고 있어. 방어 룰렛을 이용해서 공격을 막아볼까?";
                            nextTutorial = () =>
                            {
                                HideTutorialBox();
                                SetNextTutorial_Card("1칸 회전", false);
                                Action<Card> onUseCard = null;
                                onUseCard = (card) =>
                                {
                                    Debug.Log("다음 튜토리얼로 이동: 2-2-2");
                                    ShowTutorialBox(2, 2, 2);
                                    TurnManager.OnUseCard -= onUseCard;
                                };
                                TurnManager.OnUseCard += onUseCard;
                            };
                            break;
                        case 2:
                            tutorialText.text = "좋아! 이제 방어 룰렛이 앨리스 앞에 위치하고 있으니 룰렛을 발동시키면 적의 공격을 막을 수 있어.\n하지만 한번 발동시키는 것만으론 공격을 완전히 막을 수 없을 것 같아...";
                            nextTutorial = () =>
                            {
                                HideTutorialBox();
                                ShowTutorialScreen(2, 2, 2);
                            };
                            break;
                        case 3:
                            tutorialText.text = "한번 더 룰렛을 발동시키면 적의 공격을 완벽히 막을 수 있을 것 같아!";
                            nextTutorial = () =>
                            {
                                HideTutorialBox();
                                SetNextTutorial_Roulette(false);
                                Action onRouletteActivate = null;
                                onRouletteActivate = () =>
                                {
                                    ShowTutorialBox(2, 2, 4);
                                    TurnManager.OnRouletteActivate -= onRouletteActivate;
                                };
                                TurnManager.OnRouletteActivate += onRouletteActivate;
                            };
                            break;
                        case 4:
                            tutorialText.text = "좋아! 이제 턴을 종료하고 적이 공격하더라도 방어도가 공격으로 인한 피해를 막아줄거야.";
                            nextTutorial = () =>
                            {
                                HideTutorialBox();
                                SetNextTutorial_EndTurn(false);
                                Action onPlayerTurnEnd = null;
                                onPlayerTurnEnd = () =>
                                {
                                    TurnManager.OnPlayerTurnEnd -= onPlayerTurnEnd;
                                };
                                TurnManager.OnPlayerTurnEnd += onPlayerTurnEnd;
                            };
                            break;
                    }
                    break;
            }
        }
        else if(stage == 3)
        {
            switch(turn)
            {
                case 1:
                    switch(step)
                    {
                        case 1:
                            tutorialText.text = "드디어 마지막 단계에 왔어! 지금까지 배운 것들을 활용하여 적을 물리쳐보자!";
                            nextTutorial = () =>
                            {
                                HideTutorialBox();
                                tutorialStage = 0;
                                TurnManager.Inst.isLoading = false;
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
                                ShowTutorialScreen(3, 3, 1);
                            };
                            break;
                        case 2:
                            tutorialText.text = "적의 강력한 공격을 막기 위해서 무언가 방법이 필요해... 숨기 카드를 활용해 볼까?";
                            nextTutorial = () =>
                            {
                                HideTutorialBox();
                                SetNextTutorial_Card("숨기", false);
                                Action<Card> onUseCard = null;
                                onUseCard = (card) =>
                                {
                                    ShowTutorialBox(3, 3, 3);
                                    TurnManager.OnUseCard -= onUseCard;
                                };
                                TurnManager.OnUseCard += onUseCard;
                            };
                            break;
                        case 3:
                            tutorialText.text = "좋았어! 숨기 카드를 사용해서 방어 룰렛을 우리 앞으로 가져왔어. 이제 룰렛을 발동시켜서 적의 공격을 막아보자!";
                            nextTutorial = () =>
                            {
                                HideTutorialBox();
                                SetNextTutorial_Roulette(false);
                                Action onRouletteActivate = null;
                                onRouletteActivate = () =>
                                {
                                    ShowTutorialBox(3, 3, 4);
                                    TurnManager.OnRouletteActivate -= onRouletteActivate;
                                };
                                TurnManager.OnRouletteActivate += onRouletteActivate;
                            };
                            break;
                        case 4:
                            tutorialText.text = "한번 더 발동시키면 적의 공격을 완벽히 막을 수 있을 것 같아!";
                            nextTutorial = () =>
                            {
                                HideTutorialBox();
                                SetNextTutorial_Roulette(false);
                                Action onRouletteActivate = null;
                                onRouletteActivate = () =>
                                {
                                    ShowTutorialBox(3, 3, 5);
                                    TurnManager.OnRouletteActivate -= onRouletteActivate;
                                };
                                TurnManager.OnRouletteActivate += onRouletteActivate;
                            };
                            break;
                        case 5:
                            tutorialText.text = "완벽해! 이제 턴을 종료하고 적이 공격하더라도 방어도가 공격으로 인한 피해를 막아줄거야.";
                            nextTutorial = () =>
                            {
                                HideTutorialBox();
                                SetNextTutorial_EndTurn(false);
                                Action onPlayerTurnEnd = null;
                                onPlayerTurnEnd = () =>
                                {
                                    TurnManager.OnPlayerTurnEnd -= onPlayerTurnEnd;
                                };
                                TurnManager.OnPlayerTurnEnd += onPlayerTurnEnd;
                            };
                            break;
                    }
                    break;
                case 4:
                    switch(step)
                    {
                        case 1:
                            tutorialText.text = "휴... 적의 트리거 상태를 무사히 막아냈어.\n이번엔 우리가 실력을 보여줄 차례야! 내가 힘을 보태 줄테니, 아무 카드나 사용해 봐.";
                            nextTutorial = () =>
                            {
                                HideTutorialBox();
                                TurnManager.Inst.playerTriggerCnt = 11;
                                SetNextTutorial_Card(CardManager.Inst.myCards[CardManager.Inst.myCards.Count - 1].item.name, false);
                                Action onRouletteTrigger = null;
                                onRouletteTrigger = () =>
                                {
                                    ShowTutorialScreen(3, 4, 1);
                                    TurnManager.OnRouletteTrigger -= onRouletteTrigger;
                                };
                                TurnManager.OnRouletteTrigger += onRouletteTrigger;
                            };
                            break;
                        case 2:
                            tutorialText.text = "좋아! 룰렛을 트리거시키는 데 성공했어. 우리도 적에게 한 방 먹여주자고!";
                            nextTutorial = () =>
                            {
                                HideTutorialBox();
                                SetNextTutorial_Roulette(false);
                                Action onRouletteActivate = null;
                                onRouletteActivate = () =>
                                {
                                    tutorialStage = 0;
                                    TurnManager.Inst.isLoading = false;
                                    TurnManager.OnRouletteActivate -= onRouletteActivate;
                                };
                                TurnManager.OnRouletteActivate += onRouletteActivate;
                            };
                            break;
                    }
                    break;
            }
        }
        tutorialBox.SetActive(true);
    }

    public void HideTutorialBox()
    {
        tutorialBox.SetActive(false);
        nextTutorial = null;
    }

    public void ShowTutorialScreen(int stage, int turn, int step)
    {
        hideScreen.SetActive(true);
        if(stage == 1)
        {
            switch(step)
            {
                case 1:
                    hideScreenTitle.text = "카드의 사용";
                    hideScreenText.text = "손패에서 카드를 클릭해 손패 밖으로 드래그하면 카드를 사용할 수 있습니다!";

                    SetNextTutorial_Card("2칸 회전", true);
                    Action<Card> onUseCard = null;
                    onUseCard = (card) =>
                    {
                        ShowTutorialBox(1, 1, 2);
                        TurnManager.OnUseCard -= onUseCard;
                    };
                    TurnManager.OnUseCard += onUseCard;
                    break;
                case 2:
                    hideScreenTitle.text = "룰렛의 발동";
                    hideScreenText.text = "룰렛 중앙의 버튼을 클릭하면 행동력을 소모하여 룰렛의 효과가 발동합니다.\n\n적 앞(12시 방향)의 룰렛은 적에게, 앨리스 앞(6시 방향)의 룰렛은 앨리스에게 효과를 적용합니다.\n\n룰렛은 항상 적과 앨리스에게 동시에 적용되니 주의하세요!";
                    
                    SetNextTutorial_Roulette(true);
                    Action onRouletteActivate = null;
                    onRouletteActivate = () =>
                    {
                        ShowTutorialBox(1, 1, 3);
                        TurnManager.OnRouletteActivate -= onRouletteActivate;
                    };
                    TurnManager.OnRouletteActivate += onRouletteActivate;
                    break;
                case 4:
                    hideScreenTitle.text = "적의 행동";
                    hideScreenText.text = "플레이어의 턴이 종료된 이후 적의 턴이 되면 적들이 행동을 시작합니다.\n\n적들이 수행하는 행동은 화면 왼쪽 위에 표시되니, 이를 잘 보고 적절하게 대응해봅시다!";

                    nextTutorial_Button = () =>
                    {
                        HideTutorialBox();
                        ShowTutorialBox(1, 1, 5);
                    };
                    break;
            }
        }
        else if(stage == 2)
        {
            switch(turn)
            {
                case 1:
                    switch(step)
                    {
                        case 2:
                            hideScreenTitle.text = "룰렛의 부여";
                            hideScreenText.text = "앨리스는 카드를 사용해 룰렛에 새로운 힘을 부여할 수 있습니다. 효과가 부여되는 위치는 카드마다 다릅니다.\n\n추가로, '발톱 세우기'를 비롯한 몇몇 카드는 효과 발동을 위해 대상이 될 적을 선택해야 합니다. 이는 추후 여러 적이 등장했을 때 유용하게 활용할 수 있습니다.";

                            SetNextTutorial_Card("발톱 세우기", true);
                            Action<Card> onUseCard = null;
                            onUseCard = (card) =>
                            {
                                ShowTutorialBox(2, 1, 3);
                                TurnManager.OnUseCard -= onUseCard;
                            };
                            TurnManager.OnUseCard += onUseCard;
                            break;
                    }
                    break;
                case 2:
                    switch(step)
                    {
                        case 2:
                            hideScreenTitle.text = "룰렛의 연속 발동";
                            hideScreenText.text = "룰렛 버튼을 연속으로 누르면 같은 룰렛의 효과를 연속으로 발동할 수 있습니다.\n\n다만, 룰렛을 발동할 때마다 다음 발동을 위해 필요한 행동력의 양이 증가하니 주의합시다!\n\n증가한 룰렛 발동 행동력은 룰렛을 회전시키면 다시 1로 초기화됩니다.";

                            SetNextTutorial_Roulette(true);
                            Action onRouletteActivate = null;
                            onRouletteActivate = () =>
                            {
                                ShowTutorialBox(2, 2, 3);
                                TurnManager.OnRouletteActivate -= onRouletteActivate;
                            };
                            TurnManager.OnRouletteActivate += onRouletteActivate;
                            break;
                    }
                    break;
            }
        }
        else if(stage == 3)
        {
            switch(turn)
            {
                case 3:
                    switch(step)
                    {
                        case 1:
                            hideScreenTitle.text = "적의 트리거";
                            hideScreenText.text = "적들은 특정한 조건을 만족하면 체력 게이지 아래의 트리거 게이지가 차며, 트리거 게이지가 모두 차면 트리거 상태가 됩니다.\n\n트리거 조건은 적의 초상화를 살펴보면 확인할 수 있습니다.\n\n트리거 상태가 된 적은 평소보다 강력한 행동을 수행할 수 있으니 주의합시다!";

                            nextTutorial_Button = () =>
                            {
                                HideTutorialBox();
                                ShowTutorialBox(3, 3, 2);
                            };
                            break;
                    }
                    break;
                case 4:
                    switch(step)
                    {
                        case 1:
                            hideScreenTitle.text = "앨리스의 트리거";
                            hideScreenText.text = "앨리스도 특정한 조건을 만족하면 체력 게이지 아래의 트리거 게이지가 차며, 트리거 게이지가 모두 차면 룰렛이 트리거 상태가 됩니다.\n\n트리거 조건과 효과는 앨리스가 장착한 첫 번째 꿈 조각에 따라 달라지며, 체력 게이지 옆의 꿈 조각 아이콘을 살펴보면 확인할 수 있습니다.";

                            nextTutorial_Button = () =>
                            {
                                HideTutorialBox();
                                ShowTutorialBox(3, 4, 2);
                            };
                            break;
                    }
                    break;
            }
        }
    }

    public void SetNextTutorial_Card(string cardName, bool isButton)
    {
        GameObject cardExample = Instantiate(CardManager.Inst.myCards.Find(c => c.item.name == cardName).gameObject, new Vector3(7.5f, -14f, 0f), Utils.QI);
        Destroy(cardExample.GetComponent<Card>());
        var cardSR = cardExample.GetComponentsInChildren<SpriteRenderer>();
        foreach(var sr in cardSR)
        {
            sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 0.7f);
        }
        cardExample.SetActive(false);
        int cardOrder = cardExample.GetComponent<Order>().originOrder;

        Sequence exampleCardSequence = DOTween.Sequence();
        exampleCardSequence.Append(cardExample.transform.DOMove(Vector3.zero, 0.5f).SetEase(Ease.OutQuad))
            .AppendInterval(0.5f)
            .AppendCallback(() =>
            {
                cardExample.transform.position = new Vector3(7.5f, -14f, 0f);
            }).SetLoops(-1);

        GameObject pointer = Instantiate(pointerIcon, rightCardPos, Utils.QI);
        pointer.transform.SetParent(tutorialScreen.transform, true);
        var pointerSR = pointer.GetComponentsInChildren<SpriteRenderer>();
        foreach(var sr in pointerSR)
        {
            sr.sortingOrder = 310;
        }
        pointer.SetActive(false);

        Action<Card> setNextTutorial = null;
        setNextTutorial = (card) =>
        {
            tutorialScreen.SetActive(false);
            CardManager.Inst.myCards[CardManager.Inst.myCards.Count - 1].GetComponent<Order>().SetOriginOrder(cardOrder);
            cardExample.GetComponent<Order>().SetOriginOrder(cardOrder);
            TurnManager.OnUseCard -= setNextTutorial;
            cardActivate = false;
            activateCardName = "";
            Destroy(cardExample);
            exampleCardSequence.Kill();
            Destroy(pointer);
        };
        TurnManager.OnUseCard += setNextTutorial;

        Action next = () =>
        {
            tutorialScreen.SetActive(true);
            CardManager.Inst.myCards[CardManager.Inst.myCards.Count - 1].GetComponent<Order>().SetOriginOrder(30);
            cardExample.GetComponent<Order>().SetOriginOrder(30);
            cardActivate = true;
            activateCardName = cardName;
            cardExample.SetActive(true);
            exampleCardSequence.Play();
            pointer.SetActive(true);
        };
        if(isButton)
        {
            nextTutorial_Button = next;
        }
        else
        {
            next.Invoke();
        }
    }

    public void SetNextTutorial_Roulette(bool isButton)
    {
        int rBsortingOrder = rouletteButton.GetComponent<SpriteRenderer>().sortingOrder;
        int rBCsortingOrder = rouletteButtonCost.GetComponent<SpriteRenderer>().sortingOrder;

        GameObject pointer = Instantiate(pointerIcon, rouletteButtonPos, Utils.QI);
        pointer.transform.SetParent(tutorialScreen.transform, true);
        var pointerSR = pointer.GetComponentsInChildren<SpriteRenderer>();
        foreach(var sr in pointerSR)
        {
            sr.sortingOrder = 310;
        }
        pointer.SetActive(false);

        Action setNextTutorial = null;
        setNextTutorial = () =>
        {
            tutorialScreen.SetActive(false);
            rouletteButton.GetComponent<SpriteRenderer>().sortingOrder = rBsortingOrder;
            rouletteButtonCost.GetComponent<SpriteRenderer>().sortingOrder = rBCsortingOrder;
            TurnManager.OnRouletteActivate -= setNextTutorial;
            rouletteActivate = false;
            Destroy(pointer);
        };
        TurnManager.OnRouletteActivate += setNextTutorial;
        Action next = () =>
        {
            tutorialScreen.SetActive(true);
            rouletteButton.GetComponent<SpriteRenderer>().sortingOrder = 310;
            rouletteButtonCost.GetComponent<SpriteRenderer>().sortingOrder = 311;
            rouletteActivate = true;
            pointer.SetActive(true);
        };
        if(isButton)
        {
            nextTutorial_Button = next;
        }
        else
        {
            next.Invoke();
        }
    }

    public void SetNextTutorial_EndTurn(bool isButton)
    {
        var newEndTurnBtn = Instantiate(endTurnButton, endTurnButton.transform.position, Utils.QI);
        newEndTurnBtn.transform.SetParent(tutorialScreen.transform, true);
        newEndTurnBtn.transform.SetAsLastSibling();
        newEndTurnBtn.transform.localScale = endTurnButton.transform.localScale;
        newEndTurnBtn.SetActive(false);

        GameObject pointer = Instantiate(pointerIcon, endTurnButtonPos, Utils.QI);
        pointer.transform.SetParent(tutorialScreen.transform, true);
        var pointerSR = pointer.GetComponentsInChildren<SpriteRenderer>();
        foreach(var sr in pointerSR)
        {
            sr.sortingOrder = 310;
        }
        pointer.SetActive(false);

        Action setNextTutorial = null;
        setNextTutorial = () =>
        {
            tutorialScreen.SetActive(false);
            Destroy(newEndTurnBtn);
            TurnManager.OnPlayerTurnEnd -= setNextTutorial;
            endTurnActivate = false;
            Destroy(pointer);
        };
        TurnManager.OnPlayerTurnEnd += setNextTutorial;
        Action next = () =>
        {
            tutorialScreen.SetActive(true);
            newEndTurnBtn.SetActive(true);
            endTurnActivate = true;
            pointer.SetActive(true);
        };
        if(isButton)
        {
            nextTutorial_Button = next;
        }
        else
        {
            next.Invoke();
        }
    }
    public void HideTutorialScreen()
    {
        hideScreen.SetActive(false);
    }
}
