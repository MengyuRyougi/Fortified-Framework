using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Fortified
{
    /// <summary>
    /// 與原版 HediffGiver_Random 相同的 MTB 隨機給予，但不寄信。
    /// 若 levelUpIfExists 為 true 且目標部位已有此 hediff（Hediff_Level 系列），
    /// 則改為升一級而不是略過；因此裝越多帶有此 giver 的植入物，病情惡化越快。
    /// 只要有任何 hediff 的 stage 對此病 makeImmuneTo（例如解藥的 High），升級會暫停。
    /// </summary>
    public class HediffGiver_RandomSilent : HediffGiver
    {
        public float mtbDays;

        public bool levelUpIfExists = false;

        /// <summary>升級時對殖民者發出的訊息 key，{PAWN} 為角色、{0} 為含分級的病症名稱；留空則不發訊息。</summary>
        public string levelUpMessageKey;

        private static List<Hediff> tmpHediffs = new List<Hediff>();

        public override void OnIntervalPassed(Pawn pawn, Hediff cause)
        {
            float num = mtbDays;
            float num2 = ChanceFactor(pawn);
            if (num2 == 0f || !Rand.MTBEventOccurs(num / num2, 60000f, 60f))
            {
                return;
            }
            if (levelUpIfExists && TryLevelUp(pawn))
            {
                return;
            }
            TryApply(pawn);
        }

        /// <summary>目標部位已有此 hediff 時升一級。回傳 true 代表已處理（無論是否真的升級），不再走 TryApply。</summary>
        private bool TryLevelUp(Pawn pawn)
        {
            tmpHediffs.Clear();
            pawn.health.hediffSet.GetHediffs(ref tmpHediffs, h => h.def == hediff && h is Hediff_Level && (partsToAffect == null || partsToAffect.Contains(h.Part?.def)));
            if (tmpHediffs.Count == 0)
            {
                return false;
            }
            Hediff_Level level = (Hediff_Level)tmpHediffs.RandomElement();
            tmpHediffs.Clear();
            // ImmunityHandler.GetImmunity 對 makeImmuneTo 的來源會回傳固定的 0.65。
            if (pawn.health.immunity.GetImmunity(hediff) > 0f || level.level >= hediff.maxSeverity)
            {
                return true;
            }
            level.ChangeLevel(1);
            // 立即同步 Severity，讓 stage 與致死判定在同一個 tick 生效。
            level.Severity = level.level;
            if (!levelUpMessageKey.NullOrEmpty() && PawnUtility.ShouldSendNotificationAbout(pawn))
            {
                Messages.Message(levelUpMessageKey.Translate(pawn.Named("PAWN"), level.LabelCap), pawn, MessageTypeDefOf.NegativeHealthEvent);
            }
            return true;
        }
    }
}
