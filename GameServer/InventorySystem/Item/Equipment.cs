using System.Collections;
using System.Collections.Generic;

namespace GameServer.InventorySystem;

/// <summary>
/// 装备
/// </summary>
public class Equipment : Item
{
    public Equipment(int id, string name, ItemType itemType, Quality quality, string description, int capicity, int buyPrice, int sellPrice, string sprite) : base(id, name, itemType, quality, description, capicity, buyPrice, sellPrice, sprite)
    {
    }
}
