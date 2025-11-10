using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class HealNPC : PlayerInteractableObject
{
    [SerializeField] float healPercent;
    [SerializeField] Image loadingScreen;
    public override void Interact()
    {
        loadingScreen.gameObject.SetActive(true);
        loadingScreen.fillAmount = 0f;
        loadingScreen.fillClockwise = true;

        Sequence healSeq = DOTween.Sequence();
        healSeq.Append(loadingScreen.DOFillAmount(1f, 1f).OnComplete(() =>
        {
            PlayerManager.Inst.characterSO.curHealth += (int)(PlayerManager.Inst.characterSO.maxHealth * healPercent);
            if (PlayerManager.Inst.characterSO.curHealth > PlayerManager.Inst.characterSO.maxHealth) PlayerManager.Inst.characterSO.curHealth = PlayerManager.Inst.characterSO.maxHealth;
            loadingScreen.fillClockwise = false;
        }));
        healSeq.Append(loadingScreen.DOFillAmount(0f, 1f).OnComplete(() =>
        {
            loadingScreen.gameObject.SetActive(false);
        }));
    }
}
