using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Fortified
{
    /// <summary>
    /// 掛在 <see cref="HediffDef"/> 上，讓帶有該 Hediff 的 Pawn 不被 <see cref="CompAlertScanner"/>（監視器、震動傳感器、哨塔）偵測。
    /// 用途：隱身、潛行、電子對抗等特殊技能的 Hediff。<br/>
    /// <c>detectionChance</c>：每次掃描仍被發現的機率（0 = 完全豁免，預設；1 = 沒有效果）。
    /// 一個 Pawn 同時帶多個豁免 Hediff 時取最低值。<br/>
    /// 另外，任何讓 <see cref="PawnUtility.IsPsychologicallyInvisible"/> 為 true 的 Hediff
    /// （如 <c>PsychicInvisibility</c>、<c>FFF_Camouflage</c>）不需要掛這個 Extension 也會自動豁免。
    /// <para>
    /// Placed on a <see cref="HediffDef"/>: a pawn carrying that hediff is skipped by <see cref="CompAlertScanner"/>
    /// (cameras, seismic sensors, sentry towers). Meant for stealth / invisibility / ECM style ability hediffs.
    /// <c>detectionChance</c> is the per-scan chance the pawn is still spotted (0 = fully immune, default;
    /// 1 = no effect). With several immune hediffs on one pawn the lowest value wins.
    /// Hediffs that make <see cref="PawnUtility.IsPsychologicallyInvisible"/> true (e.g. <c>PsychicInvisibility</c>,
    /// <c>FFF_Camouflage</c>) are exempt automatically and do not need this extension.
    /// </para>
    /// </summary>
    public class ModExtension_AlertScannerImmunity : DefModExtension
    {
        /// <summary>每次掃描仍被偵測到的機率（0~1）。0 = 完全豁免。</summary>
        public float detectionChance = 0f;

        public override IEnumerable<string> ConfigErrors()
        {
            if (detectionChance < 0f || detectionChance > 1f)
                yield return $"{nameof(ModExtension_AlertScannerImmunity)}: detectionChance must be within 0~1, got {detectionChance}.";
        }

        /// <summary>
        /// 計算 Pawn 對警戒掃描器的「被偵測機率」。
        /// 沒有任何豁免來源時回傳 1；心理隱身時回傳 0；否則取所有豁免 Hediff 的最低 detectionChance。
        /// Returns the pawn's chance of being detected by an alert scanner: 1 with no exemption source,
        /// 0 while psychologically invisible, otherwise the lowest detectionChance among its immune hediffs.
        /// </summary>
        public static float GetDetectionChance(Pawn pawn, bool ignoreInvisibility = false)
        {
            if (!ignoreInvisibility && pawn.IsPsychologicallyInvisible()) return 0f;

            float chance = 1f;
            List<Hediff> hediffs = pawn.health?.hediffSet?.hediffs;
            if (hediffs == null) return chance;
            for (int i = 0; i < hediffs.Count; i++)
            {
                ModExtension_AlertScannerImmunity ext = hediffs[i].def.GetModExtension<ModExtension_AlertScannerImmunity>();
                if (ext == null) continue;
                chance = Mathf.Min(chance, ext.detectionChance);
                if (chance <= 0f) break;
            }
            return chance;
        }
    }
}
