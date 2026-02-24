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
    [SerializeField] AudioClip selectSound;
    [SerializeField] AudioClip enterSound;

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
        SoundManager.Inst.PlayBGM(SoundManager.Inst.titleBGM);
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
            StartCoroutine(InitDataCoroutine());
            buttonBlink.OnComplete(() =>
            {
                SceneChangeManager.Inst.SceneFadeOut("RoomScene");
            });

            GetComponent<AudioSource>().PlayOneShot(enterSound);
        }
        else
        {
            SelectNewGame();
        }
    }

    IEnumerator InitDataCoroutine()
    {
        yield return DataManager.Inst.LoadPlayerData(true);
        DataManager.Inst.playerDataSO.isTutorial = true;
        DataManager.Inst.playerDataSO.currentActNum = 0;
        DataManager.Inst.playerDataSO.dreamDust = 0;
        DataManager.Inst.characterSO.isTutorial = true;
        DataManager.Inst.actSO.curActNum = 0;
        DataManager.Inst.characterSO.dreamDust = 0;
        Utils.SaveData(DataManager.Inst.playerDataSO, "player_data_start.json");
    }

    public void SelectNewGame()
    {
        selector.transform.DOMove(newGameButton.transform.position, 0.2f);
        newGameButton.GetComponentInChildren<TMP_Text>().color = Color.white;
        loadGameButton.GetComponentInChildren<TMP_Text>().color = new Color32(100, 100, 100, 255);
        optionButton.GetComponentInChildren<TMP_Text>().color = new Color32(100, 100, 100, 255);
        exitButton.GetComponentInChildren<TMP_Text>().color = new Color32(100, 100, 100, 255);
        GetComponent<AudioSource>().PlayOneShot(selectSound);
    }

    public void OnLoadGameClicked()
    {        
        if(selector.transform.position == loadGameButton.transform.position)
        {
            Sequence buttonBlink = DOTween.Sequence();
            buttonBlink.Append(loadGameButton.GetComponentInChildren<TMP_Text>().DOColor(new Color32(255, 255, 255, 0), 0.1f));
            buttonBlink.Append(loadGameButton.GetComponentInChildren<TMP_Text>().DOColor(Color.white, 0.1f));
            buttonBlink.SetLoops(2);
            // StartCoroutine(DataManager.Inst.LoadPlayerData(false));
            buttonBlink.OnComplete(() =>
            {
                SceneChangeManager.Inst.SceneFadeOut(DataManager.Inst.characterSO.lastSceneName);
            });
            
            GetComponent<AudioSource>().PlayOneShot(enterSound);
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
        GetComponent<AudioSource>().PlayOneShot(selectSound);
    }
    public void OnOptionClicked()
    {
        // if(selector.transform.position == optionButton.transform.position)
        // {
        //     Sequence buttonBlink = DOTween.Sequence();
        //     buttonBlink.Append(optionButton.GetComponentInChildren<TMP_Text>().DOColor(new Color32(255, 255, 255, 0), 0.1f));
        //     buttonBlink.Append(optionButton.GetComponentInChildren<TMP_Text>().DOColor(Color.white, 0.1f));
        //     buttonBlink.SetLoops(2);
            
        //     GetComponent<AudioSource>().PlayOneShot(enterSound);
        // }
        // else
        // {
        //     SelectOption();
        // }
    }
    public void SelectOption()
    {
        selector.transform.DOMove(optionButton.transform.position, 0.2f);
        newGameButton.GetComponentInChildren<TMP_Text>().color = new Color32(100, 100, 100, 255);
        loadGameButton.GetComponentInChildren<TMP_Text>().color = new Color32(100, 100, 100, 255);
        // optionButton.GetComponentInChildren<TMP_Text>().color = Color.white;
        optionButton.GetComponentInChildren<TMP_Text>().color = new Color32(100, 100, 100, 255);
        exitButton.GetComponentInChildren<TMP_Text>().color = new Color32(100, 100, 100, 255);
        // GetComponent<AudioSource>().PlayOneShot(selectSound);
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
            
            GetComponent<AudioSource>().PlayOneShot(enterSound);
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
        GetComponent<AudioSource>().PlayOneShot(selectSound);
    }
}
