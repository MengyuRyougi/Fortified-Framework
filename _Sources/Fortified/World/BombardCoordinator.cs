using System.Collections.Generic;
using Verse;

namespace Fortified
{
    // 多据点炮击协同器
    // 追踪地图已声明目标类型
    public static class BombardCoordinator
    {
        #region 字段
        // 缓存目标类型及其到期时间
        private static readonly Dictionary<Map, List<(BombardTargetType type, int expiry)>> claimed
            = new();
        #endregion

        #region 查询与注册
        public static bool IsClaimed(Map map, BombardTargetType type)
        {
            Purge(map);
            return claimed.TryGetValue(map, out var list)
                && list.Exists(x => x.type == type);
        }

        public static void Claim(Map map, BombardTargetType type, int holdTicks)
        {
            if (!claimed.TryGetValue(map, out var list))
                claimed[map] = list = new List<(BombardTargetType, int)>();
            // 同类型已存在则刷新到期时间
            list.RemoveAll(x => x.type == type);
            list.Add((type, Find.TickManager.TicksGame + holdTicks));
        }
        #endregion

        #region 清理
        private static void Purge(Map map)
        {
            if (!claimed.TryGetValue(map, out var list)) return;
            int now = Find.TickManager.TicksGame;
            list.RemoveAll(x => x.expiry <= now);
        }

        public static void ClearMap(Map map) => claimed.Remove(map);
        #endregion
    }
}
