using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;
using System;

public class SceneChangeManager : MonoBehaviour
{
    public static SceneChangeManager Inst { get; private set; }
    void Awake() => Inst = this;

    [SerializeField] Image fadeoutScreen;

    public void SceneFadeOut(string toSceneName)
    {
        Tooltip.showTooltipSignal = false;
        fadeoutScreen.color = new Color(Color.black.r, Color.black.g, Color.black.b, 0f);
        fadeoutScreen.gameObject.SetActive(true);
        DataManager.Inst.SavePlayerData();
        Sequence fadeout = DOTween.Sequence();
        fadeout.Append(fadeoutScreen.DOFade(1f, 1f).OnComplete(() =>
        {
            if(PlayerManager.Inst != null)
            {
                string curSceneName = SceneManager.GetActiveScene().name;
                if (PlayerPositionData.scenePlayerPos.ContainsKey(curSceneName))
                {
                    PlayerPositionData.scenePlayerPos[curSceneName] = PlayerManager.Inst.player.transform.position;
                }
                else
                {
                    PlayerPositionData.scenePlayerPos.Add(curSceneName, PlayerManager.Inst.player.transform.position);
                }
            }
            SceneManager.LoadScene(toSceneName);
        }));
    }

    public void SceneFadeIn(Action callbackAction)
    {
        if(PlayerManager.Inst != null)
        {
            string curSceneName = SceneManager.GetActiveScene().name;
            if (PlayerPositionData.scenePlayerPos.ContainsKey(curSceneName))
            {
                PlayerManager.Inst.player.transform.position = PlayerPositionData.scenePlayerPos[curSceneName];
                PlayerManager.Inst.player.moveTowards = PlayerManager.Inst.player.transform.position;
            }
        }
        fadeoutScreen.color = new Color(Color.black.r, Color.black.g, Color.black.b, 1f);
        fadeoutScreen.gameObject.SetActive(true);
        Sequence fadein = DOTween.Sequence();
        fadein.Append(fadeoutScreen.DOFade(0f, 1f).OnComplete(() => 
        {
            fadeoutScreen.gameObject.SetActive(false);
            Tooltip.showTooltipSignal = true;
            callbackAction?.Invoke();
        }));
    }
}
