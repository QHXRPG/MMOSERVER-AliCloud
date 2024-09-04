using Proto;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Security.Policy;

namespace GameServer.InventorySystem;

// 物品类型
public enum ItemType
{
    Material,       // 材料&道具
    Consumable,     // 消耗品
    Equipment,      // 武器&装备
}

// 物品品质
public enum Quality
{
    Common,     // 普通
    Uncommon,   // 非凡
    Rare,       // 稀有
    Epic,       // 史诗
    Legendary,  // 传说
    Artifact,   // 神器
}

/// <summary>
/// 物品基类
/// </summary>
[Serializable]
public class Item
{
    public int Id { get; set; }                  // 物品ID
    public string Name { get; set; }             // 物品名称
    public ItemType ItemType { get; set; }       // 物品种类
    public Quality Quality { get; set; }         // 物品品质
    public string Description { get; set; }      // 物品描述
    public int Capicity { get; set; }            // 物品叠加数量上限
    public int BuyPrice { get; set; }            // 物品买入价格
    public int SellPrice { get; set; }           // 物品卖出价格
    public string Sprite { get; set; }           // 存放物品的图片路径，通过Resources加载
    public ItemDefine Def { get; private set; }  // 物品定义，私有设置

    public int amount;          // 物品当前数量
    public int position;        // 物品在背包或其他容器中的位置

    private ItemInfo _itemInfo; // 缓存的物品信息

    /// <summary>
    /// 获取物品信息
    /// </summary>
    public ItemInfo ItemInfo
    {
        get
        {
            // 如果缓存为空，创建新的物品信息
            if (_itemInfo == null)
            {
                _itemInfo = new ItemInfo() { ItemId = Id };
            }
            _itemInfo.Amount = amount;      // 更新数量
            _itemInfo.Position = position;  // 更新位置
            return _itemInfo;               // 返回物品信息
        }
    }

    /// <summary>
    /// 使用物品ID、数量和位置创建物品
    /// </summary>
    public Item(int itemId, int amount = 1, int position = 0)
        : this(DataManager.Instance.Items[itemId], amount, position)
    {
    }

    /// <summary>
    /// 使用物品信息创建物品
    /// </summary>
    public Item(ItemInfo itemInfo) : this(DataManager.Instance.Items[itemInfo.ItemId])
    {
        this.amount = itemInfo.Amount;      // 设置数量
        this.position = itemInfo.Position;  // 设置位置
    }

    /// <summary>
    /// 使用物品定义、数量和位置创建物品
    /// </summary>
    public Item(ItemDefine _def, int amount = 1, int position = 0) :
        this(_def.ID, _def.Name, ItemType.Material, Quality.Common,
            _def.Description, _def.Capicity, _def.BuyPrice, _def.SellPrice, _def.Icon)
    {
        Def = _def;                         // 设置物品定义
        this.amount = amount;               // 设置数量
        this.position = position;           // 设置位置

        // 根据物品定义的类型设置物品类型
        switch (Def.ItemType)
        {
            case "消耗品": this.ItemType = ItemType.Consumable; break;
            case "道具": this.ItemType = ItemType.Material; break;
            case "装备": this.ItemType = ItemType.Equipment; break;
        }

        // 根据物品定义的品质设置物品品质
        switch (Def.Quality)
        {
            // 根据物品定义的品质设置物品品质
            case "普通": this.Quality = Quality.Common; break;
            case "非凡": this.Quality = Quality.Uncommon; break;
            case "稀有": this.Quality = Quality.Rare; break;
            case "史诗": this.Quality = Quality.Epic; break;
            case "传说": this.Quality = Quality.Legendary; break;
            case "神器": this.Quality = Quality.Artifact; break;
        }
    }

    /// <summary>
    /// 使用各种属性创建物品
    /// </summary>
    public Item(int id, string name, ItemType itemType, Quality quality, string description, int capicity, int buyPrice, int sellPrice, string sprite)
    {
        Id = id;                           // 设置物品ID
        Name = name;                       // 设置物品名称
        ItemType = itemType;               // 设置物品类型
        Quality = quality;                 // 设置物品品质
        Description = description;         // 设置物品描述
        Capicity = capicity;               // 设置物品叠加数量上限
        BuyPrice = buyPrice;               // 设置物品买入价格
        SellPrice = sellPrice;             // 设置物品卖出价格
        Sprite = sprite;                   // 设置物品图片路径
    }
}
