using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System.Collections;

public class StartCanvasController : MonoBehaviour
{
    [SerializeField] private GameObject pabText; 
    [SerializeField] private GameObject menuGroup; 
    [SerializeField] private Image blackPanel; 

    private bool menuActivated = false;
    private bool isLoading  = false;
    private CanvasGroup pabCanvasGroup;

    void Start()
    {
        // 초기 상태
        menuGroup.SetActive(false);
        pabText.SetActive(true);
        blackPanel.gameObject.SetActive(false);

        if (blackPanel != null)
        {
            var  c = blackPanel.color;
            c.a = 0f;
            blackPanel.color = c;
        }
        pabCanvasGroup = pabText.GetComponent<CanvasGroup>();
        if (pabCanvasGroup == null)
            pabCanvasGroup = pabText.AddComponent<CanvasGroup>();

        pabCanvasGroup.alpha = 1f;

        // 깜빡이는 코루틴 시작
        StartCoroutine(FadePabTextRoutine());
    }

    void Update()
    {
        if (menuActivated) return;

        if (Input.anyKeyDown || Input.GetMouseButtonDown(0))
        {
            ActivateMenu();
        }
    }

    private void ActivateMenu()
    {
        menuActivated = true;

        pabText.SetActive(false);
        menuGroup.SetActive(true);

        var firstButton = menuGroup.GetComponentInChildren<UnityEngine.UI.Button>();
        if (firstButton != null)
        {
            EventSystem.current.SetSelectedGameObject(firstButton.gameObject);
        }
    }

    public void OnNewGameClicked(Button btn)
    {
        StartCoroutine(ClickScaleRoutine(btn.transform));
        if (!isLoading)
            StartCoroutine(BlackAndLoad("RoomScene"));
    }

    public void OnLoadGameClicked(Button btn)
    {        
        StartCoroutine(ClickScaleRoutine(btn.transform));
        if (!isLoading)
            StartCoroutine(BlackAndLoad("MapScene"));
    }
    public void OnOptionClicked(Button btn)
    {
        StartCoroutine(ClickScaleRoutine(btn.transform));
    }
    public void OnExitClicked(Button btn)
    {
        StartCoroutine(ClickScaleRoutine(btn.transform));
        Debug.Log("Exit Game");
        Application.Quit();
    }
    
    private IEnumerator ClickScaleRoutine(Transform target)
    {
        Vector3 original = target.localScale;
        Vector3 smaller = original * 0.9f;

        float duration = 0.08f;
        float t = 0f;

        // 눌림 (작아짐)
        while (t < duration)
        {
            t += Time.deltaTime;
            target.localScale = Vector3.Lerp(original, smaller, t / duration);
            yield return null;
        }

        // 복귀
        t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            target.localScale = Vector3.Lerp(smaller, original, t / duration);
            yield return null;
        }
    }

    
    private IEnumerator BlackAndLoad(string sceneName)
    {
        isLoading = true;

        blackPanel.gameObject.SetActive(true);
        Color c = blackPanel.color;
        c.a = 0f;
        blackPanel.color = c;

        float duration = 1f; 
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, t / duration);
            c.a = alpha;
            blackPanel.color = c;
            yield return null;
        }

        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene(sceneName);
    }
    private IEnumerator FadePabTextRoutine()
    {
        float duration = 1f;

        while (!menuActivated)
        {
            float t = 0f;
            while (t < duration && !menuActivated)
            {
                t += Time.deltaTime;
                pabCanvasGroup.alpha = Mathf.Lerp(0f, 1f, t / duration);
                yield return null;
            }

            yield return new WaitForSeconds(0.5f);
            
            t = 0f;
            while (t < duration && !menuActivated)
            {
                t += Time.deltaTime;
                pabCanvasGroup.alpha = Mathf.Lerp(1f, 0f, t / duration);
                yield return null;
            }

            yield return null;
        }
    }
}
