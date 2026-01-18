    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public enum CardType { Turn, Enchant, Skill, Dream };
    public enum CardRarity { Normal, Rare };
    public enum ECardValueType { Damage, Heal, Shield, Special, Default };

    [System.Serializable]
    public class Item
    {
        [Tooltip("카드 제목")] public string name;
        [Tooltip("카드 코스트")] public int cost;
        [Tooltip("카드 타입")] public CardType type;
        [Tooltip("카드 희귀도")] public CardRarity rarity;
        [Tooltip("카드가 속한 꿈 조각 번호")] public int dreamPieceNum;
        [Tooltip("휘발성")] public bool isVolatile;
        [Tooltip("소멸")] public bool isVanish;
        [Tooltip("잔류")] public bool isRemain;
        [Tooltip("단일기")] public bool isSingleTarget;
        [Tooltip("카드 이미지")] public Sprite sprite;
        [Tooltip("카드 설명")] public string text;
        [Tooltip("카드 계수")] public List<int> cardValues = new List<int>();
        [Tooltip("카드 계수 타입")] public List<ECardValueType> cardValueTypes = new List<ECardValueType>();
        [Tooltip("덱 내 카드 장수")] public int num;
        [Tooltip("강화 여부")] public bool isEnhanced;

        public void SetItem(Item item)
        {
            if(item == null) return;
            name = item.name;
            cost = item.cost;
            type = item.type;
            rarity = item.rarity;
            dreamPieceNum = item.dreamPieceNum;
            isVolatile = item.isVolatile;
            isVanish = item.isVanish;
            isRemain = item.isRemain;
            sprite = item.sprite;
            text = item.text;
            cardValues.Clear();
            foreach(var v in item.cardValues) cardValues.Add(v);
            cardValueTypes.Clear();
            foreach(var v in item.cardValueTypes) cardValueTypes.Add(v);
            num = item.num;
            isEnhanced = item.isEnhanced;
        }

        public Item() { }

        public Item(Item item)
        {
            SetItem(item);
        }

        public Item(Item_Data item_Data, bool isEnhanced)
        {
            if(item_Data == null) return;
            if(isEnhanced)
            {
                name = item_Data.name_enhanced;
                cost = item_Data.cost_enhanced;
                type = item_Data.type;
                rarity = item_Data.rarity;
                dreamPieceNum = item_Data.dreamPieceNum;
                isVolatile = item_Data.isVolatile_enhanced;
                isVanish = item_Data.isVanish_enhanced;
                isRemain = item_Data.isRemain_enhanced;
                isSingleTarget = item_Data.isSingleTarget_enhanced;
                sprite = Utils.LoadSpriteByName("Cards", item_Data.sprite);
                text = item_Data.text_enhanced;
                cardValues = new List<int>(item_Data.cardValues_enhanced);
                cardValueTypes = new List<ECardValueType>(item_Data.cardValueTypes_enhanced);
                this.isEnhanced = true;
            }
            else
            {
                name = item_Data.name;
                cost = item_Data.cost;
                type = item_Data.type;
                rarity = item_Data.rarity;
                dreamPieceNum = item_Data.dreamPieceNum;
                isVolatile = item_Data.isVolatile;
                isVanish = item_Data.isVanish;
                isRemain = item_Data.isRemain;
                isSingleTarget = item_Data.isSingleTarget;
                sprite = Utils.LoadSpriteByName("Cards", item_Data.sprite);
                text = item_Data.text;
                cardValues = new List<int>(item_Data.cardValues);
                cardValueTypes = new List<ECardValueType>(item_Data.cardValueTypes);
                this.isEnhanced = false;
            }
        }
    }

    [System.Serializable]
    public class Item_Enhanceable : Item
    {
        [Tooltip("강화 카드")] public Item enhancedItem;

        public Item_Enhanceable(Item_Enhanceable item)
        {
            if(item == null) return;
            this.SetItem((Item)item);
            this.enhancedItem = new Item();
            this.enhancedItem.SetItem(item.enhancedItem);
        }

        public Item_Enhanceable(Item_Data item_Data)
        {
            if(item_Data == null) return;
            this.name = item_Data.name;
            this.cost = item_Data.cost;
            this.type = item_Data.type;
            this.rarity = item_Data.rarity;
            this.dreamPieceNum = item_Data.dreamPieceNum;
            this.isVolatile = item_Data.isVolatile;
            this.isVanish = item_Data.isVanish;
            this.isRemain = item_Data.isRemain;
            this.isSingleTarget = item_Data.isSingleTarget;
            this.sprite = Utils.LoadSpriteByName("Cards", item_Data.sprite);
            this.text = item_Data.text;
            this.cardValues = new List<int>(item_Data.cardValues);
            this.cardValueTypes = new List<ECardValueType>(item_Data.cardValueTypes);
            this.num = 0;
            this.isEnhanced = false;

            this.enhancedItem = new Item();
            this.enhancedItem.name = item_Data.name_enhanced;
            this.enhancedItem.cost = item_Data.cost_enhanced;
            this.enhancedItem.type = item_Data.type;
            this.enhancedItem.rarity = item_Data.rarity;
            this.enhancedItem.dreamPieceNum = item_Data.dreamPieceNum;
            this.enhancedItem.isVolatile = item_Data.isVolatile_enhanced;
            this.enhancedItem.isVanish = item_Data.isVanish_enhanced;
            this.enhancedItem.isRemain = item_Data.isRemain_enhanced;
            this.enhancedItem.isSingleTarget = item_Data.isSingleTarget_enhanced;
            this.enhancedItem.sprite = this.sprite;
            this.enhancedItem.text = item_Data.text_enhanced;
            this.enhancedItem.cardValues = new List<int>(item_Data.cardValues_enhanced);
            this.enhancedItem.cardValueTypes = new List<ECardValueType>(item_Data.cardValueTypes_enhanced);
            this.enhancedItem.num = 0;
            this.enhancedItem.isEnhanced = true;
        }
    }

    [CreateAssetMenu(fileName = "ItemSO", menuName = "Scriptable Objects/ItemSO")]
    public class ItemSO : ScriptableObject
    {
        public List<Item> items;
    }

    [System.Serializable]
    public class Item_Data
    {
        [Tooltip("카드 제목")] public string name;
        [Tooltip("카드 코스트")] public int cost;
        [Tooltip("카드 타입")] public CardType type;
        [Tooltip("카드 희귀도")] public CardRarity rarity;
        [Tooltip("카드가 속한 꿈 조각 번호")] public int dreamPieceNum;
        [Tooltip("휘발성")] public bool isVolatile;
        [Tooltip("소멸")] public bool isVanish;
        [Tooltip("잔류")] public bool isRemain;
        [Tooltip("단일기")] public bool isSingleTarget;
        [Tooltip("카드 이미지")] public string sprite;
        [Tooltip("카드 설명")] public string text;
        [Tooltip("카드 계수")] public List<int> cardValues = new List<int>();
        [Tooltip("카드 계수 타입")] public List<ECardValueType> cardValueTypes = new List<ECardValueType>();
        [Tooltip("카드 제목(강화)")] public string name_enhanced;
        [Tooltip("카드 코스트(강화)")] public int cost_enhanced;
        [Tooltip("휘발성(강화)")] public bool isVolatile_enhanced;
        [Tooltip("소멸(강화)")] public bool isVanish_enhanced;
        [Tooltip("잔류(강화)")] public bool isRemain_enhanced;
        [Tooltip("단일기(강화)")] public bool isSingleTarget_enhanced;
        [Tooltip("카드 설명(강화)")] public string text_enhanced;
        [Tooltip("카드 계수(강화)")] public List<int> cardValues_enhanced = new List<int>();
        [Tooltip("카드 계수 타입(강화)")] public List<ECardValueType> cardValueTypes_enhanced = new List<ECardValueType>();

        public Item_Data() { }

        public Item_Data(Item_Data data)
        {
            if(data == null) return;
            name = data.name;
            cost = data.cost;
            type = data.type;
            rarity = data.rarity;
            dreamPieceNum = data.dreamPieceNum;
            isVolatile = data.isVolatile;
            isVanish = data.isVanish;
            isRemain = data.isRemain;
            isSingleTarget = data.isSingleTarget;
            sprite = data.sprite;
            text = data.text;
            cardValues = new List<int>(data.cardValues);
            cardValueTypes = new List<ECardValueType>(data.cardValueTypes);
            name_enhanced = data.name_enhanced;
            cost_enhanced = data.cost_enhanced;
            isVolatile_enhanced = data.isVolatile_enhanced;
            isVanish_enhanced = data.isVanish_enhanced;
            isRemain_enhanced = data.isRemain_enhanced;
            isSingleTarget_enhanced = data.isSingleTarget_enhanced;
            text_enhanced = data.text_enhanced;
            cardValues_enhanced = new List<int>(data.cardValues_enhanced);
            cardValueTypes_enhanced = new List<ECardValueType>(data.cardValueTypes_enhanced);
        }

        public Item_Data(Item_Enhanceable item_Enhanceable)
        {
            if(item_Enhanceable == null) return;
            name = item_Enhanceable.name;
            cost = item_Enhanceable.cost;
            type = item_Enhanceable.type;
            rarity = item_Enhanceable.rarity;
            dreamPieceNum = item_Enhanceable.dreamPieceNum;
            isVolatile = item_Enhanceable.isVolatile;
            isVanish = item_Enhanceable.isVanish;
            isRemain = item_Enhanceable.isRemain;
            isSingleTarget = item_Enhanceable.isSingleTarget;
            sprite = item_Enhanceable.sprite != null ? item_Enhanceable.sprite.name : "";
            text = item_Enhanceable.text;
            cardValues = new List<int>(item_Enhanceable.cardValues);
            cardValueTypes = new List<ECardValueType>(item_Enhanceable.cardValueTypes);

            if(item_Enhanceable.enhancedItem != null)
            {
                name_enhanced = item_Enhanceable.enhancedItem.name;
                cost_enhanced = item_Enhanceable.enhancedItem.cost;
                isVolatile_enhanced = item_Enhanceable.enhancedItem.isVolatile;
                isVanish_enhanced = item_Enhanceable.enhancedItem.isVanish;
                isRemain_enhanced = item_Enhanceable.enhancedItem.isRemain;
                isSingleTarget_enhanced = item_Enhanceable.enhancedItem.isSingleTarget;
                text_enhanced = item_Enhanceable.enhancedItem.text;
                cardValues_enhanced = new List<int>(item_Enhanceable.enhancedItem.cardValues);
                cardValueTypes_enhanced = new List<ECardValueType>(item_Enhanceable.enhancedItem.cardValueTypes);
            }
        }

        public Item_Data(Item item)
        {
            if(item == null) return;
            name = item.name;
            cost = item.cost;
            type = item.type;
            rarity = item.rarity;
            dreamPieceNum = item.dreamPieceNum;
            isVolatile = item.isVolatile;
            isVanish = item.isVanish;
            isRemain = item.isRemain;
            isSingleTarget = item.isSingleTarget;
            sprite = item.sprite != null ? item.sprite.name : "";
            text = item.text;
            cardValues = new List<int>(item.cardValues);
            cardValueTypes = new List<ECardValueType>(item.cardValueTypes);
        }
    }

    [System.Serializable]
    public class Item_Num
    {
        public string cardName;
        public int num;

        public Item_Num(Item_Num data)
        {
            cardName = data.cardName;
            num = data.num;
        }

        public Item_Num(string cardName, int num)
        {
            this.cardName = cardName;
            this.num = num;
        }
    }

    [CreateAssetMenu(fileName = "ItemDataSO", menuName = "Scriptable Objects/ItemDataSO")]
    public class ItemDataSO : ScriptableObject
    {
        public List<Item_Data> items;
    }
