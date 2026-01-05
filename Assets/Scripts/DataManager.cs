using UnityEngine;
using System.Collections.Generic;

public class DataManager : MonoBehaviour
{
    public static DataManager Inst;
    void Awake()
    {
        if (Inst != null && Inst != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Inst = this;
        DontDestroyOnLoad(this.gameObject);
        
        LoadDeveloperData();
        LoadPlayerData();
        // SavePlayerData();
    }

    [Header("Developer Data")]
    public DreamPieceDataSO dreamPieceDataSO;
    public EnemyDataSO enemyDataSO;
    public ItemDataSO itemDataSO;
    public RelicDataSO relicDataSO;
    public ShowBuffDataSO showBuffDataSO;
    public UseableItemDataSO useableItemDataSO;
    public ActDataSO actDataSO;
    public DreamPieceSO dreamPieceSO;
    public EnemySO enemySO;
    public ItemSO normalItemSO;
    public RelicSO relicSO;
    public KeywordSO keywordSO;
    public ShowBuffSO showBuffSO;
    public UseableItemSO useableItemSO;
    public ActSO actSO;
    public LocationDatabaseSO locationDatabaseSO;
    public EncounterDatabaseSO encounterDatabaseSO;

    [Header("Player Data")]
    public PlayerDataSO playerDataSO;
    public CharacterSO characterSO;
    public RelicSO playerRelicSO;
    public UseableItemSO playerItemSO;
    public PlayerStatsSo playerStatsSO;

    public void LoadDeveloperData()
    {
        Utils.LoadData(itemDataSO, "card.json");
        Utils.LoadData(dreamPieceDataSO, "dreampiece.json");
        Utils.LoadData(enemyDataSO, "enemy.json");
        Utils.LoadData(keywordSO, "keyword.json");
        Utils.LoadData(relicDataSO, "relic.json");
        Utils.LoadData(showBuffDataSO, "buff.json");
        Utils.LoadData(useableItemDataSO, "item.json");
        Utils.LoadData(actDataSO, "act.json");

        dreamPieceSO.dreamPieces.Clear();
        foreach (var dp in dreamPieceDataSO.dreamPieces)
        {
            DreamPiece_Reference newDP = new DreamPiece_Reference();
            newDP.Setup(dp, itemDataSO);
            dreamPieceSO.dreamPieces.Add(newDP);
        }
        enemySO.enemies.Clear();
        foreach (var enemy in enemyDataSO.enemies)
        {
            Enemy newEnemy = new Enemy(enemy);
            enemySO.enemies.Add(newEnemy);
        }
        enemySO.subEnemies.Clear();
        foreach (var subEnemy in enemyDataSO.subEnemies)
        {
            SubEnemy newSubEnemy = new SubEnemy(subEnemy);
            enemySO.subEnemies.Add(newSubEnemy);
        }
        normalItemSO.items.Clear();
        foreach (var item in itemDataSO.items)
        {
            if (item.dreamPieceNum == -1)
            {
                normalItemSO.items.Add(new Item(item));
            }
        }
        relicSO.relicItems.Clear();
        foreach(var relic in relicDataSO.relicItems)
        {
            relicSO.relicItems.Add(new RelicItem_Enhanceable(relic));
        }
        showBuffSO.showBuffs.Clear();
        foreach (var sb in showBuffDataSO.showBuffs)
        {
            showBuffSO.showBuffs.Add(new ShowBuff(sb));
        }
        useableItemSO.useableItems.Clear();
        foreach (var ui in useableItemDataSO.useableItems)
        {
            useableItemSO.useableItems.Add(new UseItem(ui));
        }
        actSO.acts.Clear();
        foreach (var act in actDataSO.actDataList)
        {
            Act newAct = new Act(act, actDataSO.locationDataList);
            actSO.acts.Add(newAct);
        }
        actSO.normalNodes.Clear();
        foreach(var loc in actDataSO.locationDataList)
        {
            if(loc.isNormalLocation)
            {
                MapNode normalNode = new MapNode(loc);
                actSO.normalNodes.Add(normalNode);
            }
        }
        locationDatabaseSO.locationTable.Clear();
        foreach(var loc in actDataSO.locationDataList)
        {
            LocationMetaInfo locInfo = new LocationMetaInfo();
            locInfo.id = loc.id;
            locInfo.nameKO = loc.nameKO;
            locInfo.encounterPool = new List<string>(loc.encounterPool);
            locInfo.selectedEncounterPool = new List<string>();
            locInfo.howManyEnc = loc.encounterNum;
            locationDatabaseSO.locationTable.Add(locInfo);
        }
        encounterDatabaseSO.masterTable.Clear();
        foreach(var enc in actDataSO.encounterDataList)
        {
            EncounterMetaInfo encInfo = new EncounterMetaInfo();
            encInfo.id = enc.id;
            encInfo.nameKO = enc.nameKO;
            encInfo.type = enc.type;
            encInfo.imagePath = enc.imagePath;
            encInfo.filePath = enc.filePath;
            encInfo.order = enc.order;
            encInfo.isEssential = enc.isEssential;
            encounterDatabaseSO.masterTable.Add(encInfo);
        }
    }

    public void LoadPlayerData()
    {
        Utils.LoadData(playerDataSO, "player_data.json");
        // 전투 외적 데이터
        characterSO.maxHealth = playerDataSO.maxHealth;
        characterSO.curHealth = playerDataSO.curHealth;
        characterSO.dreamDust = playerDataSO.dreamDust;
        characterSO.leftPassengers = playerDataSO.leftPassengers;
        // 카드 데이터
        if (playerDataSO.personaPiece != "")
        {
            characterSO.personaPiece = new DreamPiece_Player(playerDataSO.personaPiece, playerDataSO.isPersonaEnhanced, false, playerDataSO.personaCards, dreamPieceDataSO, itemDataSO);
        }
        if (playerDataSO.shadowPiece != "")
        {
            characterSO.shadowPiece = new DreamPiece_Player(playerDataSO.shadowPiece, false, playerDataSO.isShadowEnhanced, playerDataSO.shadowCards, dreamPieceDataSO, itemDataSO);
        }
        characterSO.normalCards = new List<Item>();
        foreach (var cardData in playerDataSO.normalCards)
        {
            Item_Data item_Data = itemDataSO.items.Find(x => x.name == cardData.cardName);
            if (item_Data != null)
            {
                Item item = new Item(item_Data);
                item.num = cardData.num;
                characterSO.normalCards.Add(item);
            }
        }
        // 오브제 데이터
        if(playerDataSO.relics.Count != playerDataSO.relicEnhancements.Count)
        {
            Debug.LogError("DataManager LoadPlayerData Error: relic count mismatch");
            return;
        }
        playerRelicSO.relicItems.Clear();
        for(int i = 0; i < playerDataSO.relics.Count; i++)
        {
            int relicOwner = playerDataSO.relics[i];
            RelicItem_Enhanceable relicItem = new RelicItem_Enhanceable(relicDataSO.relicItems.Find(x => x.relicOwner == relicOwner));
            relicItem.isEnhanced = playerDataSO.relicEnhancements[i];
            playerRelicSO.relicItems.Add(relicItem);
        }
        // 소모품 데이터
        playerItemSO.useableItems.Clear();
        foreach (var itemName in playerDataSO.useableItems)
        {
            UseItem useItem = useableItemSO.useableItems.Find(x => x.name == itemName);
            if(useItem == null) continue;
            playerItemSO.useableItems.Add(new UseItem(useItem));
        }
        // 스탯 데이터
        playerStatsSO.courage = playerDataSO.courage;
        playerStatsSO.wisdom = playerDataSO.wisdom;
        playerStatsSO.luck = playerDataSO.luck;
        // 맵 데이터
        actSO.curActNum = playerDataSO.currentActNum;
        actSO.mapSave = new Map();
        actSO.mapSave.sortedMapNodeList = new List<MapNode>();
        actSO.mapSave.totalLevel = playerDataSO.totalLevel;
        actSO.mapNodeScreenPosSave = new List<Vector3>();
        foreach(var nodeData in playerDataSO.mapNodes)
        {
            MapNode mapNode = new MapNode(nodeData, actDataSO.locationDataList);
            actSO.mapSave.sortedMapNodeList.Add(mapNode);
            actSO.mapNodeScreenPosSave.Add(nodeData.screenPos);
            locationDatabaseSO.FindById(nodeData.locationID).selectedEncounterPool = new List<string>(nodeData.selectedEncounterPool);
        }
        actSO.curNodeIndex = playerDataSO.curNodeIndex;
        actSO.curNodeLocationID = playerDataSO.curNodeLocationID;
        actSO.lastEncounterType = playerDataSO.lastEncounterType;
        // 전투 데이터
        characterSO.enemyName = playerDataSO.enemyName;
    }

    public void SavePlayerData()
    {
        // 전투 외적 데이터
        playerDataSO.maxHealth = characterSO.maxHealth;
        playerDataSO.curHealth = characterSO.curHealth;
        playerDataSO.dreamDust = characterSO.dreamDust;
        playerDataSO.leftPassengers = characterSO.leftPassengers;
        // 카드 데이터
        playerDataSO.personaPiece = characterSO.personaPiece != null ? characterSO.personaPiece.name : "";
        playerDataSO.isPersonaEnhanced = characterSO.personaPiece != null ? characterSO.personaPiece.persona.isEnhanced : false;
        playerDataSO.personaCards = new List<Item_Num>();
        foreach (var card in characterSO.personaPiece.cards)
        {
            playerDataSO.personaCards.Add(new Item_Num(card.name, card.num));
        }
        playerDataSO.shadowPiece = characterSO.shadowPiece != null ? characterSO.shadowPiece.name : "";
        playerDataSO.isShadowEnhanced = characterSO.shadowPiece != null ? characterSO.shadowPiece.shadow.isEnhanced : false;
        playerDataSO.shadowCards = new List<Item_Num>();
        foreach (var card in characterSO.shadowPiece.cards)
        {
            playerDataSO.shadowCards.Add(new Item_Num(card.name, card.num));
        }
        playerDataSO.normalCards = new List<Item_Num>();
        foreach (var card in characterSO.normalCards)
        {
            playerDataSO.normalCards.Add(new Item_Num(card.name, card.num));
        }
        // 오브제 데이터
        playerDataSO.relics = new List<int>();
        playerDataSO.relicEnhancements = new List<bool>();
        foreach (var relic in playerRelicSO.relicItems)
        {
            playerDataSO.relics.Add(relic.relicOwner);
            playerDataSO.relicEnhancements.Add(relic.isEnhanced);
        }
        // 소모품 데이터
        playerDataSO.useableItems = new List<string>();
        foreach (var item in playerItemSO.useableItems)
        {
            playerDataSO.useableItems.Add(item.name);
        }
        // 스탯 데이터
        playerDataSO.courage = playerStatsSO.courage;
        playerDataSO.wisdom = playerStatsSO.wisdom;
        playerDataSO.luck = playerStatsSO.luck;
        // 맵 데이터
        playerDataSO.currentActNum = actSO.curActNum;
        playerDataSO.mapNodes = new List<MapNode_Data>();
        if(actSO.mapSave.sortedMapNodeList.Count != actSO.mapNodeScreenPosSave.Count)
        {
            Debug.LogError("DataManager SavePlayerData Error: map node count mismatch");
            return;
        }
        for(int i = 0; i < actSO.mapSave.sortedMapNodeList.Count; i++)
        {
            playerDataSO.mapNodes.Add(new MapNode_Data(actSO.mapSave.sortedMapNodeList[i], actSO.mapNodeScreenPosSave[i]));
            playerDataSO.mapNodes[i].selectedEncounterPool = new List<string>(locationDatabaseSO.FindById(playerDataSO.mapNodes[i].locationID).selectedEncounterPool);
        }
        playerDataSO.totalLevel = actSO.mapSave.totalLevel;
        playerDataSO.curNodeIndex = actSO.curNodeIndex;
        playerDataSO.curNodeLocationID = actSO.curNodeLocationID;
        playerDataSO.lastEncounterType = actSO.lastEncounterType;
        // 전투 데이터
        playerDataSO.enemyName = characterSO.enemyName;
        Utils.SaveData(playerDataSO, "player_data.json");
    }
}
