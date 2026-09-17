using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Fortified
{
    /// <summary>
    /// 訊息卡「特殊機制」條目的唯一入口。
    /// 流程：列舉 owner Def 的來源物件 → 介面式 (IInfoProvider) 與宣告式 (autoAttachTo) 比對 → 去重 → 輸出 StatDrawEntry。
    /// </summary>
    public static class InfoDisplayUtility
    {
        public struct CollectedEntry
        {
            public InfoEntry entry;
            public object source;

            public CollectedEntry(InfoEntry entry, object source)
            {
                this.entry = entry;
                this.source = source;
            }
        }

        private struct AutoAttachRecord
        {
            public Type type;
            public FFF_InfoDef def;
            public bool inherit;
        }

        // 宣告式自動附加的原始紀錄（XML autoAttachTo + RegisterAutoAttach）
        private static readonly List<AutoAttachRecord> autoAttachRecords = new List<AutoAttachRecord>();

        // 以來源型別為 key 的查詢快取
        private static readonly Dictionary<Type, List<FFF_InfoDef>> autoAttachCache = new Dictionary<Type, List<FFF_InfoDef>>();

        // 下游可註冊「如何從某種 Def 取出來源物件」
        private static readonly Dictionary<Type, Func<Def, IEnumerable<object>>> sourceEnumerators = new Dictionary<Type, Func<Def, IEnumerable<object>>>();

        private static bool autoAttachInitialized;

        private static readonly List<FFF_InfoDef> emptyDefs = new List<FFF_InfoDef>();

        #region Public API

        // 主要入口：給 Harmony Postfix 用
        public static IEnumerable<StatDrawEntry> BuildEntries(Def owner, StatRequest req)
        {
            return BuildEntries(owner, req, null, false, null);
        }

        // 進階入口：自帶去重集合供多個 Def 共用，pawnCardOnly 過濾 InfoEntry.showOnPawnCard，
        // sourceLabel 非空時標題加上來源後綴（Pawn 卡彙總用）
        public static IEnumerable<StatDrawEntry> BuildEntries(Def owner, StatRequest req, HashSet<FFF_InfoDef> seen, bool pawnCardOnly = false, string sourceLabel = null)
        {
            if (owner == null)
                return Enumerable.Empty<StatDrawEntry>();
            return BuildEntriesFromSources(owner, req, EnumerateSources(owner, req), seen, pawnCardOnly, sourceLabel);
        }

        // 自訂來源清單：Hediff 實例 comps、Pawn 身上物件等非 Def 層級的來源
        public static IEnumerable<StatDrawEntry> BuildEntriesFromSources(Def owner, StatRequest req, IEnumerable<object> sources, HashSet<FFF_InfoDef> seen, bool pawnCardOnly = false, string sourceLabel = null)
        {
            if (owner == null || sources == null)
                yield break;

            List<CollectedEntry> collected = CollectEntriesFromSources(owner, req, sources, seen, pawnCardOnly);
            if (collected.Count == 0)
                yield break;

            int index = 0;
            for (int i = 0; i < collected.Count; i++)
            {
                StatDrawEntry drawEntry = MakeDrawEntry(owner, req, collected[i], sourceLabel, ref index);
                if (drawEntry != null)
                    yield return drawEntry;
            }
        }

        // 只收集不輸出：供其他 UI 重用
        public static List<CollectedEntry> CollectEntries(Def owner, StatRequest req)
        {
            return CollectEntries(owner, req, null, false);
        }

        public static List<CollectedEntry> CollectEntries(Def owner, StatRequest req, HashSet<FFF_InfoDef> seen, bool pawnCardOnly)
        {
            if (owner == null)
                return new List<CollectedEntry>();
            return CollectEntriesFromSources(owner, req, EnumerateSources(owner, req), seen, pawnCardOnly);
        }

        public static List<CollectedEntry> CollectEntriesFromSources(Def owner, StatRequest req, IEnumerable<object> sources, HashSet<FFF_InfoDef> seen, bool pawnCardOnly)
        {
            List<CollectedEntry> result = new List<CollectedEntry>();
            if (owner == null || sources == null)
                return result;

            EnsureAutoAttachInitialized();
            HashSet<FFF_InfoDef> localSeen = seen ?? new HashSet<FFF_InfoDef>();
            InfoDisplayExtension manual = owner.GetModExtension<InfoDisplayExtension>();

            foreach (object source in sources)
            {
                if (source == null)
                    continue;

                InfoContext ctx = new InfoContext(owner, req, source);

                // (a) 介面式
                if (source is IInfoProvider provider)
                {
                    IEnumerable<InfoEntry> entries = null;
                    try
                    {
                        entries = provider.GetInfoEntries(ctx);
                    }
                    catch (Exception e)
                    {
                        Log.ErrorOnce($"[FFF] IInfoProvider {provider.GetType()} threw on {owner.defName}: {e}", provider.GetType().GetHashCode() ^ owner.GetHashCode());
                    }
                    if (entries != null)
                    {
                        foreach (InfoEntry entry in entries)
                            TryAdd(result, localSeen, manual, entry, source, pawnCardOnly);
                    }
                }

                // (b) 宣告式
                Type sourceType = source as Type ?? source.GetType();
                List<FFF_InfoDef> autoDefs = LookupAutoAttach(sourceType);
                for (int i = 0; i < autoDefs.Count; i++)
                    TryAdd(result, localSeen, manual, new InfoEntry(autoDefs[i]), source, pawnCardOnly);
            }

            return result;
        }

        // Pawn 卡彙總：Hediff / 服裝 / 武器 / 基因所帶的機制（InfoEntry.showOnPawnCard），標題加來源後綴。
        // race ThingDef 自身的條目由 Def Postfix 輸出，這裡先標記為已見。
        public static IEnumerable<StatDrawEntry> BuildPawnAggregateEntries(Pawn pawn)
        {
            if (pawn == null)
                yield break;

            StatRequest req = StatRequest.For(pawn);
            HashSet<FFF_InfoDef> seen = new HashSet<FFF_InfoDef>();
            MarkSeen(pawn.def, req, seen);

            if (pawn.health?.hediffSet != null)
            {
                List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
                for (int i = 0; i < hediffs.Count; i++)
                {
                    Hediff h = hediffs[i];
                    if (h?.def == null || !h.Visible)
                        continue;
                    IEnumerable<object> sources = Concat(EnumerateSources(h.def, req), HediffInstanceSources(h));
                    foreach (StatDrawEntry entry in BuildEntriesFromSources(h.def, req, sources, seen, true, h.LabelBaseCap))
                        yield return entry;
                }
            }

            if (pawn.apparel != null)
            {
                List<Apparel> worn = pawn.apparel.WornApparel;
                for (int i = 0; i < worn.Count; i++)
                {
                    Apparel a = worn[i];
                    if (a?.def == null)
                        continue;
                    IEnumerable<object> sources = Concat(EnumerateSources(a.def, req), ThingInstanceSources(a));
                    foreach (StatDrawEntry entry in BuildEntriesFromSources(a.def, req, sources, seen, true, a.def.LabelCap))
                        yield return entry;
                }
            }

            if (pawn.equipment != null)
            {
                List<ThingWithComps> equipment = pawn.equipment.AllEquipmentListForReading;
                for (int i = 0; i < equipment.Count; i++)
                {
                    ThingWithComps eq = equipment[i];
                    if (eq?.def == null)
                        continue;
                    IEnumerable<object> sources = Concat(EnumerateSources(eq.def, req), ThingInstanceSources(eq));
                    foreach (StatDrawEntry entry in BuildEntriesFromSources(eq.def, req, sources, seen, true, eq.def.LabelCap))
                        yield return entry;
                }
            }

            if (ModsConfig.BiotechActive && pawn.genes != null)
            {
                List<Gene> genes = pawn.genes.GenesListForReading;
                for (int i = 0; i < genes.Count; i++)
                {
                    Gene g = genes[i];
                    if (g?.def == null || !g.Active)
                        continue;
                    IEnumerable<object> sources = Concat(EnumerateSources(g.def, req), Gen.YieldSingle<object>(g));
                    foreach (StatDrawEntry entry in BuildEntriesFromSources(g.def, req, sources, seen, true, g.def.LabelCap))
                        yield return entry;
                }
            }
        }

        // 只把 owner 會輸出的 InfoDef 加進 seen，不產生條目；用於避免與另一條輸出路徑重複
        public static void MarkSeen(Def owner, StatRequest req, HashSet<FFF_InfoDef> seen)
        {
            if (owner == null || seen == null)
                return;
            List<CollectedEntry> collected = CollectEntries(owner, req, null, false);
            for (int i = 0; i < collected.Count; i++)
                seen.Add(collected[i].entry.def);
        }

        // C# 端註冊自動附加，等同 XML 的 autoAttachTo
        public static void RegisterAutoAttach(Type sourceType, FFF_InfoDef def, bool inherit = true)
        {
            if (sourceType == null || def == null)
                return;
            autoAttachRecords.Add(new AutoAttachRecord { type = sourceType, def = def, inherit = inherit });
            autoAttachCache.Clear();
        }

        // 註冊額外的來源列舉器；同一 Def 型別（含子類）可疊加多個
        public static void RegisterSourceEnumerator<TDef>(Func<TDef, IEnumerable<object>> enumerator) where TDef : Def
        {
            if (enumerator == null)
                return;
            Type key = typeof(TDef);
            Func<Def, IEnumerable<object>> wrapped = d => enumerator((TDef)d);
            if (sourceEnumerators.TryGetValue(key, out Func<Def, IEnumerable<object>> existing))
                sourceEnumerators[key] = d => Concat(existing(d), wrapped(d));
            else
                sourceEnumerators[key] = wrapped;
        }

        #endregion

        #region Sources

        // 依 owner 型別列舉可能攜帶機制的來源物件
        public static IEnumerable<object> EnumerateSources(Def owner, StatRequest req)
        {
            yield return owner;

            if (owner.modExtensions != null)
            {
                for (int i = 0; i < owner.modExtensions.Count; i++)
                    yield return owner.modExtensions[i];
            }

            switch (owner)
            {
                case ThingDef thingDef:
                    foreach (object s in ThingDefSources(thingDef)) yield return s;
                    break;
                case HediffDef hediffDef:
                    foreach (object s in HediffDefSources(hediffDef)) yield return s;
                    break;
                case AbilityDef abilityDef:
                    foreach (object s in AbilityDefSources(abilityDef)) yield return s;
                    break;
                case GeneDef geneDef:
                    if (geneDef.geneClass != null) yield return geneDef.geneClass;
                    break;
                case RecipeDef recipeDef:
                    if (recipeDef.workerClass != null) yield return recipeDef.workerClass;
                    break;
            }

            // Thing 實例：ThingComp 也可實作 IInfoProvider 讀取執行期狀態
            if (req.HasThing && req.Thing is ThingWithComps twc && twc.def == owner)
            {
                List<ThingComp> comps = twc.AllComps;
                for (int i = 0; i < comps.Count; i++)
                    yield return comps[i];
            }

            foreach (KeyValuePair<Type, Func<Def, IEnumerable<object>>> kv in sourceEnumerators)
            {
                if (!kv.Key.IsInstanceOfType(owner))
                    continue;
                IEnumerable<object> extra = null;
                try
                {
                    extra = kv.Value(owner);
                }
                catch (Exception e)
                {
                    Log.ErrorOnce($"[FFF] InfoDisplay source enumerator for {kv.Key} threw on {owner.defName}: {e}", kv.Key.GetHashCode() ^ owner.GetHashCode());
                }
                if (extra == null)
                    continue;
                foreach (object s in extra)
                    yield return s;
            }
        }

        // Hediff 實例層級的來源：Hediff 本身與其 HediffComp 實例（可讀執行期狀態）
        public static IEnumerable<object> HediffInstanceSources(Hediff hediff)
        {
            if (hediff == null)
                yield break;
            yield return hediff;
            if (hediff is HediffWithComps hwc && hwc.comps != null)
            {
                for (int i = 0; i < hwc.comps.Count; i++)
                    yield return hwc.comps[i];
            }
        }

        // Thing 實例層級的來源：Thing 本身與其 ThingComp 實例
        public static IEnumerable<object> ThingInstanceSources(Thing thing)
        {
            if (thing == null)
                yield break;
            yield return thing;
            if (thing is ThingWithComps twc)
            {
                List<ThingComp> comps = twc.AllComps;
                for (int i = 0; i < comps.Count; i++)
                    yield return comps[i];
            }
        }

        private static IEnumerable<object> ThingDefSources(ThingDef def)
        {
            if (def.thingClass != null) yield return def.thingClass;
            if (def.comps != null)
            {
                for (int i = 0; i < def.comps.Count; i++)
                {
                    yield return def.comps[i];
                    // compClass 也列為來源，讓 autoAttachTo 可直接指向 ThingComp 型別（例如共用 Props 的自訂 comp）
                    if (def.comps[i]?.compClass != null) yield return def.comps[i].compClass;
                }
            }
            if (def.Verbs != null)
            {
                for (int i = 0; i < def.Verbs.Count; i++)
                {
                    VerbProperties verb = def.Verbs[i];
                    yield return verb;
                    // 武器的預設投射物：投射物本身很少被開卡，其機制顯示在武器卡上
                    ThingDef proj = verb?.defaultProjectile;
                    if (proj == null) continue;
                    if (proj.thingClass != null) yield return proj.thingClass;
                    if (proj.modExtensions != null)
                    {
                        for (int j = 0; j < proj.modExtensions.Count; j++)
                            yield return proj.modExtensions[j];
                    }
                }
            }
            if (def.tools != null)
            {
                for (int i = 0; i < def.tools.Count; i++)
                    yield return def.tools[i];
            }
            if (def.race != null) yield return def.race;
            if (def.apparel != null) yield return def.apparel;
            if (def.building != null) yield return def.building;
        }

        private static IEnumerable<object> HediffDefSources(HediffDef def)
        {
            if (def.hediffClass != null) yield return def.hediffClass;
            if (def.comps != null)
            {
                for (int i = 0; i < def.comps.Count; i++)
                {
                    yield return def.comps[i];
                    if (def.comps[i]?.compClass != null) yield return def.comps[i].compClass;
                }
            }
            if (def.stages != null)
            {
                for (int i = 0; i < def.stages.Count; i++)
                    yield return def.stages[i];
            }
        }

        private static IEnumerable<object> AbilityDefSources(AbilityDef def)
        {
            if (def.abilityClass != null) yield return def.abilityClass;
            if (def.comps != null)
            {
                for (int i = 0; i < def.comps.Count; i++)
                {
                    yield return def.comps[i];
                    if (def.comps[i]?.compClass != null) yield return def.comps[i].compClass;
                }
            }
            if (def.verbProperties != null) yield return def.verbProperties;
        }

        private static IEnumerable<object> Concat(IEnumerable<object> a, IEnumerable<object> b)
        {
            if (a != null) foreach (object o in a) yield return o;
            if (b != null) foreach (object o in b) yield return o;
        }

        #endregion

        #region Auto attach

        private static void EnsureAutoAttachInitialized()
        {
            if (autoAttachInitialized)
                return;
            autoAttachInitialized = true;

            foreach (FFF_InfoDef def in DefDatabase<FFF_InfoDef>.AllDefsListForReading)
            {
                if (def.autoAttachTo.NullOrEmpty())
                    continue;
                for (int i = 0; i < def.autoAttachTo.Count; i++)
                {
                    Type t = def.autoAttachTo[i];
                    if (t == null)
                        continue;
                    autoAttachRecords.Add(new AutoAttachRecord { type = t, def = def, inherit = def.autoAttachInherit });
                }
            }
            autoAttachCache.Clear();
        }

        private static List<FFF_InfoDef> LookupAutoAttach(Type sourceType)
        {
            if (autoAttachRecords.Count == 0)
                return emptyDefs;

            if (autoAttachCache.TryGetValue(sourceType, out List<FFF_InfoDef> cached))
                return cached;

            List<FFF_InfoDef> list = null;
            for (int i = 0; i < autoAttachRecords.Count; i++)
            {
                AutoAttachRecord rec = autoAttachRecords[i];
                bool match = rec.inherit ? rec.type.IsAssignableFrom(sourceType) : rec.type == sourceType;
                if (!match)
                    continue;
                list ??= new List<FFF_InfoDef>();
                if (!list.Contains(rec.def))
                    list.Add(rec.def);
            }

            list ??= emptyDefs;
            autoAttachCache[sourceType] = list;
            return list;
        }

        #endregion

        #region Build

        private static void TryAdd(List<CollectedEntry> result, HashSet<FFF_InfoDef> seen, InfoDisplayExtension manual, InfoEntry entry, object source, bool pawnCardOnly)
        {
            if (entry == null || entry.def == null)
                return;
            if (pawnCardOnly && !entry.showOnPawnCard)
                return;
            if (manual != null && manual.Suppresses(entry.def))
                return;
            if (!seen.Add(entry.def))
                return;
            result.Add(new CollectedEntry(entry, source));
        }

        private static StatDrawEntry MakeDrawEntry(Def owner, StatRequest req, CollectedEntry collected, string sourceLabel, ref int index)
        {
            InfoEntry entry = collected.entry;
            FFF_InfoDef def = entry.def;
            InfoContext ctx = new InfoContext(owner, req, collected.source);
            InfoWorker worker;
            try
            {
                worker = def.Worker;
            }
            catch (Exception e)
            {
                Log.ErrorOnce($"[FFF] Failed to create InfoWorker {def.workerClass} for {def.defName}: {e}", def.GetHashCode());
                return Fallback(def, entry, sourceLabel, ref index);
            }

            try
            {
                if (!worker.Visible(def, ctx))
                    return null;

                int priority = entry.priorityOverride ?? (def.displayPriority - index);
                index++;
                // 純文字條目：右欄留空，內文放 reportText（同原版 Description 條目）
                return new StatDrawEntry(
                    def.Category,
                    WithSource(worker.GetLabel(def, ctx), sourceLabel),
                    string.Empty,
                    worker.GetDescription(def, ctx),
                    priority,
                    null,
                    Dialog_InfoCard.DefsToHyperlinks(def.hyperlinks),
                    false,
                    def.overridesHideStats);
            }
            catch (Exception e)
            {
                Log.ErrorOnce($"[FFF] InfoWorker {worker.GetType()} failed for {def.defName} on {owner.defName}: {e}", def.GetHashCode() ^ owner.GetHashCode());
                return Fallback(def, entry, sourceLabel, ref index);
            }
        }

        // Worker 出錯時退回靜態文本
        private static StatDrawEntry Fallback(FFF_InfoDef def, InfoEntry entry, string sourceLabel, ref int index)
        {
            int priority = entry.priorityOverride ?? (def.displayPriority - index);
            index++;
            return new StatDrawEntry(
                def.Category,
                WithSource(def.LabelCap, sourceLabel),
                string.Empty,
                def.description,
                priority,
                null,
                Dialog_InfoCard.DefsToHyperlinks(def.hyperlinks),
                false,
                def.overridesHideStats);
        }

        // 標題加上來源後綴，例如「彈藥切換 (突擊步槍)」
        private static string WithSource(string label, string sourceLabel)
        {
            if (sourceLabel.NullOrEmpty())
                return label;
            return "FFF.InfoDisplay.SourceSuffix".Translate(label, sourceLabel);
        }

        #endregion
    }
}
