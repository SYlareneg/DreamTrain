using UnityEngine;

[System.Serializable]
public class Keyword
{
    public string word;
    public string explanation;
}

[CreateAssetMenu(fileName = "KeywordSO", menuName = "Scriptable Objects/KeywordSO")]
public class KeywordSO : ScriptableObject
{
    public Keyword[] keywords;
}
