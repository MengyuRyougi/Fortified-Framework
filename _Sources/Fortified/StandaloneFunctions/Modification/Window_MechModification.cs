using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Fortified
{
    public static class MechModificationWindowUtility
    {
        private static readonly List<Func<Pawn, Window>> WindowFactories = new List<Func<Pawn, Window>>();

        public static void RegisterWindowFactory(Func<Pawn, Window> factory)
        {
            if (factory != null && !WindowFactories.Contains(factory)) WindowFactories.Add(factory);
        }

        public static bool CanOpenFor(Pawn pawn)
        {
            return pawn != null
                && pawn.Spawned
                && !pawn.Dead
                && !pawn.Downed
                && pawn.Faction == Faction.OfPlayer
                && pawn.RaceProps?.IsMechanoid == true
                && ResearchProjectDefOf.MicroelectronicsBasics?.IsFinished == true;
        }

        public static void OpenFor(Pawn pawn)
        {
            if (!CanOpenFor(pawn)) return;
            for (int i = 0; i < WindowFactories.Count; i++)
            {
                Window window = WindowFactories[i](pawn);
                if (window != null)
                {
                    Find.WindowStack.Add(window);
                    return;
                }
            }
            Find.WindowStack.Add(new Window_MechModification(pawn));
        }
    }

    public enum MechModificationOperationKind
    {
        Install,
        Uninstall,
        Custom
    }

    public class MechModificationQueueEntry
    {
        public MechModificationOperationKind kind;
        public Thing item;
        public ThingDef itemDef;
        public BodyPartRecord part;
        public Hediff uninstallHediff;
        public bool custom;
        public bool allowEquivalentPart;
        public bool submitted;
        public string customId;
        public string customData;
    }

    /// <summary>
    /// Plans a set of install/uninstall operations for one mech and hands them to the mech as
    /// ordered jobs on Apply. Follows the vanilla dialog pattern: the window pauses the game,
    /// data is rebuilt only when something changed, and accepting closes the window.
    /// </summary>
    public class Window_MechModification : Window
    {
        private const float TitleHeight = 32f;
        private const float FooterHeight = 40f;
        private const float LeftColumnWidth = 360f;
        private const float PortraitSize = 160f;
        private const float SectionHeaderHeight = 26f;
        private const float SectionPadding = 6f;
        private const float SectionGap = 8f;
        private const float RowHeight = 28f;
        private const float IconSize = 24f;
        private const float ButtonSize = 24f;
        private const float Indent = 16f;
        private const float ActionButtonHeight = 26f;
        private const float ActionButtonWidth = 180f;
        // Only matters when the game keeps running underneath the window (multiplayer ignores forcePause).
        private const float AvailableRefreshSeconds = 1f;

        protected readonly Pawn Mech;
        protected readonly List<MechModificationQueueEntry> QueuedOperations = new List<MechModificationQueueEntry>();

        private Vector2 partScroll;
        private Vector2 availableScroll;
        private Vector2 queueScroll;
        private readonly List<SlotInfo> slots = new List<SlotInfo>();
        private readonly List<Hediff> installedSnapshot = new List<Hediff>();
        private readonly List<Thing> availableItems = new List<Thing>();
        private SlotInfo selectedSlot;
        private BodyPartRecord corePart;
        private bool slotsDirty = true;
        private bool availableDirty = true;
        private float availableBuiltAt = -1f;

        public override Vector2 InitialSize => new Vector2(Mathf.Min(940f, UI.screenWidth - 40f), Mathf.Min(720f, UI.screenHeight - 40f));

        protected BodyPartRecord SelectedPart => selectedSlot?.part;

        protected virtual Texture2D FooterIcon => null;
        protected virtual string ApplyButtonLabel => "FFF.MechModification.Apply".Translate();
        protected virtual string ResetButtonLabel => "FFF.MechModification.Reset".Translate();
        protected virtual string ReturnButtonLabel => "FFF.MechModification.Return".Translate();

        public Window_MechModification(Pawn mech)
        {
            Mech = mech;
            forcePause = true;
            doCloseX = true;
            closeOnCancel = true;
            absorbInputAroundWindow = true;
            closeOnClickedOutside = true;
        }

        public override void PreOpen()
        {
            base.PreOpen();
            RebuildSlots();
            selectedSlot = slots.FirstOrDefault();
            OnCorePreOpen();
        }

        protected virtual void OnCorePreOpen()
        {
        }

        public override void PostClose()
        {
            QueuedOperations.Clear();
            base.PostClose();
        }

        public override void DoWindowContents(Rect inRect)
        {
            if (Mech == null || !MechModificationWindowUtility.CanOpenFor(Mech))
            {
                Close();
                return;
            }
            RefreshIfNeeded();
            DrawWindowContents(inRect);
        }

        // ---------------------------------------------------------------- state

        /// <summary>Marks the available-item list for rebuild. Call after changing QueuedOperations.</summary>
        protected void MarkDirty()
        {
            availableDirty = true;
        }

        private void RefreshIfNeeded()
        {
            if (!slotsDirty && !InstalledMatchesSnapshot()) slotsDirty = true;
            if (slotsDirty) RebuildSlots();
            float now = Time.realtimeSinceStartup;
            if (availableDirty || (!Find.TickManager.Paused && now - availableBuiltAt >= AvailableRefreshSeconds)) RebuildAvailableItems();
        }

        private bool InstalledMatchesSnapshot()
        {
            List<Hediff> hediffs = Mech.health?.hediffSet?.hediffs;
            if (hediffs == null) return installedSnapshot.Count == 0;
            int index = 0;
            for (int i = 0; i < hediffs.Count; i++)
            {
                if (!IsInstalledModification(hediffs[i])) continue;
                if (index >= installedSnapshot.Count || installedSnapshot[index] != hediffs[i]) return false;
                index++;
            }
            return index == installedSnapshot.Count;
        }

        private void RebuildSlots()
        {
            slotsDirty = false;
            BodyPartRecord previousPart = selectedSlot?.part;
            slots.Clear();
            installedSnapshot.Clear();
            if (Mech?.RaceProps?.body == null) return;
            corePart = Mech.RaceProps.body.corePart;

            HashSet<BodyPartDef> targetDefs = new HashSet<BodyPartDef>();
            foreach (ThingDef def in ModificationProfileDatabase.ModificationDefs)
            {
                CompProperties_AddHediffOnTarget props = def.GetCompProperties<CompProperties_AddHediffOnTarget>();
                if (props?.targetBodyPartDefs.NullOrEmpty() != false)
                {
                    if (corePart?.def != null) targetDefs.Add(corePart.def);
                    continue;
                }
                for (int j = 0; j < props.targetBodyPartDefs.Count; j++) targetDefs.Add(props.targetBodyPartDefs[j]);
            }
            AddCustomTargetPartDefs(targetDefs);

            List<BodyPartRecord> allParts = Mech.RaceProps.body.AllParts;
            for (int i = 0; i < allParts.Count; i++)
            {
                BodyPartRecord part = allParts[i];
                if (targetDefs.Any(target => part.def == target || CustomPartMatches(part, target))) slots.Add(new SlotInfo(part));
            }

            List<Hediff> hediffs = Mech.health?.hediffSet?.hediffs ?? new List<Hediff>();
            for (int i = 0; i < hediffs.Count; i++)
            {
                Hediff hediff = hediffs[i];
                if (!IsInstalledModification(hediff)) continue;
                installedSnapshot.Add(hediff);
                BodyPartRecord displayPart = hediff.Part ?? corePart;
                if (displayPart == null) continue;
                SlotInfo slot = slots.FirstOrDefault(candidate => candidate.part == displayPart);
                if (slot == null)
                {
                    slot = new SlotInfo(displayPart);
                    slots.Add(slot);
                }
                ThingDef source = hediff.TryGetComp<HediffComp_Modification>()?.SourceThingDef ?? ModificationProfileDatabase.GetSource(hediff.def);
                if (source == null) TryGetCustomSourceThing(hediff, out source);
                slot.installed.Add(new InstalledModification { hediff = hediff, displayPart = displayPart, source = source });
            }
            slots.Sort((left, right) => left.part.Index.CompareTo(right.part.Index));

            // Hediffs that disappeared underneath us (job finished while the game ran) cannot be uninstalled any more.
            QueuedOperations.RemoveAll(entry => entry?.kind == MechModificationOperationKind.Uninstall && !installedSnapshot.Contains(entry.uninstallHediff));
            selectedSlot = slots.FirstOrDefault(slot => slot.part == previousPart) ?? selectedSlot;
            if (selectedSlot != null && !slots.Contains(selectedSlot)) selectedSlot = slots.FirstOrDefault();
            availableDirty = true;
        }

        private void RebuildAvailableItems()
        {
            availableDirty = false;
            availableBuiltAt = Time.realtimeSinceStartup;
            availableItems.Clear();
            BodyPartRecord part = SelectedPart;
            Map map = Mech?.Map;
            if (map == null || part == null) return;

            IEnumerable<ThingDef> defs = ModificationProfileDatabase.ModificationDefs
                .Concat(GetAdditionalInstallItemDefs() ?? Enumerable.Empty<ThingDef>())
                .Where(def => def != null)
                .Distinct();
            foreach (ThingDef def in defs)
            {
                List<Thing> things = ModificationUtility.GetCandidates(map, def, Mech, Mech.Position, true);
                if (things.Count == 0) continue;
                // Stock check once per def; CanInstall below runs without its own inventory scan.
                int available = 0;
                for (int i = 0; i < things.Count; i++) available += ModificationUtility.AvailableCount(things[i], Mech);
                int pending = QueuedOperations.Count(entry => entry?.kind == MechModificationOperationKind.Install && entry.itemDef == def);
                if (pending >= available) continue;
                for (int i = 0; i < things.Count; i++)
                {
                    if (!CanUseThingOnPart(things[i], part)) continue;
                    availableItems.Add(things[i]);
                    break;
                }
            }
            availableItems.Sort((left, right) =>
            {
                int byLabel = string.Compare(left.LabelCapNoCount, right.LabelCapNoCount, StringComparison.CurrentCulture);
                return byLabel != 0 ? byLabel : string.CompareOrdinal(left.def.defName, right.def.defName);
            });
        }

        private bool CanUseThingOnPart(Thing item, BodyPartRecord part)
        {
            if (TryBuildCustomInstallEntry(item, part, out MechModificationQueueEntry customEntry))
            {
                return ValidateBuiltInstallEntry(customEntry, false, out _);
            }
            return item != null && ModificationInstallValidator.CanInstall(Mech, item.def, part, QueuedOperations, out _, false);
        }

        private bool IsInstalledModification(Hediff hediff)
        {
            return hediff?.TryGetComp<HediffComp_Modification>() != null || IsCustomInstalledHediff(hediff);
        }

        private bool IsQueuedForUninstall(Hediff hediff)
        {
            return QueuedOperations.Any(entry => entry?.kind == MechModificationOperationKind.Uninstall && entry.uninstallHediff == hediff);
        }

        // ---------------------------------------------------------------- drawing

        protected virtual void DrawWindowContents(Rect inRect)
        {
            Rect titleRect = new Rect(inRect.x, inRect.y, inRect.width - 40f, TitleHeight);
            Text.Font = GameFont.Medium;
            Widgets.Label(titleRect, "FFF.MechModification.WindowTitle".Translate(Mech.LabelShortCap));
            Text.Font = GameFont.Small;

            Rect contentRect = new Rect(inRect.x, titleRect.yMax + 4f, inRect.width, inRect.height - TitleHeight - 4f - FooterHeight - 6f);
            Rect footerRect = new Rect(inRect.x, contentRect.yMax + 6f, inRect.width, FooterHeight);
            Rect leftRect = new Rect(contentRect.x, contentRect.y, LeftColumnWidth, contentRect.height);
            Rect rightRect = new Rect(leftRect.xMax + 12f, contentRect.y, contentRect.width - LeftColumnWidth - 12f, contentRect.height);
            DrawLeft(leftRect);
            DrawRight(rightRect);
            DrawFooter(footerRect);
        }

        /// <summary>Label with a separator line underneath, drawn inside <paramref name="rect"/> (Widgets.ListSeparator always starts at x = 0).</summary>
        private static void DrawSectionHeader(Rect rect, string label)
        {
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(new Rect(rect.x + 4f, rect.y, rect.width - 8f, rect.height), label);
            Text.Anchor = TextAnchor.UpperLeft;
            Color previous = GUI.color;
            GUI.color = Widgets.SeparatorLineColor;
            Widgets.DrawLineHorizontal(rect.x, rect.yMax - 1f, rect.width);
            GUI.color = previous;
        }

        /// <summary>Draws a menu section with a header row and returns the header rect; <paramref name="body"/> is the remaining inner area.</summary>
        private static Rect Section(Rect rect, string header, out Rect body)
        {
            Widgets.DrawMenuSection(rect);
            Rect inner = rect.ContractedBy(SectionPadding);
            Rect headerRect = new Rect(inner.x, inner.y, inner.width, SectionHeaderHeight);
            DrawSectionHeader(headerRect, header);
            body = new Rect(inner.x, headerRect.yMax + 4f, inner.width, inner.yMax - headerRect.yMax - 4f);
            return headerRect;
        }

        private void DrawLeft(Rect rect)
        {
            float portraitSectionHeight = PortraitSize + SectionPadding * 2f;
            Rect portraitRect = new Rect(rect.x, rect.y, rect.width, portraitSectionHeight);
            Widgets.DrawMenuSection(portraitRect);
            DrawPortrait(portraitRect.ContractedBy(SectionPadding));

            Rect partsRect = new Rect(rect.x, portraitRect.yMax + SectionGap, rect.width, rect.height - portraitSectionHeight - SectionGap);
            Section(partsRect, "FFF.MechModification.SlotsTitle".Translate(), out Rect partsBody);
            DrawPartsList(partsBody);
        }

        private void DrawPortrait(Rect rect)
        {
            Rect portraitRect = new Rect(rect.x, rect.y, PortraitSize, PortraitSize);
            Widgets.DrawBoxSolid(portraitRect, new Color(0f, 0f, 0f, 0.2f));
            GUI.DrawTexture(portraitRect, PortraitsCache.Get(Mech, portraitRect.size, Rot4.South, Vector3.zero), ScaleMode.ScaleToFit);

            const float lineHeight = 22f;
            Rect infoRect = new Rect(portraitRect.xMax + 10f, rect.y + 4f, rect.xMax - portraitRect.xMax - 10f, rect.height - 8f);
            Text.Anchor = TextAnchor.MiddleLeft;
            Rect line = new Rect(infoRect.x, infoRect.y, infoRect.width - ButtonSize - 4f, lineHeight);
            Widgets.Label(line, "FFF.MechModification.InfoName".Translate(Mech.Name?.ToStringShort ?? Mech.LabelCap.ToString()));
            line.y += lineHeight;
            Widgets.Label(line, "FFF.MechModification.InfoRace".Translate(Mech.def?.LabelCap ?? "-"));
            line.y += lineHeight;
            Widgets.Label(line, "FFF.MechModification.InfoAge".Translate(Mech.ageTracker?.AgeBiologicalYears.ToString() ?? "0"));
            Text.Anchor = TextAnchor.UpperLeft;

            Widgets.InfoCardButton(infoRect.xMax - ButtonSize, infoRect.y, Mech);
        }

        private void DrawPartsList(Rect rect)
        {
            float viewHeight = 0f;
            for (int i = 0; i < slots.Count; i++) viewHeight += RowHeight * (1 + slots[i].installed.Count);
            Rect view = new Rect(0f, 0f, rect.width - 16f, Mathf.Max(rect.height, viewHeight));
            Widgets.BeginScrollView(rect, ref partScroll, view);
            float y = 0f;
            for (int i = 0; i < slots.Count; i++)
            {
                SlotInfo slot = slots[i];
                Rect header = new Rect(0f, y, view.width, RowHeight);
                Widgets.DrawOptionBackground(header, selectedSlot == slot);
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(header.ContractedBy(4f, 0f), slot.part.LabelCap);
                Text.Anchor = TextAnchor.UpperLeft;
                if (Widgets.ButtonInvisible(header) && selectedSlot != slot)
                {
                    selectedSlot = slot;
                    MarkDirty();
                }
                y += RowHeight;
                for (int j = 0; j < slot.installed.Count; j++)
                {
                    DrawInstalledRow(new Rect(Indent, y, view.width - Indent, RowHeight), slot.installed[j]);
                    y += RowHeight;
                }
            }
            Widgets.EndScrollView();
        }

        private void DrawInstalledRow(Rect row, InstalledModification installed)
        {
            Hediff hediff = installed.hediff;
            bool pendingUninstall = IsQueuedForUninstall(hediff);
            Widgets.DrawHighlightIfMouseover(row);
            TooltipHandler.TipRegion(row, new TipSignal(() => hediff.GetTooltip(Mech, false), hediff.loadID));

            float x = row.x;
            if (installed.source != null)
            {
                Widgets.ThingIcon(new Rect(x, row.y + (RowHeight - IconSize) * 0.5f, IconSize, IconSize), installed.source);
            }
            x += IconSize + 4f;

            string label = hediff.LabelCap;
            int count = hediff.TryGetComp<HediffComp_Modification>()?.InstalledCount ?? 1;
            if (count > 1) label += " x" + count;
            if (pendingUninstall) label += " (" + "FFF.MechModification.PendingUninstall".Translate() + ")";
            Rect labelRect = new Rect(x, row.y, row.xMax - x - ButtonSize * 2f - 8f, RowHeight);
            Color previousColor = GUI.color;
            if (pendingUninstall) GUI.color = Color.gray;
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(labelRect, label.Truncate(labelRect.width));
            Text.Anchor = TextAnchor.UpperLeft;
            GUI.color = previousColor;

            float buttonY = row.y + (RowHeight - ButtonSize) * 0.5f;
            if (installed.source != null) Widgets.InfoCardButton(row.xMax - ButtonSize * 2f - 4f, buttonY, installed.source);
            else Widgets.InfoCardButton(row.xMax - ButtonSize * 2f - 4f, buttonY, hediff.def);

            Rect removeRect = new Rect(row.xMax - ButtonSize, buttonY, ButtonSize, ButtonSize);
            TooltipHandler.TipRegion(removeRect, "FFF.Modification_Remove".Translate(hediff.LabelCap));
            if (Widgets.ButtonImage(removeRect, pendingUninstall ? TexButton.Plus : TexButton.Delete))
            {
                if (pendingUninstall) QueuedOperations.RemoveAll(entry => entry?.kind == MechModificationOperationKind.Uninstall && entry.uninstallHediff == hediff);
                else QueueUninstall(installed);
                MarkDirty();
            }
        }

        protected virtual void DrawRight(Rect rect)
        {
            DrawModificationPanel(rect);
        }

        protected void DrawModificationPanel(Rect rect)
        {
            float availableHeight = Mathf.Round((rect.height - SectionGap) * 0.58f);
            Rect availableRect = new Rect(rect.x, rect.y, rect.width, availableHeight);
            Rect queueRect = new Rect(rect.x, availableRect.yMax + SectionGap, rect.width, rect.height - availableHeight - SectionGap);

            string availableTitle = selectedSlot != null
                ? "FFF.MechModification.SelectModsFor".Translate(selectedSlot.part.LabelCap)
                : "FFF.MechModification.SelectPartPrompt".Translate();
            Rect availableHeader = Section(availableRect, availableTitle, out Rect availableBody);
            const float buttonGap = 4f;
            float halfWidth = (ActionButtonWidth - buttonGap) * 0.5f;
            float buttonY = availableHeader.y + (availableHeader.height - ActionButtonHeight) * 0.5f - 2f;
            Rect saveRect = new Rect(availableHeader.xMax - ActionButtonWidth, buttonY, halfWidth, ActionButtonHeight);
            Rect loadRect = new Rect(saveRect.xMax + buttonGap, buttonY, halfWidth, ActionButtonHeight);
            if (Widgets.ButtonText(saveRect, "FFF.MechModification.SavePreset".Translate())) Find.WindowStack.Add(new Dialog_MechModificationPresetSave(BuildPreset));
            if (Widgets.ButtonText(loadRect, "FFF.MechModification.LoadPreset".Translate())) Find.WindowStack.Add(new Dialog_MechModificationPresetLoad(ApplyPreset));
            DrawAvailableItems(availableBody);

            Section(queueRect, "FFF.MechModification.QueueTitle".Translate(), out Rect queueBody);
            DrawQueuedOperations(queueBody);
        }

        private void DrawAvailableItems(Rect rect)
        {
            Rect view = new Rect(0f, 0f, rect.width - 16f, Mathf.Max(rect.height, availableItems.Count * RowHeight));
            Widgets.BeginScrollView(rect, ref availableScroll, view);
            if (availableItems.Count == 0)
            {
                Widgets.Label(new Rect(4f, 0f, view.width - 8f, RowHeight), "FFF.MechModification.NoModsOnMap".Translate());
            }
            for (int i = 0; i < availableItems.Count; i++)
            {
                Thing item = availableItems[i];
                Rect row = new Rect(0f, i * RowHeight, view.width, RowHeight);
                if (row.yMax < availableScroll.y || row.y > availableScroll.y + rect.height) continue;
                if (i % 2 == 1) Widgets.DrawLightHighlight(row);
                Widgets.DrawHighlightIfMouseover(row);
                TooltipHandler.TipRegion(row, new TipSignal(() => item.DescriptionDetailed, item.thingIDNumber));

                Widgets.ThingIcon(new Rect(row.x, row.y + (RowHeight - IconSize) * 0.5f, IconSize, IconSize), item);
                Rect labelRect = new Rect(row.x + IconSize + 4f, row.y, row.width - IconSize - ButtonSize - 12f, RowHeight);
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(labelRect, item.LabelCapNoCount.Truncate(labelRect.width));
                Text.Anchor = TextAnchor.UpperLeft;
                Widgets.InfoCardButton(row.xMax - ButtonSize, row.y + (RowHeight - ButtonSize) * 0.5f, item);

                if (Widgets.ButtonInvisible(row) && TryCreateInstallEntry(item, SelectedPart, out MechModificationQueueEntry entry))
                {
                    QueuedOperations.Add(entry);
                    MarkDirty();
                    break;
                }
            }
            Widgets.EndScrollView();
        }

        private void DrawQueuedOperations(Rect rect)
        {
            Rect view = new Rect(0f, 0f, rect.width - 16f, Mathf.Max(rect.height, QueuedOperations.Count * RowHeight));
            Widgets.BeginScrollView(rect, ref queueScroll, view);
            if (QueuedOperations.Count == 0)
            {
                Widgets.Label(new Rect(4f, 0f, view.width - 8f, RowHeight), "FFF.MechModification.QueueEmpty".Translate());
            }
            for (int i = 0; i < QueuedOperations.Count; i++)
            {
                MechModificationQueueEntry entry = QueuedOperations[i];
                Rect row = new Rect(0f, i * RowHeight, view.width, RowHeight);
                if (i % 2 == 1) Widgets.DrawLightHighlight(row);
                Widgets.DrawHighlightIfMouseover(row);
                Rect labelRect = new Rect(row.x + 4f, row.y, row.width - ButtonSize - 8f, RowHeight);
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(labelRect, GetQueueLabel(entry).Truncate(labelRect.width));
                Text.Anchor = TextAnchor.UpperLeft;
                if (!entry.submitted && Widgets.ButtonImage(new Rect(row.xMax - ButtonSize, row.y + (RowHeight - ButtonSize) * 0.5f, ButtonSize, ButtonSize), TexButton.Delete))
                {
                    QueuedOperations.RemoveAt(i--);
                    MarkDirty();
                }
            }
            Widgets.EndScrollView();
        }

        private string GetQueueLabel(MechModificationQueueEntry entry)
        {
            string missing = "FFF.MechModification.Missing".Translate();
            string partLabel = entry.part?.LabelCap.ToString() ?? corePart?.LabelCap.ToString() ?? "FFF.MechModification.PartFallback".Translate();
            switch (entry.kind)
            {
                case MechModificationOperationKind.Uninstall:
                    return "FFF.MechModification.QueueUninstall".Translate(entry.uninstallHediff?.LabelCap ?? missing, partLabel);
                case MechModificationOperationKind.Custom:
                    return GetCustomQueueLabel(entry);
                default:
                    string itemLabel = entry.item?.LabelCapNoCount ?? entry.itemDef?.LabelCap.ToString() ?? missing;
                    return "FFF.MechModification.QueueInstall".Translate(itemLabel, partLabel);
            }
        }

        protected virtual void DrawFooter(Rect rect)
        {
            const float buttonWidth = 160f;
            Rect resetRect = new Rect(rect.xMax - buttonWidth * 3f - 16f, rect.y, buttonWidth, rect.height);
            Rect applyRect = new Rect(rect.xMax - buttonWidth * 2f - 8f, rect.y, buttonWidth, rect.height);
            Rect returnRect = new Rect(rect.xMax - buttonWidth, rect.y, buttonWidth, rect.height);

            if (FooterIcon != null)
            {
                float iconSize = rect.height - 8f;
                Widgets.DrawTextureFitted(new Rect(rect.x + 4f, rect.y + 4f, iconSize, iconSize), FooterIcon, 1f);
            }
            if (Widgets.ButtonText(resetRect, ResetButtonLabel))
            {
                QueuedOperations.RemoveAll(entry => entry?.submitted != true);
                MarkDirty();
                OnResetClicked();
            }
            if (Widgets.ButtonText(applyRect, ApplyButtonLabel)) ApplyAndStartJobs();
            if (Widgets.ButtonText(returnRect, ReturnButtonLabel)) Close();
        }

        // ---------------------------------------------------------------- queue editing

        protected void QueueCustomOperation(string id, string data = null)
        {
            QueuedOperations.Add(new MechModificationQueueEntry
            {
                kind = MechModificationOperationKind.Custom,
                custom = true,
                customId = id,
                customData = data
            });
            MarkDirty();
        }

        protected bool HasCustomOperation(string id)
        {
            return QueuedOperations.Any(entry => entry.kind == MechModificationOperationKind.Custom && entry.customId == id);
        }

        private bool TryCreateInstallEntry(Thing item, BodyPartRecord part, out MechModificationQueueEntry entry)
        {
            string reason = null;
            if (TryBuildCustomInstallEntry(item, part, out entry))
            {
                if (ValidateBuiltInstallEntry(entry, true, out reason)) return true;
            }
            else if (item?.TryGetComp<CompTargetable_AddHediffOnTarget>() == null)
            {
                reason = "FFF.MechModification.InvalidModification".Translate();
            }
            else if (ModificationInstallValidator.CanInstall(Mech, item.def, part, QueuedOperations, out reason))
            {
                entry = new MechModificationQueueEntry
                {
                    kind = MechModificationOperationKind.Install,
                    item = item,
                    itemDef = item.def,
                    part = part
                };
                return true;
            }
            if (!reason.NullOrEmpty()) Messages.Message(reason, Mech, MessageTypeDefOf.RejectInput, false);
            entry = null;
            return false;
        }

        private bool ValidateBuiltInstallEntry(MechModificationQueueEntry entry, bool checkInventory, out string reason)
        {
            reason = null;
            if (entry == null) return false;
            ThingDef itemDef = entry.itemDef ?? entry.item?.def;
            // Custom entries that are not standard modifications are the extension's own business.
            if (entry.kind != MechModificationOperationKind.Install || !ModificationProfileDatabase.IsModificationDef(itemDef)) return true;
            return ModificationInstallValidator.CanInstall(Mech, itemDef, entry.part, QueuedOperations, out reason, checkInventory, entry.allowEquivalentPart);
        }

        private void QueueUninstall(InstalledModification installed)
        {
            if (installed?.hediff == null || IsQueuedForUninstall(installed.hediff)) return;
            bool custom = IsCustomInstalledHediff(installed.hediff);
            if (!custom && installed.hediff.TryGetComp<HediffComp_Modification>() == null) return;
            // Removals go first so a slot freed by one is available to the installs behind it.
            QueuedOperations.Insert(0, new MechModificationQueueEntry
            {
                kind = MechModificationOperationKind.Uninstall,
                uninstallHediff = installed.hediff,
                part = installed.displayPart,
                custom = custom
            });
        }

        // ---------------------------------------------------------------- apply

        protected virtual bool ValidateBeforeCommit(out string rejectionReason)
        {
            rejectionReason = null;
            return true;
        }

        protected virtual void CommitCustomSettings()
        {
        }

        protected virtual void NotifyJobsStarted(int count)
        {
            Messages.Message("FFF.MechModification.AppliedQueued".Translate(count), Mech, MessageTypeDefOf.PositiveEvent, false);
        }

        private void ApplyAndStartJobs()
        {
            if (!ValidateStandardQueue(out string rejectionReason) || !ValidateBeforeCommit(out rejectionReason))
            {
                if (!rejectionReason.NullOrEmpty()) Messages.Message(rejectionReason, Mech, MessageTypeDefOf.RejectInput, false);
                return;
            }
            CommitCustomSettings();
            int started = StartQueuedJobs();
            if (started == 0 && QueuedOperations.Count == 0)
            {
                Messages.Message("FFF.MechModification.QueueEmpty".Translate(), Mech, MessageTypeDefOf.RejectInput, false);
                return;
            }
            NotifyJobsStarted(started);
            Close();
        }

        private bool ValidateStandardQueue(out string rejectionReason)
        {
            rejectionReason = null;
            List<MechModificationQueueEntry> preceding = QueuedOperations.Where(entry => entry?.submitted == true).ToList();
            for (int i = 0; i < QueuedOperations.Count; i++)
            {
                MechModificationQueueEntry entry = QueuedOperations[i];
                if (entry == null || entry.submitted) continue;
                if (entry.kind == MechModificationOperationKind.Install && !entry.custom
                    && ModificationProfileDatabase.IsModificationDef(entry.itemDef)
                    && !ModificationInstallValidator.CanInstall(Mech, entry.itemDef, entry.part, preceding, out rejectionReason, true, entry.allowEquivalentPart))
                {
                    return false;
                }
                preceding.Add(entry);
            }
            return true;
        }

        private int StartQueuedJobs()
        {
            if (Mech?.jobs == null || Mech.Map == null) return 0;
            int started = 0;
            // Mirror vanilla shift-queueing: the first new order interrupts whatever the mech is
            // doing, later ones line up behind it.
            bool queue = QueuedOperations.Any(entry => entry != null && entry.submitted);
            for (int i = 0; i < QueuedOperations.Count; i++)
            {
                MechModificationQueueEntry entry = QueuedOperations[i];
                if (entry == null || entry.submitted || !TryIssueEntry(entry, queue)) continue;
                entry.submitted = true;
                queue = true;
                started++;
            }
            return started;
        }

        private bool TryIssueEntry(MechModificationQueueEntry entry, bool queue)
        {
            // Extension-defined jobs cannot be described with plain data, so they are issued
            // directly and are the extension's responsibility to synchronise in multiplayer.
            // Custom entries the extension declines to handle fall back to the standard jobs.
            if (entry.custom && TryCreateCustomJob(entry, out Job customJob) && customJob != null)
            {
                return Mech.jobs.TryTakeOrderedJob(customJob, JobTag.MiscWork, queue);
            }
            switch (entry.kind)
            {
                case MechModificationOperationKind.Uninstall:
                    if (entry.uninstallHediff?.def == null) return false;
                    MechModificationOrders.OrderUninstall(Mech, entry.uninstallHediff.def, entry.uninstallHediff.Part?.Index ?? -1, queue);
                    return true;
                case MechModificationOperationKind.Install:
                    if (entry.itemDef == null || FFF_DefOf.FFF_Modification == null) return false;
                    Thing item = entry.item;
                    if (item == null || item.Destroyed || !item.Spawned) item = FindClosestInstallItem(entry.itemDef);
                    MechModificationOrders.OrderInstall(Mech, entry.itemDef, item, entry.part?.Index ?? -1, entry.allowEquivalentPart, queue);
                    return true;
                default:
                    return false;
            }
        }

        private Thing FindClosestInstallItem(ThingDef def)
        {
            if (def == null || Mech?.Map == null) return null;
            List<Thing> things = ModificationUtility.GetCandidates(Mech.Map, def, Mech, Mech.Position, true);
            return things.Count == 0 ? null : things[0];
        }

        // ---------------------------------------------------------------- extension points

        protected virtual bool TryBuildCustomInstallEntry(Thing item, BodyPartRecord part, out MechModificationQueueEntry entry)
        {
            entry = null;
            return false;
        }

        protected virtual bool TryRestoreCustomInstallEntry(ThingDef itemDef, BodyPartRecord part, out MechModificationQueueEntry entry)
        {
            entry = null;
            return false;
        }

        protected virtual IEnumerable<ThingDef> GetAdditionalInstallItemDefs()
        {
            yield break;
        }

        protected virtual void AddCustomTargetPartDefs(HashSet<BodyPartDef> targetDefs)
        {
        }

        protected virtual bool CustomPartMatches(BodyPartRecord part, BodyPartDef targetDef)
        {
            return false;
        }

        protected virtual bool IsCustomInstalledHediff(Hediff hediff)
        {
            return false;
        }

        protected virtual bool TryGetCustomSourceThing(Hediff hediff, out ThingDef source)
        {
            source = null;
            return false;
        }

        protected virtual bool TryCreateCustomJob(MechModificationQueueEntry entry, out Job job)
        {
            job = null;
            return false;
        }

        protected virtual string GetCustomQueueLabel(MechModificationQueueEntry entry)
        {
            return entry.customId ?? "FFF.MechModification.Missing".Translate();
        }

        protected virtual void OnResetClicked()
        {
        }

        // ---------------------------------------------------------------- presets

        private MechModificationPreset BuildPreset()
        {
            MechModificationPreset preset = new MechModificationPreset();
            for (int i = 0; i < QueuedOperations.Count; i++)
            {
                MechModificationQueueEntry operation = QueuedOperations[i];
                if (operation.kind == MechModificationOperationKind.Custom) continue;
                MechModificationPresetEntry entry = new MechModificationPresetEntry
                {
                    uninstall = operation.kind == MechModificationOperationKind.Uninstall,
                    partDefName = operation.part?.def?.defName,
                    partIndex = operation.part?.Index ?? -1
                };
                if (entry.uninstall)
                {
                    entry.hediffDefName = operation.uninstallHediff?.def?.defName;
                    if (!entry.hediffDefName.NullOrEmpty()) preset.entries.Add(entry);
                }
                else
                {
                    entry.itemDefName = operation.itemDef?.defName ?? operation.item?.def?.defName;
                    if (!entry.itemDefName.NullOrEmpty()) preset.entries.Add(entry);
                }
            }
            return preset;
        }

        private void ApplyPreset(MechModificationPreset preset)
        {
            QueuedOperations.RemoveAll(entry => entry?.submitted != true);
            MarkDirty();
            if (preset?.entries == null) return;
            for (int i = 0; i < preset.entries.Count; i++)
            {
                MechModificationPresetEntry presetEntry = preset.entries[i];
                if (presetEntry == null) continue;
                BodyPartRecord part = FindPart(presetEntry.partIndex, presetEntry.partDefName);
                if (part == null) continue;
                if (presetEntry.uninstall)
                {
                    Hediff hediff = FindInstalledHediff(presetEntry.hediffDefName, part);
                    if (hediff != null && !IsQueuedForUninstall(hediff))
                    {
                        QueuedOperations.Add(new MechModificationQueueEntry
                        {
                            kind = MechModificationOperationKind.Uninstall,
                            uninstallHediff = hediff,
                            part = part,
                            custom = IsCustomInstalledHediff(hediff)
                        });
                    }
                    continue;
                }

                ThingDef itemDef = DefDatabase<ThingDef>.GetNamedSilentFail(presetEntry.itemDefName);
                if (itemDef == null) continue;
                if (TryRestoreCustomInstallEntry(itemDef, part, out MechModificationQueueEntry customEntry))
                {
                    customEntry.itemDef = itemDef;
                    customEntry.item = FindClosestInstallItem(itemDef);
                    if (ValidateBuiltInstallEntry(customEntry, true, out _)) QueuedOperations.Add(customEntry);
                    continue;
                }
                if (!ModificationProfileDatabase.IsModificationDef(itemDef) || !ModificationInstallValidator.CanInstall(Mech, itemDef, part, QueuedOperations, out _)) continue;
                QueuedOperations.Add(new MechModificationQueueEntry
                {
                    kind = MechModificationOperationKind.Install,
                    itemDef = itemDef,
                    item = FindClosestInstallItem(itemDef),
                    part = part
                });
            }
        }

        private BodyPartRecord FindPart(int index, string defName)
        {
            List<BodyPartRecord> parts = Mech?.RaceProps?.body?.AllParts;
            if (parts == null) return null;
            if (index >= 0 && index < parts.Count)
            {
                BodyPartRecord indexed = parts[index];
                if (defName.NullOrEmpty() || indexed.def?.defName == defName) return indexed;
            }
            return defName.NullOrEmpty() ? null : parts.FirstOrDefault(part => part.def?.defName == defName);
        }

        private Hediff FindInstalledHediff(string defName, BodyPartRecord part)
        {
            if (defName.NullOrEmpty()) return null;
            for (int i = 0; i < installedSnapshot.Count; i++)
            {
                Hediff hediff = installedSnapshot[i];
                bool samePart = hediff.Part == part || (hediff.Part == null && part == corePart);
                if (samePart && hediff.def?.defName == defName) return hediff;
            }
            return null;
        }

        private class SlotInfo
        {
            public readonly BodyPartRecord part;
            public readonly List<InstalledModification> installed = new List<InstalledModification>();

            public SlotInfo(BodyPartRecord part)
            {
                this.part = part;
            }
        }

        private class InstalledModification
        {
            public Hediff hediff;
            public BodyPartRecord displayPart;
            public ThingDef source;
        }
    }
}
