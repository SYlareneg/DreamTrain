using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterSO", menuName = "Scriptable Objects/CharacterSO")]
public class CharacterSO : ScriptableObject
{
    [Header("Develop")]
    [Tooltip("플레이어 최대 체력")] public int maxHealth;
    [Tooltip("플레이어 남은 체력")] public int curHealth;

    [Tooltip("적 최대 체력")] public int enemyMaxHealth;
    [Tooltip("적 남은 체력")] public int enemyCurHealth;

    [Tooltip("적이 취할 수 있는 행동의 최대값\n(예: 2일 경우 최대 2칸만 회전 가능)")] public int enemyMaxActionVal;
    [Tooltip("적이 취할 수 있는 행동의 개수\n(예: 2일 경우 적은 2가지 행동 중 선택)")] public int enemyActionNum;

    [Tooltip("플레이어 트리거 발동조건")] public int playerTriggerMaxCnt;
    [Tooltip("적 트리거 발동조건")] public int enemyTriggerMaxCnt;

    public DreamPiece personaPiece;
    public DreamPiece shadowPiece;
}
