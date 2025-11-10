using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Inst { get; private set; }
    void Awake() => Inst = this;

    public bool isLoading;
    public CharacterSO characterSO;
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

    void UpdateUIState()
    {
        playerHealth.text = characterSO.curHealth.ToString() + "/" + characterSO.maxHealth.ToString();
        playerHealthBar.fillAmount = (float)characterSO.curHealth / characterSO.maxHealth;
        playerDreamDust.text = "꿈 가루: " + characterSO.dreamDust.ToString();
        passengerNum.text = "남은 승객: " + characterSO.leftPassengers.ToString() + "명";
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
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            playerCanvas.transform as RectTransform,
            screenPos,
            Camera.main,
            out Vector2 localPos
        );
        localPos += playerSpeechOffset;
        playerSpeechBubble.GetComponent<RectTransform>().anchoredPosition = localPos;
        playerSpeechBubble.SetActive(true);
        yield return new WaitForSeconds(playerSpeechTime);
        playerSpeechBubble.SetActive(false);
    }
}
