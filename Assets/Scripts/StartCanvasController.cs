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

    public void OnNewGameClicked()
    {
        if (!isLoading)
            StartCoroutine(BlackAndLoad("PassengerScene"));
    }

    public void OnLoadGameClicked()
    {
        if (!isLoading)
            StartCoroutine(BlackAndLoad("PassengerScene"));
    }
    
    private IEnumerator BlackAndLoad(string sceneName)
    {
        isLoading = true;
        var c = blackPanel.color;
        c.a = 1f;
        blackPanel.color = c;
        blackPanel.gameObject.SetActive(true);

        yield return new WaitForSeconds(1f);

        SceneManager.LoadScene(sceneName);
    }
}
