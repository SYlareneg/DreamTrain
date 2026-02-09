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
    public Sprite hideNodeImg;
    public string title;
    public string text;
    public int encounterNum;
    // 분류정보
    public string ID;
    public string locationID;
    public int difficulty;
    // 설정정보
    public int level;
    public int pos;
    public List<string> childNodes;

    public MapNode(MapNode mapNode)
    {
        nodeImg = mapNode.nodeImg;
        hideNodeImg = mapNode.hideNodeImg;
        title = mapNode.title;
        text = mapNode.text;
        encounterNum = mapNode.encounterNum;
        ID = Guid.NewGuid().ToString("N");
        locationID = mapNode.locationID;
        difficulty = mapNode.difficulty;
        level = mapNode.level;
        pos = mapNode.pos;
        childNodes = mapNode.childNodes;
    }

    public MapNode(Location_Data locData)
    {
        nodeImg = Utils.LoadSpriteByName("LocationIcons", locData.sprite);
        hideNodeImg = Utils.LoadSpriteByName("LocationIcons", locData.hideSprite);
        title = locData.nameKO;
        text = locData.descriptionKO;
        encounterNum = locData.encounterNum;
        ID = "";
        locationID = locData.id;
        difficulty = locData.difficulty;
        level = 0;
        pos = 0;
        childNodes = new List<string>();
    }

    public MapNode(MapNode_Data mapNodeData, List<Location_Data> locationDataList)
    {
        Location_Data locData = locationDataList.Find(loc => loc.id == mapNodeData.locationID);
        if(locData == null) return;
        nodeImg = Utils.LoadSpriteByName("LocationIcons", locData.sprite);
        hideNodeImg = Utils.LoadSpriteByName("LocationIcons", locData.hideSprite);
        title = locData.nameKO;
        text = locData.descriptionKO;
        encounterNum = locData.encounterNum;
        ID = mapNodeData.ID;
        locationID = mapNodeData.locationID;
        difficulty = locData.difficulty;
        level = mapNodeData.level;
        pos = mapNodeData.pos;
        childNodes = new List<string>(mapNodeData.childNodes);
    }

    public void SetPos(int level, int pos)
    {
        this.level = level;
        this.pos = pos;
        this.childNodes = new List<string>();
    }
}

[System.Serializable]
public class MapNode_Data
{
    public string locationID;
    public List<string> selectedEncounterPool;
    public string ID;
    public int level;
    public int pos;
    public Vector3 screenPos;
    public List<string> childNodes;

    public MapNode_Data() { }

    public MapNode_Data(MapNode mapNode, Vector3 screenPos)
    {
        locationID = mapNode.locationID;
        selectedEncounterPool = new List<string>();
        ID = mapNode.ID;
        level = mapNode.level;
        pos = mapNode.pos;
        this.screenPos = screenPos;
        childNodes = new List<string>(mapNode.childNodes);
    }
}

[System.Serializable]
public class Act
{
    public int actNum;
    public List<MapNode> essentialNodes;
    public List<int> essentialIntervalLayerCount;
    public List<MapNode> specialNodes;

    public Act(Act_Data actData, List<Location_Data> locationData)
    {
        actNum = actData.actNum;
        essentialNodes = new List<MapNode>();
        essentialIntervalLayerCount = new List<int>(actData.essentialIntervalLayerCount);
        specialNodes = new List<MapNode>();
        foreach(var loc in locationData)
        {
            if(loc.isNormalLocation) continue;
            if(actData.essentialLocations.Contains(loc.id))
            {
                MapNode essentialNode = new MapNode(loc);
                essentialNodes.Add(essentialNode);
            }
            else if(actData.specialLocations.Contains(loc.id))
            {
                MapNode specialNode = new MapNode(loc);
                specialNodes.Add(specialNode);
            }
        }
    }
}

[System.Serializable]
public class Map
{
    public List<MapNode> sortedMapNodeList;
    public int totalLevel;
    public static int posValMin = -2;
    public static int posValMax = 2;
    public static int layerMaxNodeNum = 4;
    public static float probability = 0.9f; // layer num이 2, 3일 확률

