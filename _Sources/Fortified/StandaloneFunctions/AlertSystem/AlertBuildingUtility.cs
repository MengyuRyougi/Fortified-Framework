using RimWorld;
using Verse;

namespace Fortified
{
    /// <summary>
    /// 警戒系統建築（掃描器、反制效果器）共用的「是否運作中」判定。<br/>
    /// 以下任一成立即視為離線：<br/>
    ///   • 有 <see cref="CompPowerTrader"/> 且沒電（含被 Flick 關掉、故障）<br/>
    ///   • 有 <see cref="CompStunnable"/> 且被暈眩（EMP / Stun 傷害）<br/>
    ///   • 有 <see cref="CompCanBeDormant"/> 且仍在休眠<br/>
    /// 沒有這些 Comp 的建築（例如純裝飾的坑洞）一律視為運作中。
    /// <para>
    /// Shared "is this thing online" check for alert-system buildings (scanners and counter-measure effectors).
    /// Offline when any applies: a <see cref="CompPowerTrader"/> without power (incl. flicked off / broken down),
    /// a <see cref="CompStunnable"/> currently stunned (EMP / Stun damage), or a <see cref="CompCanBeDormant"/>
    /// still dormant. Buildings with none of these comps (e.g. decorative holes) always count as operational.
    /// </para>
    /// </summary>
    public static class AlertBuildingUtility
    {
        public static bool IsOperational(ThingWithComps thing)
        {
            if (thing == null) return false;
            return IsOperational(
                thing.GetComp<CompPowerTrader>(),
                thing.GetComp<CompStunnable>(),
                thing.GetComp<CompCanBeDormant>());
        }

        /// <summary>快取版：呼叫端自行持有 Comp 引用，避免每次 GetComp。Cached variant for callers holding comp refs.</summary>
        public static bool IsOperational(CompPowerTrader power, CompStunnable stunnable, CompCanBeDormant dormant)
        {
            if (power != null && !power.PowerOn) return false;
            if (stunnable != null && stunnable.StunHandler != null && stunnable.StunHandler.Stunned) return false;
            if (dormant != null && !dormant.Awake) return false;
            return true;
        }
    }
}
