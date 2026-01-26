using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "CharacterSO", menuName = "Scriptable Objects/CharacterSO")]
public class CharacterSO : ScriptableObject
{
    [Header("Develop")]
    [Tooltip("플레이어 최대 체력")] public int maxHealth;
    [Tooltip("플레이어 남은 체력")] public int curHealth;
    public int dreamDust;
    public int leftPassengers;
    public bool isTutorial;
    public string lastSceneName;

    public DreamPiece_Player personaPiece;
    public DreamPiece_Player shadowPiece;
    public List<Item> normalCards;

    public string enemyName;
}
