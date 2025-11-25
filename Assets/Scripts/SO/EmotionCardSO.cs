using UnityEngine;

[CreateAssetMenu(fileName = "EmotionCardData", menuName = "Dialogue/EmotionCardData")]
public class EmotionCardSO : ScriptableObject
{
    [System.Serializable]
    public struct EmotionCardData
    {
        public FeelingType type;
        public string cardName;
        public Sprite cardSprite;
    }

    public EmotionCardData[] emotionCards; 
}