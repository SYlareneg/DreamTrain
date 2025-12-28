using UnityEngine;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using DG.Tweening;

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
    Dictionary<MapNode, Vector3> mapNodeScreenPos = new Dictionary<MapNode, Vector3>();

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
            float randOffset = Random.Range(-posDist / 2, posDist / 2);
            retVec.y += randOffset;
        }
        return retVec;
    }

    public void PrintMap(Map mp)
    {
        for(int i = 0; i < mp.sortedMapNodeList.Count; i++)
        {
            var newMapNode = Instantiate(mapNodePrefab, Vector3.zero, Utils.QI);
            MapNodeObject mapNodeObject = newMapNode.GetComponent<MapNodeObject>();
            mapNodeObject.mapNode = mp.sortedMapNodeList[i];
            Tooltip mapNodeTooltip = newMapNode.GetComponent<Tooltip>();
            mapNodeTooltip.tooltipTitle = mp.sortedMapNodeList[i].title;
            mapNodeTooltip.tooltipTxt = mp.sortedMapNodeList[i].text;
            newMapNode.transform.SetParent(mapTransform);
            if(i == 0 || i == mp.sortedMapNodeList.Count - 1)
            {
                newMapNode.transform.position = nodePos2ScreenPos(mp.sortedMapNodeList[i], false);
            }
            else
            {
                newMapNode.transform.position = nodePos2ScreenPos(mp.sortedMapNodeList[i], true);
            }
            mapNodeTooltip.tooltipPos = Camera.main.WorldToScreenPoint(newMapNode.transform.position);
            mapNodeTooltip.tooltipPos += tooltipOffset;
            mapNodeScreenPos.Add(mp.sortedMapNodeList[i], newMapNode.transform.position);
        }
        foreach(MapNode mapNode in mp.sortedMapNodeList)
        {
            foreach(MapNode childNode in mapNode.childNodes)
            {
                Vector3 linePos = (mapNodeScreenPos[mapNode] + mapNodeScreenPos[childNode]) / 2;
                var newMapLine = Instantiate(mapLinePrefab, Vector3.zero, Utils.QI);
                newMapLine.transform.SetParent(mapTransform);
                newMapLine.transform.position = linePos;
                Vector3 direction = mapNodeScreenPos[childNode] - mapNodeScreenPos[mapNode];
                newMapLine.transform.right = direction;
                newMapLine.transform.localScale = new Vector3(direction.magnitude, lineWidth, 1f);
            }
        }
    }

    public Vector3 GetScreenPos(MapNode mapNode)
    {
        if(mapNode == null || !mapNodeScreenPos.ContainsKey(mapNode)) return Vector3.zero;
        return mapNodeScreenPos[mapNode];
    }

    public Vector3 GetStartPos()
    {
        if(map == null || map.sortedMapNodeList.Count == 0) return Vector3.zero;
        if(!mapNodeScreenPos.ContainsKey(map.sortedMapNodeList[0])) return Vector3.zero;
        return mapNodeScreenPos[map.sortedMapNodeList[0]];
    }

    public void MovePlayerTo(MapNode mapNode)
    {
        if(mapNode == null || !mapNodeScreenPos.ContainsKey(mapNode)) return;
        if(curNode.childNodes.Find(x => x == mapNode) == null) return;
        player_moveable = false;
        player.transform.DOMove(mapNodeScreenPos[mapNode], 1f).OnComplete(() =>
        {
            curNode = mapNode;
            player_moveable = true;
        });
    }

    void Start()
    {
        map = new Map();
        map.CreateMap(8, actSO.acts[0], actSO.normalNodes);
        PrintMap(map);
        curNode = map.sortedMapNodeList[0];
        player.transform.position = GetStartPos();
        mapCamera = player.transform.GetComponentInChildren<MapCamera>();
        if(mapCamera != null)
        {
            mapCamera.minX = GetScreenPos(map.sortedMapNodeList[0]).x + 6f;
            mapCamera.maxX = GetScreenPos(map.sortedMapNodeList[map.sortedMapNodeList.Count - 1]).x - 6f;
        }
    }
}
