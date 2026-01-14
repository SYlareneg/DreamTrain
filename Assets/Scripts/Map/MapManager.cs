using UnityEngine;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using DG.Tweening;
using UnityEngine.SceneManagement;
using System.Linq;

class EncounterCandidate
{
    public EncounterMetaInfo info;
    public int score;
}
public class MapManager : MonoBehaviour
{
    public static MapManager Inst;
    void Awake() => Inst = this;

    [SerializeField] ActSO actSO;
    public EncounterDatabaseSO encounterDB;
    public LocationDatabaseSO locationDB;
    [SerializeField] Transform mapTransform;
    [SerializeField] GameObject mapNodePrefab;
    [SerializeField] GameObject mapLinePrefab;
    [SerializeField] Sprite[] lineSprites;
    [SerializeField] float lineWidth;
    [SerializeField] Vector3 zeroPos;
    [SerializeField] float levelDist;
    [SerializeField] float posDist;
    public Vector2 tooltipOffset;
    public Map map;
    Dictionary<string, Vector3> mapNodeScreenPos = new Dictionary<string, Vector3>();
    Dictionary<(string, string), GameObject> mapLines = new Dictionary<(string, string), GameObject>();

    public GameObject player;
    public MapCamera mapCamera;
    public bool player_moveable = true;
    [HideInInspector] public MapNode curNode = null;

    public MapNodeObject lookatNode = null;

    public Vector3 nodePos2ScreenPos(MapNode mapNode, bool addOffset = false)
    {
        Vector3 retVec = zeroPos;
        retVec.x += mapNode.level * levelDist;
        retVec.y += mapNode.pos * posDist;
        if(addOffset)
        {
            float randOffset = 0f;
            if(mapNode.pos > 0) randOffset = Random.Range(0, posDist / 4);
            else if(mapNode.pos < 0) randOffset = Random.Range(-posDist / 4, 0);
            else randOffset = Random.Range(-posDist / 4, posDist / 4);
            retVec.y += randOffset;
        }
        return retVec;
    }

    public List<EncounterType> GetEncounterType(string locationID)
    {
        LocationMetaInfo locInfo = locationDB.locationTable.Find(x => x.id == locationID);
        if (locInfo == null)
            return new List<EncounterType>();

        if(locInfo.selectedEncounterPool != null && locInfo.selectedEncounterPool.Count == locInfo.howManyEnc)
        {
            List<EncounterType> existingTypes = new List<EncounterType>();
            foreach(string encID in locInfo.selectedEncounterPool)
            {
                EncounterMetaInfo encInfo = encounterDB.masterTable.Find(x => x.id == encID);
                if (encInfo != null)
                {
                    existingTypes.Add(encInfo.type);
                }
            }
            return existingTypes;
        }
        else
        {
            locInfo.selectedEncounterPool = new List<string>();
        }

        List<(string id, EncounterType type)> result = new List<(string id, EncounterType type)>();
        List<EncounterCandidate> candidates = new List<EncounterCandidate>();
        EncounterType prevType = (actSO != null) ? actSO.lastEncounterType : EncounterType.Battle;

        foreach (string id in locInfo.encounterPool)
        {
            EncounterMetaInfo info = encounterDB.masterTable.Find(x => x.id == id);
            if (info == null) continue;
            candidates.Add(new EncounterCandidate { info = info, score = 0 });
        }

        foreach (var cand in candidates)
        {
            cand.score = Random.Range(1, 100);

            if (cand.info.isEssential)
            {
                cand.score = 100;
            }
            else
            {
                if (prevType == EncounterType.Rest && cand.info.type == EncounterType.Rest) cand.score = 0;
                if (prevType == EncounterType.Merchant && cand.info.type == EncounterType.Merchant) cand.score = 0;
            }
        }

        int pickCount = 0;
        int targetCount = locInfo.howManyEnc;

        while (pickCount < targetCount)
        {
            var best = candidates
                .Where(c => c.score > 0 && !result.Contains((c.info.id, c.info.type)))
                .OrderByDescending(c => c.score)
                .FirstOrDefault();

            if (best == null) break; 

            result.Add((best.info.id, best.info.type));
            pickCount++;

            // 같은 Order 제거
            foreach (var other in candidates)
            {
                if (other.info.id == best.info.id) continue; 
                if (other.info.order == best.info.order) other.score = 0; 
            }
        }

        result = result.OrderBy(x => encounterDB.masterTable.Find(e => e.id == x.id).order).ToList();
        List<EncounterType> typeList = new List<EncounterType>();
        foreach (var enc in result)
        {
            locInfo.selectedEncounterPool.Add(enc.id);
            typeList.Add(enc.type);
        }
        return typeList;
    }

