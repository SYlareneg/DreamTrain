using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Sofa : PlayerInteractableObject
{
    [SerializeField] Image fadeoutScreen;
    [SerializeField] Player player;
    [SerializeField] Vector2 playerSitPos;
    public override void Interact()
    {
        if (alreadyInteracted)
        {
            return;
        }
        fadeoutScreen.color = new Color(Color.black.r, Color.black.g, Color.black.b, 0f);
        fadeoutScreen.gameObject.SetActive(true);
        Sequence sofaInteract = DOTween.Sequence();
        sofaInteract.Append(fadeoutScreen.DOFade(1f, 1f).OnComplete(() =>
        {
            player.transform.position = playerSitPos;
            player.moveTowards = playerSitPos;
            PlayerManager.Inst.isLoading = true;
            NPCSofaManager.Inst.sofa = this;
        }));
        sofaInteract.Append(fadeoutScreen.DOFade(0f, 1f).OnComplete(() =>
        {
            fadeoutScreen.gameObject.SetActive(false);
        }));
        sofaInteract.AppendInterval(1f).OnComplete(() =>
        {
            NPCSofaManager.Inst.ShowSofaUI();
        });
    }

    void Start()
    {
        if (NPCSofaManager.Inst.stageSO.restUsed)
        {
            alreadyInteracted = true;
            alreadyInteractedSpeech = "다음 정거장에 도착하기 전까지는 쉴 수 없겠어.(소파)";
        }
    }
}
