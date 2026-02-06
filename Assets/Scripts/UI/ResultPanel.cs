using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using System;
using UnityEngine.UI;

public class ResultPanel : MonoBehaviour
{
    [SerializeField] GameObject winPanel;
    [SerializeField] GameObject losePanel;

    public void ShowWin(Action onComplete = null)
    {
        ScaleOne();
        AlphaZero();
        winPanel.SetActive(true);
        losePanel.SetActive(false);
        AlphaTween(1f, 2.5f, onComplete);
    }

    public void ShowLose(Action onComplete = null)
    {
        ScaleZero();
        winPanel.SetActive(false);
        losePanel.SetActive(true);
        transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.InOutQuad).OnComplete(() => onComplete?.Invoke());
    }

    public void Restart()
    {
        SceneChangeManager.Inst.SceneFadeOut("StartScene");
    }
    
    private void Start()
    {
        ScaleZero(); 
    }

    void ScaleOne()
    {
        transform.localScale = Vector3.one;
    }

    void ScaleZero()
    {
        transform.localScale = Vector3.zero;
    }

    void AlphaZero()
    {
        var image = winPanel.GetComponentsInChildren<Image>();
        foreach(var img in image)
        {
            Color color = img.color;
            color.a = 0f;
            img.color = color;
        }
        var texts = winPanel.GetComponentsInChildren<TMP_Text>();
        foreach(var text in texts)
        {
            Color color = text.color;
            color.a = 0f;
            text.color = color;
        }
    }

    void AlphaOne()
    {
        var image = winPanel.GetComponentsInChildren<Image>();
        foreach(var img in image)
        {
            Color color = img.color;
            color.a = 1f;
            img.color = color;
        }
        var texts = winPanel.GetComponentsInChildren<TMP_Text>();
        foreach(var text in texts)
        {
            Color color = text.color;
            color.a = 1f;
            text.color = color;
        }
    }

    void AlphaTween(float targetAlpha, float duration, Action onComplete = null)
    {
        var images = winPanel.GetComponentsInChildren<Image>();
        foreach(var img in images)
        {
            img.DOFade(targetAlpha, duration);
        }
        var texts = winPanel.GetComponentsInChildren<TMP_Text>();
        foreach(var text in texts)
        {
            text.DOFade(targetAlpha, duration);
        }
        DOTween.Sequence()
            .AppendInterval(duration)
            .OnComplete(() => onComplete?.Invoke());
    }
}
