using RimWorld;
using Verse;

namespace Fortified
{
    /// <summary>
    /// Signal 是全域廣播，SignalManager 不分地圖；地下口袋地圖的掃描器一叫，地表的效果器也會聽到。
    /// 收訊端用這裡判斷訊號是不是來自自己所在的地圖：先看 MAP 參數，沒有就看 SUBJECT 所在的地圖，
    /// 兩者都沒帶（例如任務訊號）就視為全域、照常接受。
    ///
    /// Signals are global — SignalManager has no notion of maps, so a scanner in an underground pocket
    /// map is heard by effectors on the surface. Receivers use this to check whether a signal came from
    /// their own map: the MAP arg first, then the SUBJECT thing's map; a signal that carries neither
    /// (quest signals etc.) is treated as global and accepted.
    /// </summary>
    public static class SignalMapUtility
    {
        public static bool IsFromMap(this Signal signal, Map map)
        {
            if (map == null) return true;

            if (signal.args.TryGetArg("MAP", out Map signalMap) && signalMap != null)
            {
                return signalMap == map;
            }
            if (signal.args.TryGetArg("SUBJECT", out Thing subject) && subject != null && subject.MapHeld != null)
            {
                return subject.MapHeld == map;
            }
            return true;
        }

        /// <summary>收訊端常用寫法：parent 未生成時不擋，否則要求同地圖。Convenience for comps: unspawned parents pass, spawned ones must match.</summary>
        public static bool IsForParent(this Signal signal, Thing parent)
        {
            return parent == null || !parent.SpawnedOrAnyParentSpawned || signal.IsFromMap(parent.MapHeld);
        }
    }
}
