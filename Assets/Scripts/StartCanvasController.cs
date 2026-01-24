using UnityEngine;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine.InputSystem;

public class StartCanvasController : MonoBehaviour
{
    public static StartCanvasController Inst;
    [SerializeField] GameObject newGameButton;
    [SerializeField] GameObject loadGameButton;
    [SerializeField] GameObject optionButton;
    [SerializeField] GameObject exitButton;
    [SerializeField] GameObject selector;

    private InputSystem_Actions input;

    void Awake()
    {
        Inst = this;
        input = new InputSystem_Actions();
    }

    void OnEnable()
    {
        input.Player.Enable();
        input.Player.Previous.performed += (ctx) =>
        {
            if(selector.transform.position == newGameButton.transform.position)
            {
                selector.transform.DOMove(exitButton.transform.position, 0.2f);
                newGameButton.GetComponentInChildren<TMP_Text>().color = new Color32(100, 100, 100, 255);
                loadGameButton.GetComponentInChildren<TMP_Text>().color = new Color32(100, 100, 100, 255);
                optionButton.GetComponentInChildren<TMP_Text>().color = new Color32(100, 100, 100, 255);
                exitButton.GetComponentInChildren<TMP_Text>().color = Color.white;
            }
            else if(selector.transform.position == loadGameButton.transform.position)
            {
                selector.transform.DOMove(newGameButton.transform.position, 0.2f);
                newGameButton.GetComponentInChildren<TMP_Text>().color = Color.white;
                loadGameButton.GetComponentInChildren<TMP_Text>().color = new Color32(100, 100, 100, 255);
                optionButton.GetComponentInChildren<TMP_Text>().color = new Color32(100, 100, 100, 255);
                exitButton.GetComponentInChildren<TMP_Text>().color = new Color32(100, 100, 100, 255);
            }
            else if(selector.transform.position == optionButton.transform.position)
            {
                selector.transform.DOMove(loadGameButton.transform.position, 0.2f);
                newGameButton.GetComponentInChildren<TMP_Text>().color = new Color32(100, 100, 100, 255);
                loadGameButton.GetComponentInChildren<TMP_Text>().color = Color.white;
                optionButton.GetComponentInChildren<TMP_Text>().color = new Color32(100, 100, 100, 255);
                exitButton.GetComponentInChildren<TMP_Text>().color = new Color32(100, 100, 100, 255);
            }
            else if(selector.transform.position == exitButton.transform.position)
            {
                selector.transform.DOMove(optionButton.transform.position, 0.2f);
                newGameButton.GetComponentInChildren<TMP_Text>().color = new Color32(100, 100, 100, 255);
                loadGameButton.GetComponentInChildren<TMP_Text>().color = new Color32(100, 100, 100, 255);
                optionButton.GetComponentInChildren<TMP_Text>().color = Color.white;
                exitButton.GetComponentInChildren<TMP_Text>().color = new Color32(100, 100, 100, 255);
            }
        };
        input.Player.Next.performed += (ctx) =>
        {
            if(selector.transform.position == newGameButton.transform.position)
            {
                selector.transform.DOMove(loadGameButton.transform.position, 0.2f);
                newGameButton.GetComponentInChildren<TMP_Text>().color = new Color32(100, 100, 100, 255);
                loadGameButton.GetComponentInChildren<TMP_Text>().color = Color.white;
                optionButton.GetComponentInChildren<TMP_Text>().color = new Color32(100, 100, 100, 255);
                exitButton.GetComponentInChildren<TMP_Text>().color = new Color32(100, 100, 100, 255);
            }
            else if(selector.transform.position == loadGameButton.transform.position)
            {
                selector.transform.DOMove(optionButton.transform.position, 0.2f);
                newGameButton.GetComponentInChildren<TMP_Text>().color = new Color32(100, 100, 100, 255);
                loadGameButton.GetComponentInChildren<TMP_Text>().color = new Color32(100, 100, 100, 255);
                optionButton.GetComponentInChildren<TMP_Text>().color = Color.white;
                exitButton.GetComponentInChildren<TMP_Text>().color = new Color32(100, 100, 100, 255);
            }
            else if(selector.transform.position == optionButton.transform.position)
            {
                selector.transform.DOMove(exitButton.transform.position, 0.2f);
                newGameButton.GetComponentInChildren<TMP_Text>().color = new Color32(100, 100, 100, 255);
                loadGameButton.GetComponentInChildren<TMP_Text>().color = new Color32(100, 100, 100, 255);
                optionButton.GetComponentInChildren<TMP_Text>().color = new Color32(100, 100, 100, 255);
                exitButton.GetComponentInChildren<TMP_Text>().color = Color.white;
            }
            else if(selector.transform.position == exitButton.transform.position)
            {
                selector.transform.DOMove(newGameButton.transform.position, 0.2f);
                newGameButton.GetComponentInChildren<TMP_Text>().color = Color.white;
                loadGameButton.GetComponentInChildren<TMP_Text>().color = new Color32(100, 100, 100, 255);
                optionButton.GetComponentInChildren<TMP_Text>().color = new Color32(100, 100, 100, 255);
                exitButton.GetComponentInChildren<TMP_Text>().color = new Color32(100, 100, 100, 255);
            }
        };
        input.Player.EnterRoom.performed += (ctx) =>
        {
            if(selector.transform.position == newGameButton.transform.position)
            {
                OnNewGameClicked();
            }
            else if(selector.transform.position == loadGameButton.transform.position)
            {
                OnLoadGameClicked();
            }
            else if(selector.transform.position == optionButton.transform.position)
            {
                OnOptionClicked();
            }
            else if(selector.transform.position == exitButton.transform.position)
            {
                OnExitClicked();
            }
        };
    }

