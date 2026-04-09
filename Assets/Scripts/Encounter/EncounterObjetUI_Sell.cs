    using UnityEngine;
    using UnityEngine.UI;
    using UnityEngine.EventSystems; // 포인터 이벤트 처리를 위해 필수
    using TMPro;
    using System.Collections.Generic; // 리스트 사용을 위해

    public class EncounterObjetUI_Sell : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("UI References")]
        [SerializeField] Image objetIcon;         
        [SerializeField] TMP_Text nameTMP;        
        [SerializeField] public TMP_Text sellCostTMP;
        
        [Header("Tooltip Settings")]
        public GameObject cardUITooltipPrefab;
        public Transform tooltipPos;
        public TMP_Text tooltipTitleTMP;
        public TMP_Text tooltipDescTMP;
        
        private RelicItem_Data relicData;   
        
        public int cost;                
        public bool isValid;
        public bool isJunk;
        
        public CharacterSO _playerData; 
        private System.Action _onBuyRequest;
        private bool _wasAffordable = true;

        private List<GameObject> activeTooltips = new List<GameObject>();
        private Vector3 originalScale;
        private bool tooltipCreated = false;

        private void Awake()
        {
            originalScale = transform.localScale;
            
            if (tooltipPos == null) tooltipPos = this.transform;
        }

        public void Setup(RelicItem_Data data, int cost, bool isValid, CharacterSO playerData, bool isJunk, System.Action onBuyRequest)
        {
            this.relicData = data;
            this.cost = cost;
            this.isValid = isValid;
            this._playerData = playerData;
            this._onBuyRequest = onBuyRequest;
            this.isJunk = isJunk;

            if (relicData != null) 
            {
                UpdateObjetVisual(relicData);
            }

            if (sellCostTMP != null)
            {
                sellCostTMP.text = this.cost.ToString();
                UpdateColor(true);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!isValid || _onBuyRequest == null) return;
            _onBuyRequest.Invoke();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            this.transform.localScale = originalScale * 1.3f;
            if (relicData != null && !tooltipCreated && cardUITooltipPrefab != null)
            {
                var tooltipObj = Instantiate(cardUITooltipPrefab, tooltipPos.position, Quaternion.identity);
                
                GameObject mainCanvas = GameObject.FindGameObjectWithTag("MainCanvas");
                if (mainCanvas != null)
                {
                    tooltipObj.transform.SetParent(mainCanvas.transform, true);
                }
                else
                {
                    Canvas parentCanvas = GetComponentInParent<Canvas>();
                    if (parentCanvas != null) tooltipObj.transform.SetParent(parentCanvas.transform, true);
                }

                tooltipObj.transform.SetAsLastSibling(); 
                
                // tooltipObj.transform.localScale *= 0.8f; 
                

                activeTooltips.Add(tooltipObj);

                CardTooltip cardTooltip = tooltipObj.GetComponent<CardTooltip>();
                if (cardTooltip != null)
                {
                    cardTooltip.SetTooltip(relicData.relicName, relicData.relicTxt);
                }
                tooltipCreated = true;
            }
            // if(SoundManager.Inst != null && SoundManager.Inst.UISelectSFX != null) 
            //     GetComponent<AudioSource>().PlayOneShot(SoundManager.Inst.UISelectSFX);
        }
        
        

        public void OnPointerExit(PointerEventData eventData)
        {
            this.transform.localScale = originalScale;
            foreach (GameObject tooltip in activeTooltips)
            {
                if (tooltip != null) Destroy(tooltip);
            }
            activeTooltips.Clear();
            tooltipCreated = false;
        }

        void UpdateObjetVisual(RelicItem_Data data)
        {
            if (objetIcon != null)
            {
                string spriteName = data.relicName; 
                Sprite sprite = Resources.Load<Sprite>($"Relics/MerchantObjet/{spriteName}");
                Debug.Log("SpriteName: "+ spriteName);
                if (sprite != null)
                {
                    objetIcon.sprite = sprite;
                    objetIcon.gameObject.SetActive(true);
                }
                else
                {
                    Sprite notDrawn = Resources.Load<Sprite>($"Relics/MerchantObjet/Not drawn yet");
                    if (notDrawn != null)
                    {
                        objetIcon.sprite = notDrawn;
                        objetIcon.gameObject.SetActive(true);
                    }
                    objetIcon.color = Color.clear; 
                }
            }

            if (nameTMP != null)
            {
                nameTMP.text = data.relicName;
                nameTMP.gameObject.SetActive(true);
            }
        }

        private void Update()
        {
            if (_playerData == null || !isValid || sellCostTMP == null) return;
            if (isJunk) return;
            
            bool isAffordable = _playerData.dreamDust >= cost;
            if (isAffordable != _wasAffordable)
            {
                UpdateColor(isAffordable);
                _wasAffordable = isAffordable;
            }
        }

        void UpdateColor(bool isAffordable)
        {
            if (sellCostTMP == null) return;
            sellCostTMP.color = isAffordable ? Color.white : Color.red;
        }
        
        private void OnDisable()
        {
            this.transform.localScale = originalScale;
    
            foreach (GameObject tooltip in activeTooltips)
            {
                if (tooltip != null) Destroy(tooltip);
            }
    
            activeTooltips.Clear();
            tooltipCreated = false;
        }
    }