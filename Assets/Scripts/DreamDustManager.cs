using UnityEngine;
using TMPro;

public class DreamDustManager : MonoBehaviour
{
    [SerializeField] private int dreamDust = 5;
    [SerializeField] private TextMeshProUGUI dustCountText;

    void Start()
    {
        UpdateDustUI();
    }

    public int GetDreamDust()
    {
        return dreamDust;
    }

    public void SetDreamDust(int newValue)
    {
        dreamDust = Mathf.Max(0, newValue); // 음수 방지
        UpdateDustUI();
    }

    public void AddDust(int amount)
    {
        dreamDust = Mathf.Max(0, dreamDust + amount);
        UpdateDustUI();
    }

    public void UseDust(int amount)
    {
        dreamDust = Mathf.Max(0, dreamDust - amount);
        UpdateDustUI();
    }

    private void UpdateDustUI()
    {
        if (dustCountText != null)
        {
            dustCountText.text = dreamDust.ToString();
        }
        else
        {
            Debug.LogWarning("[dreamDustControll] DustCnt 텍스트가 연결되지 않았습니다.");
        }
    }
}