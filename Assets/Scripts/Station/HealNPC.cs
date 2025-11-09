using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class HealNPC : PlayerInteractableObject
{
    [SerializeField] CharacterSO characterSO;
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
            characterSO.curHealth += (int)(characterSO.maxHealth * healPercent);
            if (characterSO.curHealth > characterSO.maxHealth) characterSO.curHealth = characterSO.maxHealth;
            loadingScreen.fillClockwise = false;
        }));
        healSeq.Append(loadingScreen.DOFillAmount(0f, 1f).OnComplete(() =>
        {
            loadingScreen.gameObject.SetActive(false);
        }));

        Destroy(this);
    }
}
