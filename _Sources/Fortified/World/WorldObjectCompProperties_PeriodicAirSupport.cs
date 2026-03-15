using RimWorld;
using RimWorld.Planet;
using Verse;
using System.Collections.Generic;

namespace Fortified
{
    public enum StrikePattern
    {
        Random,      // 随机散布
        Circle,      // 圆形均匀散布
        Fan,         // 扇形散布
        Creeping     // 向前徐进轰炸
    }

    public class WorldObjectCompProperties_PeriodicAirSupport : WorldObjectCompProperties
    {
        public AirSupportDef airSupportDef;
        // 定义多空袭模式权重池
        // 启用权重池时忽略默认定义
        public List<WeightedAirSupportEntry> airSupportPool;
        public ThingDef vanillaBombardmentDef;
        public StrikePattern strikePattern = StrikePattern.Random; // 轰炸模式
        public float spreadRadius = 4f; // 散布半径
        public float fanAngle = 45f; // 扇形弧度
        public float creepingStep = 5f; // 徐进模式下的每段步进距离
        public int wavesPerStrike = 1; // 每次执行打击时的子波次数
        public int waveIntervalTicks = 45; // 波次之间的发射延迟
        public int projectileIntervalTicks = 10; // 炮弹发射间隔时间
        public float projectileOriginHeight = 120f; // 炮弹生成的模拟高度
        public float targetSearchRange = 65f; // 搜索玩家防御设施的探测半径
        public IntRange firstStrikeDelayTicks = new IntRange(150, 150); // 首枚炮弹落下的基础延迟

        public IntRange ticksBetweenStrikes = new IntRange(2500, 3000); // 首次检测到威胁后的随机延迟
        public int strikeCycles = 1; // 轰炸轮次数
        public int strikeIntervalTicks = 2700; // 每轮轰炸之间的间隔
        public int projectilesPerStrike = 12; // 每次轰炸发射的炮弹数量
        public bool requireActiveThreat = true; // 是否只在地图受到袭击时触发
        public string messageText; // 开火时左上角的提示
        public MessageTypeDef messageType;

        public float successChance = 1.0f; // 每轮打击结果概率
        public float triggerChance = 1.0f; // 袭击触发服务概率
        public string letterLabel; // 开火时的信件标题
        public string letterText; // 开火时的信件内容

        public List<SitePartDef> requiresAnySitePart;

        // 定义袭击可用弹药池
        public List<WeightedShellEntry> shellPool;

        // 弹药选择模式
        // 权重模式弹药随机逻辑
        // 顺序模式弹药循环逻辑
        public ShellSelectionMode shellSelectionMode = ShellSelectionMode.Weighted;

        // 设置目标选择优先级列表
        // 优先级配置示例
        // 跳过已被占用的优先目标
        // 默认自动选择敌军聚集区
        public List<BombardTargetType> targetPriorities;

        // 设置协同目标锁定时长
        // 描述锁定自动过期机制
        public int coordinationHoldTicks = 600;

        public WorldObjectCompProperties_PeriodicAirSupport()
        {
            this.compClass = typeof(WorldObjectComp_PeriodicAirSupport);
        }
    }
}
