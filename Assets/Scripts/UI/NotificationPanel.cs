using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NotificationPanel : MonoBehaviour
{
    [SerializeField] TMP_Text notificationTMP;
    [SerializeField] float notificationTime = 1.5f;

    public void Show(string message, Action onComplete)
    {
        notificationTMP.text = message;
        Sequence sequence = DOTween.Sequence()
            .Append(transform.DOScale(Vector3.one, notificationTime*0.2f).SetEase(Ease.InOutQuad))
            .AppendInterval(notificationTime*0.6f)
            .Append(transform.DOScale(Vector3.zero, notificationTime*0.2f).SetEase(Ease.InOutQuad))
            .OnComplete(() => onComplete?.Invoke());
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
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
}