    public void PrintMap(Map mp, List<Vector3> savedPos = null)
    {
        foreach(Transform child in mapTransform)
        {
            if(child.name == "Background") continue;
            Destroy(child.gameObject);
        }
        mapNodeScreenPos.Clear();
        for(int i = 0; i < mp.sortedMapNodeList.Count; i++)
        {
            var newMapNode = Instantiate(mapNodePrefab, Vector3.zero, Utils.QI);
            newMapNode.GetComponent<SpriteRenderer>().sprite = mp.sortedMapNodeList[i].hideNodeImg;
            MapNodeObject mapNodeObject = newMapNode.GetComponent<MapNodeObject>();
            mapNodeObject.mapNode = mp.sortedMapNodeList[i];
            if(actSO.visitedNodeIDList.Contains(mp.sortedMapNodeList[i].ID))
            {
                newMapNode.GetComponent<SpriteRenderer>().sprite = mp.sortedMapNodeList[i].nodeImg;
            }
            else
            {
                newMapNode.GetComponent<SpriteRenderer>().sprite = mp.sortedMapNodeList[i].hideNodeImg;
            }
            MapNodeTooltip mapNodeTooltip = newMapNode.GetComponent<MapNodeTooltip>();
            mapNodeTooltip.tooltipTitle = mp.sortedMapNodeList[i].title;
            mapNodeTooltip.tooltipTxt = mp.sortedMapNodeList[i].text;
            newMapNode.transform.SetParent(mapTransform);
            if(savedPos != null && savedPos.Count == mp.sortedMapNodeList.Count)
            {
                newMapNode.transform.position = savedPos[i];
            }
            else
            {
                if(i == 0 || i == mp.sortedMapNodeList.Count - 1)
                {
                    newMapNode.transform.position = nodePos2ScreenPos(mp.sortedMapNodeList[i], false);
                }
                else
                {
                    newMapNode.transform.position = nodePos2ScreenPos(mp.sortedMapNodeList[i], true);
                }
            }
            mapNodeTooltip.tooltipPos = newMapNode.transform.position;
            mapNodeScreenPos.Add(mp.sortedMapNodeList[i].ID, newMapNode.transform.position);
        }
        foreach(MapNode mapNode in mp.sortedMapNodeList)
        {
            foreach(string childNode in mapNode.childNodes)
            {
                // Vector3 linePos = (mapNodeScreenPos[mapNode.ID] + mapNodeScreenPos[childNode]) / 2;
                Vector3 linePos = mapNodeScreenPos[mapNode.ID];
                var newMapLine = Instantiate(mapLinePrefab, Vector3.zero, Utils.QI);
                newMapLine.transform.SetParent(mapTransform);
                newMapLine.transform.position = linePos;
                Vector3 direction = mapNodeScreenPos[childNode] - mapNodeScreenPos[mapNode.ID];
                newMapLine.transform.right = direction;
                newMapLine.transform.localScale = new Vector3(lineWidth, lineWidth, 1f);
                if(actSO.visitedNodeIDList.Contains(mapNode.ID) && actSO.visitedNodeIDList.Contains(childNode))
                {
                    newMapLine.GetComponent<SpriteRenderer>().sprite = lineSprites[0];
                }
                else
                {
                    newMapLine.GetComponent<SpriteRenderer>().sprite = lineSprites[1];
                }
                newMapLine.GetComponent<SpriteRenderer>().size = new Vector2(direction.magnitude / lineWidth, 1f);
                mapLines.Add((mapNode.ID, childNode), newMapLine);
            }
        }
    }

    public void SaveMap()
    {
        actSO.mapSave = map;
        actSO.mapNodeScreenPosSave = new List<Vector3>();
        foreach(MapNode mapNode in map.sortedMapNodeList)
        {
            actSO.mapNodeScreenPosSave.Add(mapNodeScreenPos[mapNode.ID]);
        }
        //DataManager.Inst.SavePlayerData();
    }

    public Vector3 GetScreenPos(MapNode mapNode)
    {
        if(mapNode == null || !mapNodeScreenPos.ContainsKey(mapNode.ID)) return Vector3.zero;
        return mapNodeScreenPos[mapNode.ID];
    }

    public Vector3 GetStartPos(MapNode curNode)
    {
        if(map == null || map.sortedMapNodeList.Count == 0) return Vector3.zero;
        if(!mapNodeScreenPos.ContainsKey(curNode.ID)) return Vector3.zero;
        return mapNodeScreenPos[curNode.ID];
    }

