using GameServer.Model;
using Google.Protobuf;
using Proto;
using Serilog;
using Summer;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.InventorySystem
{
    /// <summary>
    /// 库存对象
    /// </summary>
    public class Inventory
    {

        public Character Chr { get; private set; }

        public int Capacity { get; private set; }                           //背包容量
        private ConcurrentDictionary<int, Item> ItemMap { get; } = new();   //物品字典 <插槽索引，物品对象>
        private bool hasChanged;                                            //字典是否发生变化

        public Inventory(Character _chr)
        {
            Chr = _chr;
        }

        // 在数据库中拿到背包信息的比特数据来初始化用户背包
        public void Init(byte[] bytes)
        {
            //默认背包
            if (bytes == null)
            {
                Capacity = 10;
            }
            //数据还原
            else
            {
                InventoryInfo inv = InventoryInfo.Parser.ParseFrom(bytes);
                Log.Information("数据还原：" + inv);
                Capacity = inv.Capacity;
                //创建物品
                foreach (var itemInfo in inv.List)
                {
                    SetItem(itemInfo.Position, new Item(itemInfo));
                }
                //给6号插槽加入物品
                //SetItem(6, new Item(1002,998));
            }
        }

        /// <summary>
        /// 设置这个插槽所承载的物品类型，插槽索引从0开始
        /// </summary>
        /// <param name="slotIndex">插槽索引</param>
        /// <param name="item"></param>
        public bool SetItem(int slotIndex, Item item)
        {
            if (slotIndex >= Capacity) return false;  //如果索引大于容量则停止设置
            hasChanged = true;                        //标记数据发生变化

            //清空当前插槽
            if (item is null)
            {
                ItemMap.TryRemove(slotIndex, out var _value);
                if (_value != null) _value.position = -1;
                return true;
            }

            //设置插槽物品
            ItemMap[slotIndex] = item;
            item.position = slotIndex;
            return true;
        }

        //获取插槽物品
        public bool TrySlotItem(int slotIndex, out Item item)
        {
            return ItemMap.TryGetValue(slotIndex, out item);
        }

        private InventoryInfo _inventoryInfo;

        // 将背包信息打包成 protobuf 
        public InventoryInfo InventoryInfo
        {
            get
            {
                if (_inventoryInfo == null)
                {
                    _inventoryInfo = new InventoryInfo();
                }
                _inventoryInfo.Capacity = Capacity;
                //如果数据有变化
                if (hasChanged)
                {
                    //重新拼装proto对象
                    _inventoryInfo.List.Clear();
                    foreach (var item in ItemMap.Values)
                    {
                        _inventoryInfo.List.Add(item.ItemInfo);
                    }
                }
                return _inventoryInfo;
            }
        }



        /// <summary>
        /// 物品加入库存
        /// </summary>
        /// <param name="itemId"> 物品id </param>  
        /// <param name="amount"> 加入该物品的数量 </param>
        /// <returns></returns>
        public bool AddItem(int itemId, int amount = 1)
        {
            // 验证物品类型是否存在
            if (!DataManager.Instance.Items.TryGetValue(itemId, out var def))
            {
                Log.Information("物品id不存在:{0}", itemId);
                return false;
            }

            // 检查剩余空间
            if (CalculateMaxRemainingQuantity(itemId) < amount) return false;

            // amount代表期望添加的数量，大于0则一直循环
            while (amount > 0)
            {
                var sameItem = FindSameItemAndNotFull(itemId);
                if (sameItem != null)  //查找id相同且未满的物品
                {
                    //本次可以处理的数量
                    var current = Math.Min(amount, sameItem.Capicity - sameItem.amount);
                    sameItem.amount += current;
                    amount -= current;
                }
                else  // 另外开辟一个插槽来存这类的物品
                {
                    //查找背包空闲的插槽索引，-1代表背包已满
                    var index = FindEmptyIndex();
                    if (index > -1)
                    {
                        //本次可处理的数量
                        var current = Math.Min(amount, def.Capicity);
                        SetItem(index, new Item(def, current, index));
                        amount -= current;
                    }
                    else
                    {
                        Log.Debug("没有空的物品槽");
                        return false;
                    }
                }
            }
            return true;
        }


        /// <summary>
        /// 交换物品位置，该索引是插槽索引
        /// </summary>
        /// <param name="index"></param>
        /// <param name="targetIndex"></param>
        /// <returns></returns>
        public bool Exchange(int originSlotIndex, int targetSlotIndex)
        {
            //交换id不能相同
            if (originSlotIndex == targetSlotIndex) return false;

            //交换id不能小于零
            if (originSlotIndex < 0 || targetSlotIndex < 0) return false;

            //查找原始插槽物品
            if (!ItemMap.TryGetValue(originSlotIndex, out var item1)) return false;
            Log.Information("item1={0}", item1.Name);

            //查找目标插槽物品，如果为空，直接放置
            if (!ItemMap.TryGetValue(targetSlotIndex, out var item2))
            {
                SetItem(originSlotIndex, null);
                SetItem(targetSlotIndex, item1);
            }
            else
            {
                //如果物品类型相同
                if (item1.Id == item2.Id)
                {
                    //可移动的数量
                    int num = Math.Min(item2.Capicity - item2.amount, item1.amount);
                    // 如果原始物品数量小于等于可移动数量，将原始物品全部移动到目标插槽
                    if (item1.amount <= num)
                    {
                        item2.amount += item1.amount;
                        SetItem(originSlotIndex, null);
                    }
                    else
                    {
                        // 否则，不移动物品只修改数量
                        item2.amount += num;
                        item1.amount -= num;
                    }
                }
                //如果类型不同则交换位置
                else
                {
                    SetItem(targetSlotIndex, item1);
                    SetItem(originSlotIndex, item2);
                }
            }

            return true;
        }


        /// <summary>
        /// 移除指定数量的物品，无视插槽位置
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public int RemoveItem(int itemId, int amount = 1)
        {
            int removedAmount = 0;

            while (amount > 0)
            {
                var item = FindSameItem(itemId);

                if (item == null)
                {
                    break;
                }
                // 判断要移除的数量是否大于物品的当前数量
                int currentAmount = Math.Min(amount, item.amount);
                item.amount -= currentAmount;
                removedAmount += currentAmount;
                amount -= currentAmount;
                //清空物品槽
                if (item.amount == 0)
                {
                    SetItem(item.position, null);
                }
            }

            return removedAmount;
        }


        /**
         * 丢弃指定插槽的物品，返回实际丢弃的数量
         */
        public int Discard(int slotIndex, int amount = 1)
        {
            if (amount < 1) return 0;
            if (!ItemMap.TryGetValue(slotIndex, out var item)) return 0;

            //只丢弃一部分
            if (amount < item.amount)
            {
                item.amount -= amount;
                var newItem = new Item(item.Id, amount);
                ItemEntity.Create(Chr.Space, newItem, Chr.Position, Vector3Int.zero);
                return amount;
            }

            //全额丢弃
            SetItem(slotIndex, null);
            ItemEntity.Create(Chr.Space, item, Chr.Position, Vector3Int.zero);
            return item.amount;
        }


        /* 计算背包里还能放多少个这样的物品 */
        private int CalculateMaxRemainingQuantity(int itemId)
        {
            //检查物品类型是否存在
            if (!DataManager.Instance.Items.TryGetValue(itemId, out var def))
            {
                Log.Information("物品id不存在:{0}", itemId);
                return 0;
            }
            var quantity = 0;  //记录还能放入多少个这样的物品

            //遍历全部插槽
            for (var i = 0; i < Capacity; i++)
            {
                //如果插槽有物品
                if (ItemMap.TryGetValue(i, out var item))
                {
                    //如果物品类型相同
                    if (item.Id == itemId)
                    {
                        quantity += (item.Capicity - item.amount);
                    }
                }
                else
                {
                    quantity += def.Capicity;
                }
            }
            Log.Debug("Inventory：Entity[{0}] 物品[{1}]还能放入[{2}]个", Chr.entityId, def.Name, quantity);
            return quantity;
        }

        //查找ID相同的物品
        private Item FindSameItem(int itemId)
        {
            return ItemMap.Values.FirstOrDefault(item => item?.Id == itemId);
        }

        //查找ID相同且未满的物品
        private Item FindSameItemAndNotFull(int itemId)
        {
            return ItemMap.Values.FirstOrDefault(item =>
            item != null && item.Id == itemId && item.amount < item.Capicity);
        }

        //查找空的插槽位置
        private int FindEmptyIndex()
        {
            for (int i = 0; i < Capacity; i++)
            {
                if (!ItemMap.ContainsKey(i)) return i;
            }
            return -1;
        }


    }
}
