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
        fadeoutScreen.color = new Color(Color.black.r, Color.black.g, Color.black.b, 0f);
        fadeoutScreen.gameObject.SetActive(true);
        Sequence fadeout = DOTween.Sequence();
        fadeout.Append(fadeoutScreen.DOFade(1f, 1f).OnComplete(() =>
        {
            SceneManager.LoadScene(toSceneName);
        }));
    }

    public void SceneFadeIn(Action callbackAction)
    {
        fadeoutScreen.color = new Color(Color.black.r, Color.black.g, Color.black.b, 1f);
        fadeoutScreen.gameObject.SetActive(true);
        Sequence fadein = DOTween.Sequence();
        fadein.Append(fadeoutScreen.DOFade(0f, 1f).OnComplete(() => 
        {
            fadeoutScreen.gameObject.SetActive(false);
            callbackAction?.Invoke();
        }));
    }
}
