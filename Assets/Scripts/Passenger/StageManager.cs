using UnityEngine;

public class StageManager : MonoBehaviour
{
    public static StageManager Inst { get; private set; }
    void Awake()
    {
        Inst = this;
        stageNum = stageSO.stageList.Length;
        curStageNum = stageSO.currentStage;
        enemyNum = stageSO.stageList[curStageNum].stageEnemies.Count;
    }

    public StageSO stageSO;
    public int stageNum;
    public int curStageNum;
    public int enemyNum;
    public CharacterSO characterSO;
    public StationDoor stationDoor;
    public RestDoor restDoor;
    public RoomDoor[] roomDoors;
    public int[] roomDoorNum;
    public GameObject enemyNPCPrefab;
    [SerializeField] Player player;

    void InitStationDoor()
    {
        if (stageSO.stageList[curStageNum].stageClear)
        {
            stationDoor.alreadyInteracted = false;
        }
        else
        {
            stationDoor.alreadyInteracted = true;
            stationDoor.alreadyInteractedSpeech = "아직 정거장에 도착하지 않았어.";
        }
    }
    void InitRoomDoors()
    {
        for(int i = 0; i < stageNum; i++)
        {
            roomDoors[i].roomNum = roomDoorNum[i];
            roomDoors[i].passengerName = stageSO.stageList[curStageNum].bossEnemy.enemyName;
            roomDoors[i].alreadyInteracted = true;
            if(i == curStageNum)
            {
                roomDoors[i].alreadyInteractedSpeech = "불청객들을 모두 처리해야 들어갈 수 있어.";
            }
            else
            {
                roomDoors[i].alreadyInteractedSpeech = "이 승객은 이번에 문을 열어 주지 않을 것 같아...";
            }
        }
    }

    void InitEnemies()
    {
        characterSO.leftPassengers = 0;
        for(int i = 0; i < enemyNum; i++)
        {
            if(stageSO.stageList[curStageNum].stageEnemies[i].isClear) continue;
            var enemyObj = Instantiate(enemyNPCPrefab, Vector3.zero, Utils.QI);
            EnemyNPC enemyNPC = enemyObj.GetComponent<EnemyNPC>();
            enemyNPC.Setup(stageSO.stageList[curStageNum].stageEnemies[i]);
            characterSO.leftPassengers++;
        }
    }

    void SetStage()
    {
        InitStationDoor();
        InitRoomDoors();
        InitEnemies();
    }

    void Start()
    {
        player.transform.position = Vector3.zero;
        player.moveTowards = Vector3.zero;
        switch(stageSO.playerSpawn)
        {
            case EPlayerSpawn.Station:
                player.transform.position = stationDoor.transform.position;
                player.moveTowards = stationDoor.transform.position;
                break;
            case EPlayerSpawn.Rest:
                player.transform.position = restDoor.transform.position;
                player.moveTowards = restDoor.transform.position;
                break;
            case EPlayerSpawn.Room:
                player.transform.position = roomDoors[curStageNum].transform.position;
                player.moveTowards = roomDoors[curStageNum].transform.position;
                break;
        }
        SetStage();
    }

    void Update()
    {
        if(characterSO.leftPassengers == 0 && stageSO.stageList[curStageNum].stageClear == false)
        {
            roomDoors[curStageNum].alreadyInteracted = false;
        }
    }
}
