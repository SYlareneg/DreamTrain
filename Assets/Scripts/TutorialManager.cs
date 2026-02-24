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
    [SerializeField] GameObject roulette;
    [SerializeField] GameObject cost;
    [SerializeField] GameObject playerHealth;
    [SerializeField] GameObject enemyUI;
    [SerializeField] GameObject rouletteButton;
    [SerializeField] GameObject rouletteButtonCost;
    [SerializeField] GameObject endTurnButton;
    private InputSystem_Actions input;
    public static Action nextTutorial;
    public static Action nextTutorial_Button;
    public static Vector3 rouletteButtonPos = new Vector3(3.18f, -3.49f, 0f);
    public static Vector3 endTurnButtonPos = new Vector3(24.57f, -10.06f, 0f);
    public static Vector3 rightCardPos = new Vector3(12f, -13f, 0f);
    public static Vector3 playerTriggerPos = new Vector3(-14.5f, -12.3f, 0f);
    public static Vector3 enemyTriggerPos = new Vector3(22.57f, 8.3f, 0f);

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
        TurnManager.Inst.isLoading = true;
        tutorialStage = stage;
        tutorialTurn = turn;
        tutorialStep = step;
        if(stage == 0)
        {
            hideScreen.SetActive(false);
            tutorialBox.SetActive(true);
            tutorialText.text = "좋아! 이제 혼자서도 할 수 있겠지?\n행운을 빌어!";
            TurnManager.Inst.isLoading = false;
            DOTween.Sequence().AppendInterval(2f).AppendCallback(() =>
            {
                HideTutorialBox();
            }).Play();
            return;
        }
        if(stage == 1)
        {
            switch(step)
            {
                case 1:
                    Tooltip.showTooltipSignal = false;
                    tutorialScreen.SetActive(true);
                    tutorialText.text = "앨리스, 룰렛을 다루는 방법은 잊지 않았지?\n우선 공격 룰렛이 적 앞(12시 방향)에 오도록 룰렛을 회전시켜보자!";

                    var rouletteInstance = Instantiate(roulette, tutorialScreen.transform, true);
                    rouletteInstance.transform.localPosition = roulette.transform.localPosition;
                    rouletteInstance.transform.localScale = roulette.transform.localScale;
                    foreach(var sr in rouletteInstance.GetComponentsInChildren<SpriteRenderer>())
                    {
                        sr.sortingOrder += 299;
                    }
                    foreach(var tmpro in rouletteInstance.GetComponentsInChildren<TextMeshPro>())
                    {
                        tmpro.sortingOrder += 299;
                    }

                    SetNextTutorial_Card("2칸 회전", false, (card, enemyIdx) =>
                    {
                        Destroy(rouletteInstance);
                        Tooltip.showTooltipSignal = true;
                        ShowTutorialBox(1, 1, 2);
                    });
                    break;
                case 2:
                    Tooltip.showTooltipSignal = false;
                    tutorialScreen.SetActive(true);
                    tutorialText.text = "잘했어! 방금처럼 카드를 사용하려면 행동력이 필요해. 행동력은 매 턴 회복되지만, 행동력이 부족하면 카드를 사용할 수 없으니 주의하자!";

                    var costInstance = Instantiate(cost, tutorialScreen.transform, true);
                    costInstance.transform.localPosition = cost.transform.localPosition;
                    costInstance.transform.localScale = cost.transform.localScale;
                    foreach(var sr in costInstance.GetComponentsInChildren<SpriteRenderer>())
                    {
                        sr.sortingOrder += 300;
                    }
                    foreach(var tmpro in costInstance.GetComponentsInChildren<TextMeshPro>())
                    {
                        tmpro.sortingOrder += 300;
                    }
                    nextTutorial = () =>
                    {
                        Tooltip.showTooltipSignal = true;
                        HideTutorialBox();
                        Destroy(costInstance);
                        ShowTutorialBox(1, 1, 3);
                    };
                    break;
                case 3:
                    Tooltip.showTooltipSignal = false;
                    tutorialScreen.SetActive(true);
                    tutorialText.text = "이제 룰렛 중앙의 버튼을 눌러 룰렛의 힘을 발동시켜보자!";
                    SetNextTutorial_Roulette(false, () =>
                    {
                        Tooltip.showTooltipSignal = true;
                        ShowTutorialBox(1, 1, 4);
                    });
                    break;
                case 4:
                    Tooltip.showTooltipSignal = false;
                    tutorialScreen.SetActive(true);
                    tutorialText.text = "좋았어! 방금처럼 룰렛 중앙의 버튼을 누르면 12시 방향의 룰렛을 적에게, 6시 방향의 룰렛은 스스로에게 발동할 수 있어.";
                    nextTutorial = () =>
                    {
                        Tooltip.showTooltipSignal = true;
                        HideTutorialBox();
                        ShowTutorialBox(1, 1, 5);
                    };
                    break;
                case 5:
                    Tooltip.showTooltipSignal = false;
                    tutorialScreen.SetActive(true);
                    tutorialText.text = "룰렛을 발동하려면 행동력이 1개 필요해. 그리고 발동 후에는 룰렛이 시계방향으로 1칸 회전하게 되니 잘 기억해둬!";

                    costInstance = Instantiate(cost, tutorialScreen.transform, true);
                    costInstance.transform.localPosition = cost.transform.localPosition;
                    costInstance.transform.localScale = cost.transform.localScale;
                    foreach(var sr in costInstance.GetComponentsInChildren<SpriteRenderer>())
                    {
                        sr.sortingOrder += 300;
                    }
                    foreach(var tmpro in costInstance.GetComponentsInChildren<TextMeshPro>())
                    {
                        tmpro.sortingOrder += 300;
                    }
                    nextTutorial = () =>
                    {
                        Tooltip.showTooltipSignal = true;
                        HideTutorialBox();
                        Destroy(costInstance);
                        ShowTutorialBox(1, 1, 6);
                    };
                    break;
                case 6:
                    Tooltip.showTooltipSignal = false;
                    tutorialScreen.SetActive(true);
                    tutorialText.text = "자, 이번에는 상대의 행동에 대비해 볼까?";

                    var enemyActionInstance = Instantiate(EnemyManager.Inst.actionList[0].gameObject, tutorialScreen.transform, true);
                    enemyActionInstance.GetComponent<EnemyAction>().enabled = false;
                    enemyActionInstance.GetComponent<Tooltip>().forceTooltipEnable = true;
                    foreach(var sr in enemyActionInstance.GetComponentsInChildren<SpriteRenderer>())
                    {
                        sr.sortingOrder += 320;
                    }
                    foreach(var tmpro in enemyActionInstance.GetComponentsInChildren<TextMeshPro>())
                    {
                        tmpro.sortingOrder += 320;
                    }
                    nextTutorial = () =>
                    {
                        tutorialText.text = "상대는 공격을 준비하고 있네. 수비 룰렛을 앨리스 네 앞(6시 방향)에 위치시킨 채로 룰렛을 발동해보자.";
                        SetNextTutorial_Card("3칸 회전", false, (card, enemyIdx) =>
                        {
                            Destroy(enemyActionInstance);
                            SetNextTutorial_Roulette(false, () =>
                            {
                                Tooltip.showTooltipSignal = true;
                                ShowTutorialBox(1, 1, 7);
                            });
                        });
                    };
                    break;
                case 7:
                    Tooltip.showTooltipSignal = false;
                    tutorialScreen.SetActive(true);
                    tutorialText.text = "완벽해! 이제 턴을 종료하고 적이 공격하더라도 방어도가 공격으로 인한 피해를 막아줄거야.";

                    // var playerHealthInstance = Instantiate(playerHealth, tutorialScreen.transform, true);
                    // playerHealthInstance.transform.localPosition = playerHealth.transform.localPosition;
                    // playerHealthInstance.transform.localScale = playerHealth.transform.localScale;
                    // foreach(var sr in playerHealthInstance.GetComponentsInChildren<SpriteRenderer>())
                    // {
                    //     sr.sortingOrder += 300;
                    // }
                    // foreach(var tmpro in playerHealthInstance.GetComponentsInChildren<TextMeshPro>())
                    // {
                    //     tmpro.sortingOrder += 300;
                    // }
                    
                    SetNextTutorial_EndTurn(false, () =>
                    {
                        Tooltip.showTooltipSignal = true;
                        HideTutorialBox();
                        // Destroy(playerHealthInstance);
                    });
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
                            Tooltip.showTooltipSignal = false;
                            tutorialScreen.SetActive(true);
                            tutorialText.text = "앨리스, 혹시 트리거에 대한 것을 기억해?";
                            nextTutorial = () =>
                            {
                                Tooltip.showTooltipSignal = true;
                                tutorialScreen.SetActive(false);
                                ShowTutorialBox(2, 1, 2);
                            };
                            break;
                        case 2:
                            Tooltip.showTooltipSignal = false;
                            tutorialScreen.SetActive(true);
                            tutorialText.text = "꿈 세계의 존재들은 저마다 고유한 트리거 조건을 가지고 있어. 조건을 만족하면 트리거 상태가 돼서, 평소보다 더 위협적인 행동을 하기도 해.";
                            nextTutorial = () =>
                            {
                                Tooltip.showTooltipSignal = true;
                                tutorialScreen.SetActive(false);
                                ShowTutorialBox(2, 1, 3);
                            };
                            break;
                        case 3:
                            Tooltip.showTooltipSignal = false;
                            tutorialScreen.SetActive(true);
                            tutorialText.text = "상대의 트리거 조건은 체력 바 아래의 트리거 게이지를 관찰하면 확인할 수 있어.";

                            var enemyTriggerInstance = Instantiate(enemyUI, tutorialScreen.transform, true);
                            enemyTriggerInstance.transform.localPosition = enemyUI.transform.localPosition;
                            enemyTriggerInstance.transform.localScale = enemyUI.transform.localScale;
                            foreach(var sr in enemyTriggerInstance.GetComponentsInChildren<SpriteRenderer>())
                            {
                                sr.sortingOrder += 300;
                            }
                            foreach(var tmpro in enemyTriggerInstance.GetComponentsInChildren<TextMeshPro>())
                            {
                                tmpro.sortingOrder += 300;
                            }
                            foreach(var tooltip in enemyTriggerInstance.GetComponentsInChildren<Tooltip>())
                            {
                                tooltip.forceTooltipEnable = true;
                            }

                            GameObject pointer = Instantiate(pointerIcon, enemyTriggerPos, Utils.QI);
                            pointer.transform.SetParent(tutorialScreen.transform, true);
                            var pointerSR = pointer.GetComponentsInChildren<SpriteRenderer>();
                            foreach(var sr in pointerSR)
                            {
                                sr.sortingOrder = 400;
                            }
                            pointer.SetActive(true);

                            nextTutorial = () =>
                            {
                                Tooltip.showTooltipSignal = true;
                                tutorialScreen.SetActive(false);
                                Destroy(enemyTriggerInstance);
                                Destroy(pointer);
                                ShowTutorialBox(2, 1, 4);
                            };
                            break;
                        case 4:
                            Tooltip.showTooltipSignal = false;
                            tutorialScreen.SetActive(true);
                            tutorialText.text = "보아하니 상대는 다음 턴에 트리거되겠군. 이번 턴에는 앨리스 네 마음대로 해도 괜찮겠어.";
                            nextTutorial = () =>
                            {
                                Tooltip.showTooltipSignal = true;
                                TurnManager.Inst.isLoading = false;
                                tutorialStage = 0;
                                tutorialScreen.SetActive(false);
                                HideTutorialBox();
                            };
                            break;
                    }
                    break;
                case 2:
                    switch(step)
                    {
                        case 1:
                            Tooltip.showTooltipSignal = false;
                            tutorialScreen.SetActive(true);
                            tutorialText.text = "적이 트리거됐어. 강력한 공격을 준비하고 있는 것 같으니 조심해!";
                            nextTutorial = () =>
                            {
                                Tooltip.showTooltipSignal = true;
                                TurnManager.Inst.isLoading = false;
                                tutorialStage = 0;
                                tutorialScreen.SetActive(false);
                                HideTutorialBox();
                            };
                            break;
                    }
                    break;
                case 3:
                    switch(step)
                    {
                        case 1:
                            Tooltip.showTooltipSignal = false;
                            tutorialScreen.SetActive(true);
                            tutorialText.text = "적의 트리거 시간이 무사히 지나갔어. 이제 우리가 실력을 보여줄 차례야!";
                            nextTutorial = () =>
                            {
                                Tooltip.showTooltipSignal = true;
                                tutorialScreen.SetActive(false);
                                ShowTutorialBox(2, 3, 2);
                            };
                            break;
                        case 2:
                            Tooltip.showTooltipSignal = false;
                            tutorialScreen.SetActive(true);
                            tutorialText.text = "적 뿐만 아니라 앨리스 너도 트리거할 수 있어. 트리거 조건과 효과는 네가 장착한 꿈 조각에 따라 달라지게 돼.";

                            playerHealth.transform.SetParent(tutorialScreen.transform.parent, true);
                            playerHealth.transform.localPosition = playerHealth.transform.localPosition;
                            playerHealth.transform.localScale = playerHealth.transform.localScale;
                            foreach(var tooltip in playerHealth.GetComponentsInChildren<Tooltip>())
                            {
                                tooltip.forceTooltipEnable = true;
                            }

                            GameObject pointer = Instantiate(pointerIcon, playerTriggerPos, Utils.QI);
                            pointer.transform.SetParent(tutorialScreen.transform, true);
                            var pointerSR = pointer.GetComponentsInChildren<SpriteRenderer>();
                            foreach(var sr in pointerSR)
                            {
                                sr.sortingOrder = 400;
                            }
                            pointer.SetActive(true);

                            nextTutorial = () =>
                            {
                                Tooltip.showTooltipSignal = true;
                                tutorialScreen.SetActive(false);
                                Destroy(pointer);
                                ShowTutorialBox(2, 3, 3);
                            };
                            break;
                        case 3:
                            Tooltip.showTooltipSignal = false;
                            tutorialScreen.SetActive(true);
                            tutorialText.text = "이번에는 내가 힘을 조금 보태줄게. 아무 카드나 한 장 사용해봐.";

                            TurnManager.Inst.playerTriggerCnt = TurnManager.Inst.playerTriggerMaxCnt - 1;
                            SetNextTutorial_Card(CardManager.Inst.myCards[CardManager.Inst.myCards.Count - 1].item.name, false);
                            Action onRouletteTrigger = null;
                            onRouletteTrigger = () =>
                            {
                                Tooltip.showTooltipSignal = true;
                                tutorialScreen.SetActive(false);
                                ShowTutorialBox(2, 3, 4);
                                TurnManager.OnRouletteTrigger -= onRouletteTrigger;
                            };
                            TurnManager.OnRouletteTrigger += onRouletteTrigger;
                            break;
                        case 4:
                            Tooltip.showTooltipSignal = false;
                            tutorialScreen.SetActive(true);
                            tutorialText.text = "좋아! 룰렛을 트리거하는 데 성공했어. 이제 끝내볼까?";

                            SetNextTutorial_Roulette(false, () =>
                            {
                                Tooltip.showTooltipSignal = true;
                                tutorialScreen.SetActive(false);
                                HideTutorialBox();
                            });
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
                    Action<Card, int> onUseCard = null;
                    onUseCard = (card, enemyIdx) =>
                    {
                        ShowTutorialBox(1, 1, 2);
                        TurnManager.OnUseCard -= onUseCard;
                    };
                    TurnManager.OnUseCard += onUseCard;
                    break;
                case 2:
                    hideScreenTitle.text = "룰렛의 발동";
                    hideScreenText.text = "룰렛 중앙의 버튼을 클릭하면 행동력을 소모하여 룰렛의 효과가 발동합니다.\n\n적 앞(12시 방향)의 룰렛은 적에게, 앨리스 앞(6시 방향)의 룰렛은 앨리스에게 동시에 효과가 적용됩니다!\n\n또한, 효과가 발동한 후에는 룰렛이 시계방향으로 1칸 회전하게 되니 잘 기억해둡시다.";
                    
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
                            Action<Card, int> onUseCard = null;
                            onUseCard = (card, enemyIdx) =>
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
                            hideScreenText.text = "룰렛 버튼을 연속으로 누르면 연달아 부여되어 있는 룰렛의 효과를 연속으로 발동할 수 있습니다.";

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

    public void SetNextTutorial_Card(string cardName, bool isButton, Action<Card, int> onUseCard = null)
    {
        GameObject cardExample = Instantiate(CardManager.Inst.myCards.Find(c => c.item.name == cardName).gameObject, new Vector3(7.5f, -14f, 0f), Utils.QI);
        Destroy(cardExample.GetComponent<Card>());
        var cardSR = cardExample.GetComponentsInChildren<SpriteRenderer>();
        foreach(var sr in cardSR)
        {
            sr.color = new Color(sr.color.r * 0.7f, sr.color.g * 0.7f, sr.color.b * 0.7f, 1f);
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
            sr.sortingOrder = 400;
        }
        pointer.SetActive(false);

        Action<Card, int> setNextTutorial = null;
        setNextTutorial = (card, enemyIdx) =>
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
            HideTutorialBox();
            Sequence sequence = DOTween.Sequence();
            sequence.AppendInterval(1f).AppendCallback(() =>
            {
                onUseCard?.Invoke(card, enemyIdx);
            });
            sequence.Play();
        };
        TurnManager.OnUseCard += setNextTutorial;

        Action next = () =>
        {
            tutorialScreen.SetActive(true);
            CardManager.Inst.myCards[CardManager.Inst.myCards.Count - 1].GetComponent<Order>().SetOriginOrder(31);
            cardExample.GetComponent<Order>().SetOriginOrder(31);
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
            nextTutorial = null;
        }
    }

    public void SetNextTutorial_Roulette(bool isButton, Action onRouletteActivate = null)
    {
        int rBsortingOrder = rouletteButton.GetComponent<SpriteRenderer>().sortingOrder;
        int rBCsortingOrder = rouletteButtonCost.GetComponent<SpriteRenderer>().sortingOrder;

        GameObject pointer = Instantiate(pointerIcon, rouletteButtonPos, Utils.QI);
        pointer.transform.SetParent(tutorialScreen.transform, true);
        var pointerSR = pointer.GetComponentsInChildren<SpriteRenderer>();
        foreach(var sr in pointerSR)
        {
            sr.sortingOrder = 400;
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
            HideTutorialBox();
            Sequence sequence = DOTween.Sequence();
            sequence.AppendInterval(1.5f).AppendCallback(() =>
            {
                onRouletteActivate?.Invoke();
            });
            sequence.Play();
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
            nextTutorial = null;
        }
    }

    public void SetNextTutorial_EndTurn(bool isButton, Action onEndTurnActivate = null)
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
            sr.sortingOrder = 400;
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
            HideTutorialBox();
            onEndTurnActivate?.Invoke();
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
            nextTutorial = null;
        }
    }
    public void HideTutorialScreen()
    {
        hideScreen.SetActive(false);
    }
}
