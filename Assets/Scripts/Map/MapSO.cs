using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;

[System.Serializable]
public class MapNode
{
    // 표기정보
    public Sprite nodeImg;
    public string title;
    public string text;
    public int encounterNum;
    // 분류정보
    public int difficulty;
    // 설정정보
    public int level;
    public int pos;
    [System.NonSerialized][HideInInspector] public List<MapNode> childNodes;

    public MapNode(MapNode mapNode)
    {
        nodeImg = mapNode.nodeImg;
        title = mapNode.title;
        text = mapNode.text;
        encounterNum = mapNode.encounterNum;
        difficulty = mapNode.difficulty;
        level = mapNode.level;
        pos = mapNode.pos;
        childNodes = mapNode.childNodes;
    }

    public void SetPos(int level, int pos)
    {
        this.level = level;
        this.pos = pos;
        this.childNodes = new List<MapNode>();
    }
}

[System.Serializable]
public class Act
{
    public List<MapNode> essentialNodes;
    public List<MapNode> specialNodes;
}

[System.Serializable]
public class Map
{
    public List<MapNode> sortedMapNodeList;
    public int totalLevel;
    public static int posValMin = -2;
    public static int posValMax = 2;

    public Map CreateMap(int nodeNum, Act act, List<MapNode> normalNodes)
    {
        if(nodeNum < act.essentialNodes.Count) return null;

        List<MapNode> essentialNodes_sorted = act.essentialNodes.OrderBy(node => node.difficulty).ToList();
        List<MapNode> prev_level_nodes = new List<MapNode>();
        List<MapNode> prev2_level_nodes = new List<MapNode>();
        int cur_level = 0;
        for(int i = 0; i < act.essentialNodes.Count - 1; i++)
        {
            // 노드 선택
            // 필수 노드 삽입
            MapNode essentialNode = new MapNode(essentialNodes_sorted[i]);
            essentialNode.SetPos(cur_level, (posValMin + posValMax) / 2);
            // 이전 두 층의 노드 중 자식이 없는 노드와 새로 삽입하는 필수 노드 연결
            foreach(MapNode prev_level_node in prev_level_nodes)
            {
                if(prev_level_node.childNodes.Count == 0)
                {
                    prev_level_node.childNodes.Add(essentialNode);
                }
            }
            foreach(MapNode prev2_level_node in prev2_level_nodes)
            {
                if(prev2_level_node.childNodes.Count == 0)
                {
                    prev2_level_node.childNodes.Add(essentialNode);
                }
            }
            prev2_level_nodes = new List<MapNode>();
            prev_level_nodes = new List<MapNode>();
            prev_level_nodes.Add(essentialNode);
            sortedMapNodeList.Add(essentialNode);
            cur_level++;
            // 삽입할 특수 & 공용 노드 개수
            int chooseNum = (nodeNum - act.essentialNodes.Count) / (act.essentialNodes.Count - 1);
            if(i == act.essentialNodes.Count - 2) chooseNum = nodeNum - act.essentialNodes.Count - chooseNum * (act.essentialNodes.Count - 2);
            // 삽입할 노드 최소 & 최대 난이도
            int minDiff = essentialNodes_sorted[i].difficulty;
            int maxDiff = essentialNodes_sorted[i+1].difficulty;
            // 삽입할 특수 & 공용 노드 선별
            List<MapNode> mapNodes_curDiff_special = new List<MapNode>();
            List<MapNode> mapNodes_curDiff_normal = new List<MapNode>();
            foreach(MapNode mapNode in act.specialNodes)
            {
                if(mapNode.difficulty >= minDiff && mapNode.difficulty < maxDiff)
                {
                    mapNodes_curDiff_special.Add(mapNode);
                }
            }
            foreach(MapNode mapNode in normalNodes)
            {
                if(mapNode.difficulty >= minDiff && mapNode.difficulty < maxDiff)
                {
                    mapNodes_curDiff_normal.Add(mapNode);
                }
            }
            // 특수 & 공용 노드 삽입
            int cur_level_num = 0;
            for(int j = 0; j < chooseNum; j += cur_level_num)
            {
                // 현재 층에 추가할 노드 개수
                cur_level_num = Random.Range(1, posValMax - posValMin + 2);
                if(j + cur_level_num > chooseNum) cur_level_num = chooseNum - j;
                // 현재 층에 추가할 노드 위치
                List<int> nodePos = new List<int>();
                for(int k = posValMin; k < posValMax + 1; k++)
                {
                    nodePos.Add(k);
                }
                nodePos = nodePos.OrderBy(x => Guid.NewGuid()).Take(cur_level_num).ToList();
                // 현재 층에 추가할 노드 선택
                List<MapNode> selectedNodes = new List<MapNode>();
                for(int k = 0; k < cur_level_num; k++)
                {
                    int randNodeIdx = Random.Range(0, mapNodes_curDiff_special.Count + mapNodes_curDiff_normal.Count);
                    MapNode mapNode = null;
                    if(randNodeIdx < mapNodes_curDiff_special.Count)
                    {
                        mapNode = new MapNode(mapNodes_curDiff_special[randNodeIdx]);
                        mapNodes_curDiff_special.RemoveAt(randNodeIdx);
                    }
                    else
                    {
                        randNodeIdx -= mapNodes_curDiff_special.Count;
                        mapNode = new MapNode(mapNodes_curDiff_normal[randNodeIdx]);
                    }
                    mapNode.SetPos(cur_level, nodePos[k]);
                    selectedNodes.Add(mapNode);
                }
                selectedNodes = selectedNodes.OrderBy(x => x.difficulty).ToList();
                foreach(MapNode mapNode in selectedNodes)
                {
                    MapNode closest_node = null;
                    foreach(MapNode prev_level_node in prev_level_nodes)
                    {
                        if(closest_node == null || Mathf.Abs(closest_node.pos - mapNode.pos) > Mathf.Abs(prev_level_node.pos - mapNode.pos))
                        {
                            closest_node = prev_level_node;
                        }
                    }
                    foreach(MapNode prev2_level_node in prev2_level_nodes)
                    {
                        if(closest_node == null || Mathf.Abs(closest_node.pos - mapNode.pos) > Mathf.Abs(prev2_level_node.pos - mapNode.pos))
                        {
                            closest_node = prev2_level_node;
                        }
                    }
                    if(closest_node == null)
                    {
                        // 오류상황
                        break;
                    }
                    closest_node.childNodes.Add(mapNode);
                    sortedMapNodeList.Add(mapNode);
                }
                foreach(MapNode prev2_level_node in prev2_level_nodes)
                {
                    if(prev2_level_node.childNodes.Count == 0)
                    {
                        MapNode closest_node = null;
                        foreach(MapNode mapNode in selectedNodes)
                        {
                            if(closest_node == null || Mathf.Abs(closest_node.pos - prev2_level_node.pos) > Mathf.Abs(mapNode.pos - prev2_level_node.pos))
                            {
                                closest_node = mapNode;
                            }
                        }
                        if(closest_node == null)
                        {
                            // 오류상황
                            break;
                        }
                        prev2_level_node.childNodes.Add(closest_node);
                    }
                }
                prev2_level_nodes = prev_level_nodes;
                prev_level_nodes = selectedNodes;
                cur_level++;
            }
        }
        MapNode essentialNode_last = new MapNode(essentialNodes_sorted[act.essentialNodes.Count - 1]);
        essentialNode_last.SetPos(cur_level, (posValMin + posValMax) / 2);
        // 이전 두 층의 노드 중 자식이 없는 노드와 새로 삽입하는 필수 노드 연결
        foreach(MapNode prev_level_node in prev_level_nodes)
        {
            if(prev_level_node.childNodes.Count == 0)
            {
                prev_level_node.childNodes.Add(essentialNode_last);
            }
        }
        foreach(MapNode prev2_level_node in prev2_level_nodes)
        {
            if(prev2_level_node.childNodes.Count == 0)
            {
                prev2_level_node.childNodes.Add(essentialNode_last);
            }
        }
        sortedMapNodeList.Add(essentialNode_last);
        cur_level++;
        totalLevel = cur_level;
        return this;
    }
}

[CreateAssetMenu(fileName = "ActSO", menuName = "Scriptable Objects/ActSO")]
public class ActSO : ScriptableObject
{
    public List<MapNode> normalNodes;
    public List<Act> acts;
}

[CreateAssetMenu(fileName = "MapSO", menuName = "Scriptable Objects/MapSO")]
public class MapSO : ScriptableObject
{
    
}
