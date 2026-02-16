using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine.Rendering.Universal;

public class MagicianEffectManager : MonoBehaviour
{
    public static MagicianEffectManager Inst;
    void Awake()
    {
        Inst = this;
    }
    [SerializeField] GameObject lighting;
    [SerializeField] GameObject spotLight;
    [SerializeField] GameObject player;
    [SerializeField] Vector3 playerStartPos_notClear;
    [SerializeField] Vector3 playerStartPos_clear;
    [SerializeField] GameObject mainCamera;
    [SerializeField] Vector3 mainCameraStartPos_notClear;
    [SerializeField] Vector3 mainCameraStartPos_clear;
    [SerializeField] Vector3 mainCameraEffectPosition;
    [SerializeField] GameObject objects;
    [SerializeField] GameObject curtain_closed;
    [SerializeField] GameObject curtain_open;
    [SerializeField] GameObject[] fogs;
    [SerializeField] GameObject[] stageLights;
    [SerializeField] GameObject magician_alive;
    [SerializeField] GameObject magician_dead;
    [SerializeField] List<DialogueLine> magicianEffectDialogueLines;

    void Start()
    {
        spotLight.SetActive(false);
        lighting.SetActive(true);
        SoundManager.Inst.StopBGM();

        if(DataManager.Inst.characterSO.bossClear == false)
        {
            lighting.GetComponent<Light2D>().intensity = 0.1f;
            player.transform.position = playerStartPos_notClear;
            player.GetComponent<RoomPlayer>().moveTowards = playerStartPos_notClear;
            mainCamera.transform.localPosition = mainCameraStartPos_notClear;
            magician_alive.SetActive(true);
            magician_dead.SetActive(false);
            foreach(Transform child in objects.transform)
            {
                if(child.gameObject.name == "Crowd") child.gameObject.SetActive(true);
            }
            curtain_closed.SetActive(true);
            curtain_open.SetActive(false);
            foreach(var fog in fogs)
            {
                fog.SetActive(true);
            }
            foreach(var stageLight in stageLights)
            {
                stageLight.GetComponent<Light2D>().intensity = 5f;
            }
        }
        else
        {
            lighting.GetComponent<Light2D>().intensity = 1f;
            player.transform.position = playerStartPos_clear;
            player.GetComponent<RoomPlayer>().moveTowards = playerStartPos_clear;
            mainCamera.transform.localPosition = mainCameraStartPos_clear;
            magician_alive.SetActive(false);
            magician_dead.SetActive(true);
            foreach(Transform child in objects.transform)
            {
                if(child.gameObject.name == "Crowd") child.gameObject.SetActive(false);
            }
            curtain_closed.SetActive(false);
            curtain_open.SetActive(true);
            foreach(var fog in fogs)
            {
                fog.SetActive(false);
            }
            foreach(var stageLight in stageLights)
            {
                stageLight.GetComponent<Light2D>().intensity = 0f;
            }
        }
    }

    public void StartEffect()
    {
        if(DataManager.Inst.characterSO.bossClear) return;
        mainCamera.GetComponent<PassengerCamera>().lockFollow = true;
        Sequence effectSequence = DOTween.Sequence();
        effectSequence.Append(mainCamera.transform.DOMove(mainCameraEffectPosition, 2f).SetEase(Ease.InOutSine).OnComplete(() =>
        {
            spotLight.SetActive(true);
        }))
        .AppendInterval(1f).OnComplete(() =>
        {
            RoomDialogueManager.Inst.ShowDialogueList(magicianEffectDialogueLines);
            RoomDialogueManager.OnDialogueEnd += () =>
            {
                DataManager.Inst.characterSO.enemyName = "마술사";
                SceneChangeManager.Inst.SceneFadeOut("BattleScene");
            };
        });
    }
}