    public Map CreateMap(Act act, List<MapNode> normalNodes)
    {
        sortedMapNodeList = new List<MapNode>();

        List<MapNode> essentialNodes_sorted = act.essentialNodes.OrderBy(node => node.difficulty).ToList();
        List<MapNode> prev_level_nodes = new List<MapNode>();
        List<MapNode> prev2_level_nodes = new List<MapNode>();
        int essentialLayerCount = 0;
        for(int i = 0; i < act.essentialNodes.Count; i++)
        {
            int layerCount = 1;
            while(i + 1 < act.essentialNodes.Count && layerCount < (posValMax - posValMin + 1) && essentialNodes_sorted[i].difficulty == essentialNodes_sorted[i + 1].difficulty)
            {
                i++;
                layerCount++;
            }
            essentialLayerCount++;
        }
        if(act.essentialIntervalLayerCount == null || act.essentialIntervalLayerCount.Count != essentialLayerCount - 1) return null;
        int cur_level = 0;
        int essentialLayerIdx = 0;
        for(int i = 0; i < act.essentialNodes.Count - 1; i++)
        {
            // 노드 선택
            // 필수 노드 삽입
            List<MapNode> essentialNodes_samelayer = new List<MapNode>();
            essentialNodes_samelayer.Add(new MapNode(essentialNodes_sorted[i]));
            while(i + 1 < act.essentialNodes.Count - 1 && essentialNodes_samelayer.Count < (posValMax - posValMin + 1) && essentialNodes_sorted[i].difficulty == essentialNodes_sorted[i + 1].difficulty)
            {
                i++;
                essentialNodes_samelayer.Add(new MapNode(essentialNodes_sorted[i]));
            }
            for(int j = 0; j < essentialNodes_samelayer.Count; j++)
            {
                essentialNodes_samelayer[j].SetPos(cur_level, (posValMin + posValMax) / 2 + (j + 1) / 2 * ((j % 2 == 0) ? 1 : -1));
                MapNode closest_node = null;
                foreach(MapNode prev_level_node in prev_level_nodes)
                {
                    if(closest_node == null || Mathf.Abs(closest_node.pos - essentialNodes_samelayer[j].pos) > Mathf.Abs(prev_level_node.pos - essentialNodes_samelayer[j].pos))
                    {
                        closest_node = prev_level_node;
                    }
                }
                foreach(MapNode prev2_level_node in prev2_level_nodes)
                {
                    if(closest_node == null || Mathf.Abs(closest_node.pos - essentialNodes_samelayer[j].pos) > Mathf.Abs(prev2_level_node.pos - essentialNodes_samelayer[j].pos))
                    {
                        closest_node = prev2_level_node;
                    }
                }
                if(closest_node == null)
                {
                    // 오류상황
                    break;
                }
                closest_node.childNodes.Add(essentialNodes_samelayer[j].ID);
            }
            // 이전 두 층의 노드 중 자식이 없는 노드와 새로 삽입하는 필수 노드 연결
            foreach(MapNode prev_level_node in prev_level_nodes)
            {
                if(prev_level_node.childNodes.Count == 0)
                {
                    MapNode closest_node = null;
                    foreach(MapNode mapNode in essentialNodes_samelayer)
                    {
                        if(closest_node == null || Mathf.Abs(closest_node.pos - prev_level_node.pos) > Mathf.Abs(mapNode.pos - prev_level_node.pos))
                        {
                            closest_node = mapNode;
                        }
                    }
                    if(closest_node == null)
                    {
                        // 오류상황
                        break;
                    }
                    prev_level_node.childNodes.Add(closest_node.ID);
                }
            }
            foreach(MapNode prev2_level_node in prev2_level_nodes)
            {
                if(prev2_level_node.childNodes.Count == 0)
                {
                    MapNode closest_node = null;
                    foreach(MapNode mapNode in essentialNodes_samelayer)
                    {
                        if(closest_node == null || Mathf.Abs(closest_node.pos - prev2_level_node.pos) > Mathf.Abs(mapNode.pos - prev2_level_node.pos))
                        {
                            closest_node = mapNode;
                        }
                    }
                    foreach(MapNode mapNode in prev_level_nodes)
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
                    prev2_level_node.childNodes.Add(closest_node.ID);
                }
            }
            prev2_level_nodes = new List<MapNode>();
            prev_level_nodes = new List<MapNode>();
            foreach(MapNode essentialNode in essentialNodes_samelayer)
            {
                prev_level_nodes.Add(essentialNode);
                sortedMapNodeList.Add(essentialNode);
            }
            cur_level++;
            essentialLayerIdx++;
            // 삽입할 특수 & 공용 노드 개수
            // int chooseNum = (nodeNum - act.essentialNodes.Count) / (essentialLayerCount - 1);
            // if(i == act.essentialNodes.Count - 2) chooseNum = nodeNum - act.essentialNodes.Count - chooseNum * (essentialLayerCount - 2);
            int chooseNum = act.essentialIntervalLayerCount[essentialLayerIdx - 1];
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
            //for(int j = 0; j < chooseNum; j += cur_level_num)
            for(int j = 0; j < chooseNum; j++)
            {
                // 현재 층에 추가할 노드 개수
                int[] high_prob_layer_nums = new int[] {2, 3};
                int[] low_prob_layer_nums = new int[] {1, 4};
                if(Random.value < probability) cur_level_num = high_prob_layer_nums[Random.Range(0, high_prob_layer_nums.Length)];
                else cur_level_num = low_prob_layer_nums[Random.Range(0, low_prob_layer_nums.Length)];

                if(mapNodes_curDiff_normal.Count + mapNodes_curDiff_special.Count - cur_level_num < chooseNum - j - 1)
                {
                    cur_level_num = mapNodes_curDiff_normal.Count + mapNodes_curDiff_special.Count - (chooseNum - j - 1);
                }
                //cur_level_num = Random.Range(1, layerMaxNodeNum + 1);
                //if(j + cur_level_num > chooseNum) cur_level_num = chooseNum - j;
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
                    closest_node.childNodes.Add(mapNode.ID);
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
                        foreach(MapNode mapNode in prev_level_nodes)
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
                        prev2_level_node.childNodes.Add(closest_node.ID);
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
                prev_level_node.childNodes.Add(essentialNode_last.ID);
            }
        }
        foreach(MapNode prev2_level_node in prev2_level_nodes)
        {
            if(prev2_level_node.childNodes.Count == 0)
            {
                prev2_level_node.childNodes.Add(essentialNode_last.ID);
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

    public int curActNum;
    public Map mapSave;
    public List<Vector3> mapNodeScreenPosSave;
    public List<string> visitedNodeIDList = new List<string>();
    public int curNodeIndex;
    public string curNodeLocationID;
    public EncounterType lastEncounterType;
    
    public List<string> encounterQueue = new List<string>(); 
    public string currentStepID;
    public string currentEncounterID;
}
