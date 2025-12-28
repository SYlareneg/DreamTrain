using UnityEngine;
using System.Collections.Generic;

public enum EncounterType
{
    Battle,      
    DangerousBattle,  
    BossBattle,       
    Exploration,    
    Merchant,         
    Rest,           
    Special,            
    Essential        
}

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
    
    public List<EncounterOption> options; 
    public string nextStepId;
}

[System.Serializable]
public class EncounterOption
{
    public string text;
    public string nextStepId;  
    public string condition;
    public string functionCall;
}

[CreateAssetMenu(fileName = "NewEncounterScenario", menuName = "TRPG/Encounter Scenario")]
public class EncounterScenarioSO : ScriptableObject
{
    [Header("Basic Info")]
    public string encounterId;    
    public string encounterName;
    public Sprite defaultImage;

    [Header("Type Info")]
    public EncounterType encounterType; 
    
    [Header("Scenario Data")]
    public List<EncounterStep> steps; 

    public EncounterType GetEncounterType()
    {
        return encounterType;
    }

    public EncounterStep GetStep(string id)
    {
        return steps.Find(s => s.id == id);
    }
}