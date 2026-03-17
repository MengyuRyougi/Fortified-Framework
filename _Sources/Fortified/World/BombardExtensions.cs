using System.Collections.Generic;
using Verse;

namespace Fortified
{
    // 轰炸目标优先类型
    public enum BombardTargetType
    {
        Auto,              // 自动追随敌军聚集区
        TurretLine,        // 炮塔防线
        PowerGrid,         // 发电机和蓄电池
        HighValueStorage,  // 高价值储仓区域
        Colonists,         // 殖民者本人
        MechConcentration, // 玩家殖民地机械体聚集区
    }

    // 弹药选择模式
    public enum ShellSelectionMode
    {
        // 按权重概率随机选择弹药
        // 弹药权重分配规则
        Weighted,
        // 按固定顺序循环发射弹药
        // 顺序循环逻辑描述
        Sequential,
    }

    // 弹药权重条目定义
    public class WeightedShellEntry
    {
        public ThingDef projectileDef;
        // 记录弹药模式对应配置值
        public float weight = 1f;
    }

    // 空袭定义权重条目
    public class WeightedAirSupportEntry
    {
        public AirSupportDef airSupportDef;
        public float weight = 1f;
    }
}
