using UnityEngine;
using System.Collections.Generic;

public enum EncounterStepType
{
    DESC,   
    BRANCH,  
    BATTLE,   
    SHOP, 
    END    
}

[System.Serializable]
public class EncounterStep
{
    public string id;  
    public EncounterStepType type; 
    
    [TextArea(3, 10)] 
    public string textContent;

    public string functionCall;
    
    // BRANCH 타입 선택지들
    public List<EncounterOption> options; 
    
    // DESC 타입 다음 ID
    public string nextStepId;
}

[System.Serializable]
public class EncounterOption
{
    public string text;  // 선택지
    public string nextStepId;  
    public string condition;  // 선택 조건
    public string functionCall;
}

[CreateAssetMenu(fileName = "NewEncounterScenario", menuName = "TRPG/Encounter Scenario")]
public class EncounterScenarioSO : ScriptableObject
{
    public string encounterId;    
    public string encounterName;
    public Sprite defaultImage; 
    
    public List<EncounterStep> steps; 
    
    public EncounterStep GetStep(string id)
    {
        return steps.Find(s => s.id == id);
    }
}