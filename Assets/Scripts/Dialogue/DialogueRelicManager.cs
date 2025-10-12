using UnityEngine;

public class DialogueRelicManager : MonoBehaviour
{
    public static DialogueRelicManager Inst { get; private set; }
    void Awake() => Inst = this;
    public RelicSO relicSO;
    public RelicSO playerRelicSO;
    public int[] relicWeights;

    private void Start()
    {
        relicWeights = new int[relicSO.relicItems.Count + 1];
        for(int i = 0; i <= relicSO.relicItems.Count; i++)
        {
            relicWeights[i] = 0;
        }
    }

    public void ResetPlayerRelics()
    {
        playerRelicSO.relicItems.Clear();
    }

    public int GetMaxWeightIndex()
    {
        int maxWeight = 0;
        int maxIdx = 0;
        for (int i = 1; i < relicWeights.Length; i++)
        {
            if (maxWeight < relicWeights[i])
            {
                maxWeight = relicWeights[i];
                maxIdx = i;
            }
        }
        return maxIdx;
    }

    public void AddPlayerRelic(int idx)
    {
        if(idx >= 0 && idx < relicSO.relicItems.Count)
        {
            if (playerRelicSO.relicItems.Find(x => x.relicName == relicSO.relicItems[idx].relicName) == null)
            {
                playerRelicSO.relicItems.Add(relicSO.relicItems[idx]);
            }
        }
    }
}
