using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public static class PlayerPositionData
{
    public static Dictionary<string, Vector3> scenePlayerPos = new Dictionary<string, Vector3>();
}
public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Inst { get; private set; }
    void Awake() => Inst = this;

    public bool isLoading;
    public CharacterSO characterSO;
    public DreamPieceSO dreamPieceSO;
    public Player player;
    public Canvas playerCanvas;

    [Header("플레이어 UI")]
    [SerializeField] TMP_Text playerHealth;
    [SerializeField] Image playerHealthBar;
    [SerializeField] TMP_Text playerDreamDust;
    [SerializeField] TMP_Text passengerNum;
    [Header("플레이어 말풍선")]
    [SerializeField] GameObject playerSpeechBubble;
    [SerializeField] TMP_Text playerSpeechBubbleTMP;
    string playerSpeech;
    [SerializeField] float playerSpeechTime;
    [SerializeField] Vector2 playerSpeechOffset;

    void Start()
    {
        isLoading = true;
        SceneChangeManager.Inst.SceneFadeIn(() => isLoading = false);
    }
    void UpdateUIState()
    {
        if(playerHealth != null)
        {
            playerHealth.text = characterSO.curHealth.ToString() + "/" + characterSO.maxHealth.ToString();
        }
        if(playerHealthBar != null)
        {
            playerHealthBar.fillAmount = (float)characterSO.curHealth / characterSO.maxHealth;
        }
        if(playerDreamDust != null)
        {
            playerDreamDust.text = "꿈 가루: " + characterSO.dreamDust.ToString();
        }
        if(passengerNum != null)
        {
            passengerNum.text = "남은 승객: " + characterSO.leftPassengers.ToString() + "명";
        }
    }

    void Update()
    {
        UpdateUIState();
    }

    public void SetPlayerSpeech(string s)
    {
        playerSpeech = s;
        playerSpeechBubbleTMP.text = s;
    }
    
    public IEnumerator ShowPlayerSpeech()
    {
        Vector3 screenPos = Camera.main.WorldToScreenPoint(player.transform.position);
        Debug.Log(screenPos);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            playerCanvas.transform as RectTransform,
            screenPos,
            null,
            out Vector2 localPos
        );
        Debug.Log(localPos);
        localPos += playerSpeechOffset;
        playerSpeechBubble.GetComponent<RectTransform>().anchoredPosition = localPos;
        playerSpeechBubble.SetActive(true);
        yield return new WaitForSeconds(playerSpeechTime);
        playerSpeechBubble.SetActive(false);
    }
}
