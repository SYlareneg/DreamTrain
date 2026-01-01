using UnityEngine;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class MapManager : MonoBehaviour
{
    public static MapManager Inst;
    void Awake() => Inst = this;

    [SerializeField] ActSO actSO;
    [SerializeField] Transform mapTransform;
    [SerializeField] GameObject mapNodePrefab;
    [SerializeField] GameObject mapLinePrefab;
    [SerializeField] float lineWidth;
    [SerializeField] Vector3 zeroPos;
    [SerializeField] float levelDist;
    [SerializeField] float posDist;
    public Vector2 tooltipOffset;
    public Map map;
    Dictionary<string, Vector3> mapNodeScreenPos = new Dictionary<string, Vector3>();

    public GameObject player;
    public MapCamera mapCamera;
    public bool player_moveable = true;
    [HideInInspector] public MapNode curNode = null;

    public Vector3 nodePos2ScreenPos(MapNode mapNode, bool addOffset = false)
    {
        Vector3 retVec = zeroPos;
        retVec.x += mapNode.level * levelDist;
        retVec.y += mapNode.pos * posDist;
        if(addOffset)
        {
            float randOffset = Random.Range(-posDist / 4, posDist / 4);
            retVec.y += randOffset;
        }
        return retVec;
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
            newMapNode.GetComponent<SpriteRenderer>().sprite = mp.sortedMapNodeList[i].nodeImg;
            MapNodeObject mapNodeObject = newMapNode.GetComponent<MapNodeObject>();
            mapNodeObject.mapNode = mp.sortedMapNodeList[i];
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
            mapNodeTooltip.tooltipPos = Camera.main.WorldToScreenPoint(newMapNode.transform.position) - Camera.main.WorldToScreenPoint(Vector3.zero);
            mapNodeTooltip.tooltipPos += tooltipOffset;
            mapNodeScreenPos.Add(mp.sortedMapNodeList[i].ID, newMapNode.transform.position);
        }
        foreach(MapNode mapNode in mp.sortedMapNodeList)
        {
            foreach(string childNode in mapNode.childNodes)
            {
                Vector3 linePos = (mapNodeScreenPos[mapNode.ID] + mapNodeScreenPos[childNode]) / 2;
                var newMapLine = Instantiate(mapLinePrefab, Vector3.zero, Utils.QI);
                newMapLine.transform.SetParent(mapTransform);
                newMapLine.transform.position = linePos;
                Vector3 direction = mapNodeScreenPos[childNode] - mapNodeScreenPos[mapNode.ID];
                newMapLine.transform.right = direction;
                newMapLine.transform.localScale = new Vector3(direction.magnitude, lineWidth, 1f);
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
        player.transform.DOMove(mapNodeScreenPos[mapNode.ID], 1f).OnComplete(() =>
        {
            curNode = mapNode;
            actSO.curNodeIndex = map.sortedMapNodeList.IndexOf(mapNode);
            actSO.curNodeLocationID = mapNode.locationID;
            SceneManager.LoadScene("EncounterScene");
        });
    }

    public void SetNewMap()
    {
        map = new Map();
        map.CreateMap(actSO.acts[actSO.curActIndex], actSO.normalNodes);
        PrintMap(map);
        SaveMap();
        actSO.curNodeIndex = 0;
        curNode = map.sortedMapNodeList[actSO.curNodeIndex];
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