    void OnDisable()
    {
        input.Player.Disable();
    }

    IEnumerator Start()
    {
        yield return new WaitForEndOfFrame();
        selector.SetActive(true);
        selector.transform.position = newGameButton.transform.position;
        newGameButton.GetComponentInChildren<TMP_Text>().color = Color.white;
        loadGameButton.GetComponentInChildren<TMP_Text>().color = new Color32(100, 100, 100, 255);
        optionButton.GetComponentInChildren<TMP_Text>().color = new Color32(100, 100, 100, 255);
        exitButton.GetComponentInChildren<TMP_Text>().color = new Color32(100, 100, 100, 255);
    }

    public void OnNewGameClicked()
    {
        if(selector.transform.position == newGameButton.transform.position)
        {
            Sequence buttonBlink = DOTween.Sequence();
            buttonBlink.Append(newGameButton.GetComponentInChildren<TMP_Text>().DOColor(new Color32(255, 255, 255, 0), 0.1f));
            buttonBlink.Append(newGameButton.GetComponentInChildren<TMP_Text>().DOColor(Color.white, 0.1f));
            buttonBlink.SetLoops(2);
            StartCoroutine(DataManager.Inst.LoadPlayerData(true));
            buttonBlink.OnComplete(() =>
            {
                SceneChangeManager.Inst.SceneFadeOut("RoomScene");
            });
        }
        else
        {
            SelectNewGame();
        }
    }

    public void SelectNewGame()
    {
        selector.transform.DOMove(newGameButton.transform.position, 0.2f);
        newGameButton.GetComponentInChildren<TMP_Text>().color = Color.white;
        loadGameButton.GetComponentInChildren<TMP_Text>().color = new Color32(100, 100, 100, 255);
        optionButton.GetComponentInChildren<TMP_Text>().color = new Color32(100, 100, 100, 255);
        exitButton.GetComponentInChildren<TMP_Text>().color = new Color32(100, 100, 100, 255);
    }

    public void OnLoadGameClicked()
    {        
        if(selector.transform.position == loadGameButton.transform.position)
        {
            Sequence buttonBlink = DOTween.Sequence();
            buttonBlink.Append(loadGameButton.GetComponentInChildren<TMP_Text>().DOColor(new Color32(255, 255, 255, 0), 0.1f));
            buttonBlink.Append(loadGameButton.GetComponentInChildren<TMP_Text>().DOColor(Color.white, 0.1f));
            buttonBlink.SetLoops(2);
            StartCoroutine(DataManager.Inst.LoadPlayerData(true));
            buttonBlink.OnComplete(() =>
            {
                SceneChangeManager.Inst.SceneFadeOut("MapScene");
            });
        }
        else
        {
            SelectLoadGame();
        }
    }

    public void SelectLoadGame()
    {
        selector.transform.DOMove(loadGameButton.transform.position, 0.2f);
        newGameButton.GetComponentInChildren<TMP_Text>().color = new Color32(100, 100, 100, 255);
        loadGameButton.GetComponentInChildren<TMP_Text>().color = Color.white;
        optionButton.GetComponentInChildren<TMP_Text>().color = new Color32(100, 100, 100, 255);
        exitButton.GetComponentInChildren<TMP_Text>().color = new Color32(100, 100, 100, 255);
    }
    public void OnOptionClicked()
    {
        if(selector.transform.position == optionButton.transform.position)
        {
            Sequence buttonBlink = DOTween.Sequence();
            buttonBlink.Append(optionButton.GetComponentInChildren<TMP_Text>().DOColor(new Color32(255, 255, 255, 0), 0.1f));
            buttonBlink.Append(optionButton.GetComponentInChildren<TMP_Text>().DOColor(Color.white, 0.1f));
            buttonBlink.SetLoops(2);
        }
        else
        {
            SelectOption();
        }
    }
    public void SelectOption()
    {
        selector.transform.DOMove(optionButton.transform.position, 0.2f);
        newGameButton.GetComponentInChildren<TMP_Text>().color = new Color32(100, 100, 100, 255);
        loadGameButton.GetComponentInChildren<TMP_Text>().color = new Color32(100, 100, 100, 255);
        optionButton.GetComponentInChildren<TMP_Text>().color = Color.white;
        exitButton.GetComponentInChildren<TMP_Text>().color = new Color32(100, 100, 100, 255);
    }
    public void OnExitClicked()
    {
        if(selector.transform.position == exitButton.transform.position)
        {
            Sequence buttonBlink = DOTween.Sequence();
            buttonBlink.Append(exitButton.GetComponentInChildren<TMP_Text>().DOColor(new Color32(255, 255, 255, 0), 0.1f));
            buttonBlink.Append(exitButton.GetComponentInChildren<TMP_Text>().DOColor(Color.white, 0.1f));
            buttonBlink.SetLoops(2);
            buttonBlink.OnComplete(() =>
            {
                Application.Quit();
            });
        }
        else
        {
            SelectExit();
        }
    }

    public void SelectExit()
    {
        selector.transform.DOMove(exitButton.transform.position, 0.2f);
        newGameButton.GetComponentInChildren<TMP_Text>().color = new Color32(100, 100, 100, 255);
        loadGameButton.GetComponentInChildren<TMP_Text>().color = new Color32(100, 100, 100, 255);
        optionButton.GetComponentInChildren<TMP_Text>().color = new Color32(100, 100, 100, 255);
        exitButton.GetComponentInChildren<TMP_Text>().color = Color.white;
    }
}