    public void MovePlayerTo(MapNode mapNode)
    {
        if(mapNode == null || !mapNodeScreenPos.ContainsKey(mapNode.ID)) return;
        if(curNode.childNodes.Find(x => x == mapNode.ID) == null) return;
        player_moveable = false;
        GameObject moveRoad = mapLines[(curNode.ID, mapNode.ID)];
        if(moveRoad != null)
        {
            var newMapLine = Instantiate(mapLinePrefab, Vector3.zero, Utils.QI);
            newMapLine.transform.SetParent(mapTransform);
            newMapLine.transform.position = moveRoad.transform.position;
            newMapLine.transform.right = moveRoad.transform.right;
            newMapLine.transform.localScale = moveRoad.transform.localScale;
            SpriteRenderer sr = newMapLine.GetComponent<SpriteRenderer>();
            sr.sprite = lineSprites[0];
            sr.size = new Vector2(0f, 1f);
            sr.sortingOrder = 2;
            DOTween.To(() => sr.size, x => sr.size = x, new Vector2(moveRoad.GetComponent<SpriteRenderer>().size.x, 1f), 3f);
        }
        player.transform.DOMove(mapNodeScreenPos[mapNode.ID], 3f).OnComplete(() =>
        {
            curNode = mapNode;
            actSO.visitedNodeIDList.Add(mapNode.ID);
            actSO.curNodeIndex = map.sortedMapNodeList.IndexOf(mapNode);
            actSO.curNodeLocationID = mapNode.locationID;
            DataManager.Inst.SavePlayerData();
            SceneChangeManager.Inst.SceneFadeOut("EncounterScene");
        });
    }

    public void SetNewMap()
    {
        map = new Map();
        Act currentAct = actSO.acts.Find(x => x.actNum == actSO.curActNum);
        if(currentAct == null)
        {
            Debug.LogError("현재 막에 해당하는 맵 데이터가 없습니다. 현재 막 번호: " + actSO.curActNum);
            return;
        }
        map.CreateMap(currentAct, actSO.normalNodes);
        foreach(MapNode node in map.sortedMapNodeList)
        {
            LocationMetaInfo locInfo = locationDB.locationTable.Find(x => x.id == node.locationID);
            if (locInfo != null)
            {
                locInfo.selectedEncounterPool = new List<string>();
            }
        }
        actSO.visitedNodeIDList = new List<string>();
        PrintMap(map);
        SaveMap();
        actSO.curNodeIndex = 0;
        curNode = map.sortedMapNodeList[actSO.curNodeIndex];
        actSO.visitedNodeIDList.Add(curNode.ID);
        foreach(string nextNodeId in curNode.childNodes)
        {
            GetEncounterType(map.sortedMapNodeList.Find(x => x.ID == nextNodeId).locationID);
        }
        player.transform.position = GetStartPos(curNode);
        mapCamera = player.transform.GetComponentInChildren<MapCamera>();
        if(mapCamera != null)
        {
            mapCamera.minX = GetScreenPos(map.sortedMapNodeList[0]).x + 6f;
            mapCamera.maxX = GetScreenPos(map.sortedMapNodeList[map.sortedMapNodeList.Count - 1]).x - 6f;
        }
    }

    void Start()
    {
        if(actSO.mapSave != null && actSO.mapNodeScreenPosSave != null && actSO.mapSave.sortedMapNodeList != null && actSO.mapNodeScreenPosSave.Count != 0 && actSO.mapNodeScreenPosSave.Count == actSO.mapSave.sortedMapNodeList.Count)
        {
            map = actSO.mapSave;
            PrintMap(map, actSO.mapNodeScreenPosSave);
            curNode = map.sortedMapNodeList[actSO.curNodeIndex];
            foreach(string nextNodeId in curNode.childNodes)
            {
                GetEncounterType(map.sortedMapNodeList.Find(x => x.ID == nextNodeId).locationID);
            }
            player.transform.position = GetStartPos(curNode);
            mapCamera = player.transform.GetComponentInChildren<MapCamera>();
            if(mapCamera != null)
            {
                mapCamera.minX = GetScreenPos(map.sortedMapNodeList[0]).x + 6f;
                mapCamera.maxX = GetScreenPos(map.sortedMapNodeList[map.sortedMapNodeList.Count - 1]).x - 6f;
            }
        }
        else
        {
            SetNewMap();
        }
    }
}
