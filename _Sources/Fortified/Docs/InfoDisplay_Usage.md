# InfoDisplay 使用手冊 — 編寫「特殊機制」文本

訊息卡（Dialog_InfoCard）的「特殊機制」分類，用純文字說明 Thing / Pawn / Hediff / Ability / Recipe 等具備的機制。
所有功能由 `Fortified.InfoDisplayUtility` 統一處理；本文列出目前可用的全部功能與寫法。
設計背景見 [InfoDisplay_Plan.md](InfoDisplay_Plan.md)，完整範例見 [InfoDisplay_Example.xml](InfoDisplay_Example.xml)。

---

## 1. 定義文本：`Fortified.FFF_InfoDef`

一條 InfoDef = 一段可復用的機制說明。放在任意 `Defs/*.xml`。

```xml
<Fortified.FFF_InfoDef>
  <defName>MyMod_Info_Overheat</defName>
  <label>overheating</label>                         <!-- 左欄標題 -->
  <description>Sustained fire builds up heat…</description>  <!-- 點擊後右側內文 -->
  <displayPriority>1000</displayPriority>            <!-- 分類內排序，越大越前，預設 1000 -->
  <hyperlinks>                                       <!-- 內文下方可點的連結，元素名 = Def 型別 -->
    <HediffDef>MyMod_Hediff_Overheated</HediffDef>
    <ThingDef>MyMod_Coolant</ThingDef>
  </hyperlinks>
  <category>FFF_Mechanics</category>                 <!-- 預設 FFF_Mechanics，可換任一 StatCategoryDef -->
  <workerClass>MyMod.InfoWorker_Overheat</workerClass> <!-- 動態文字，見 §5 -->
  <overridesHideStats>true</overridesHideStats>      <!-- hideStats=true 的 Def 仍顯示此條目 -->
  <autoAttachTo>                                     <!-- 宣告式自動附加，見 §2.3 -->
    <li>MyMod.CompProperties_Heat</li>
  </autoAttachTo>
  <autoAttachInherit>true</autoAttachInherit>        <!-- 預設 true：含子類；false 只比對完全相同型別 -->
</Fortified.FFF_InfoDef>
```

| 欄位 | 型別 | 必填 | 說明 |
|---|---|---|---|
| `label` | string | ✅ | 條目標題 |
| `description` | string | ✅ | 條目內文 |
| `displayPriority` | int | – | 分類內排序基準（預設 1000） |
| `hyperlinks` | List\<DefHyperlink\> | – | 內文下方連結 |
| `category` | StatCategoryDef | – | 預設 `FFF_Mechanics` |
| `workerClass` | Type | – | `InfoWorker` 子類，預設基底 |
| `overridesHideStats` | bool | – | 預設 false |
| `autoAttachTo` | List\<Type\> | – | 自動附加的目標型別 |
| `autoAttachInherit` | bool | – | 預設 true |

---

## 2. 掛載方式（三種，可混用）

### 2.1 手動：`InfoDisplayExtension`（零 C#）

掛在任意 Def 的 `modExtensions`：ThingDef、HediffDef、GeneDef、AbilityDef、RecipeDef、TerrainDef…

```xml
<modExtensions>
  <li Class="Fortified.InfoDisplayExtension">
    <infos>
      <li>MyMod_Info_Overheat</li>                    <!-- 簡寫 -->
      <li>
        <def>FFF_Info_HeavyEquippable</def>           <!-- 展開寫法，可覆寫欄位 -->
        <priorityOverride>5</priorityOverride>        <!-- 覆寫此 Def 上的排序 -->
        <showOnPawnCard>false</showOnPawnCard>        <!-- 不彙總到 Pawn 卡（見 §9） -->
      </li>
    </infos>
    <suppress>
      <li>FFF_Info_AmmoSwitch</li>                    <!-- 排除自動附加進來的條目 -->
    </suppress>
  </li>
</modExtensions>
```

`<li>` 支援 `MayRequire` / `MayRequireAnyOf` 屬性。

### 2.2 自動（介面式）：`IInfoProvider`

C# 端由 CompProperties / HediffCompProperties / AbilityCompProperties / DefModExtension / ThingComp 實作，
可依欄位內容或執行期狀態決定是否顯示。

```csharp
public class CompProperties_Heat : CompProperties, IInfoProvider
{
    public FFF_InfoDef infoDef;                        // 慣例：留欄位讓 XML 覆寫
    public float maxHeat;

    public IEnumerable<InfoEntry> GetInfoEntries(InfoContext ctx)
    {
        if (maxHeat <= 0f) yield break;                // 條件不成立就不顯示
        yield return new InfoEntry(infoDef ?? MyDefOf.MyMod_Info_Overheat)
        {
            priorityOverride = 50,                     // 可動態設定
        };
    }
}
```

框架內建實作（皆有 `infoDef` 欄位可在 XML 覆寫）：

| 類別 | 顯示條件 | 預設 InfoDef |
|---|---|---|
| `CompProperties_AmmoSwitch` | `ammos` 非空 | `FFF_Info_AmmoSwitch` |
| `CompProperties_DamageBlocker` | 永遠 | `FFF_Info_DamageBlocker` |
| `HediffCompProperties_DamageBlocker` | 永遠 | `FFF_Info_DamageBlocker` |
| `ModExt_EnvironmentalBill` | `AnyRestriction` | `FFF_Info_EnvironmentalBill` |
| `CompProperties_Overseer` | 永遠；依 owner 是 Pawn 或建築選條目 | `FFF_Info_OverseerMech` / `FFF_Info_OverseerBuilding` |

```xml
<!-- XML 覆寫內建條目 -->
<li Class="Fortified.CompProperties_AmmoSwitch">
  <infoDef>MyMod_Info_MyAmmoText</infoDef>
  …
</li>
```

### 2.3 自動（宣告式）：`autoAttachTo`（零 C#）

在 InfoDef 上宣告「owner Def 帶有哪些 C# 型別時自動顯示」。適合原版 / 第三方類別、或「型別存在即顯示」的機制。

```xml
<autoAttachTo>
  <li>Fortified.CompPropertiesMultipleTurretGun</li>  <!-- CompProperties -->
  <li>Fortified.HeavyEquippableExtension</li>         <!-- DefModExtension -->
  <li>Fortified.MinifiedThingDeployable</li>          <!-- thingClass（Type） -->
  <li>Fortified.CompDronePack</li>                    <!-- compClass（共用原版 Props 的自訂 ThingComp） -->
  <li>RimWorld.CompProperties_Explosive</li>          <!-- 原版類別也可以 -->
</autoAttachTo>
```

C# 端等價寫法（例如下游 Mod 對原版型別註冊）：

```csharp
InfoDisplayUtility.RegisterAutoAttach(typeof(CompProperties_Explosive), MyDefOf.MyMod_Info_Explosive, inherit: true);
```

框架內建宣告（完整清單見 `1.6/Defs/InfoDefs/`）：

| 檔案 | 內容 |
|---|---|
| `InfoDefs_Framework.xml` | HeavyEquippable、MultiTurret、InternalBattery、Deployable、BiochemicalProtection（+ 三條 `IInfoProvider` 用的 AmmoSwitch / DamageBlocker / EnvironmentalBill） |
| `InfoDefs_Mech.xml` | 無人機平台 / 無人機 / 無人機背包 / 接觸引爆、人形機械體、武器系統、砲塔操作、休眠、封存機械體、休眠機械體、統御者單位 / 統御核心、失能開關、人工生命、自我修復、陶瓷護板、機械體改裝件、指令中繼、控制中繼、訊號衰減、武器銘印、服裝支援 |
| `InfoDefs_Projectile.xml` | 集束彈藥、定向爆炸（掛在投射物 thingClass / ModExtension，顯示於武器卡） |
| `InfoDefs_Misc.xml` | 偽裝、周界掃描器、屏蔽裝置、訊號廣播、掃描式警報器 / 警報響應 / 觸發式警報器、遠端操作台、門禁鎖 / 安全門、可移動結構、一次性鋁熱切割、自毀程序、電力樞紐、有人工事、自動工作台、品質優化、環境豁免、燃料生產、後勤終端、備貨容器、密封儲藏箱、碾壓、橫掃攻擊、裝甲板、主動防護、遮蔽煙幕、威懾存在、懸浮、空中支援、可裝填武器、可塗裝 |

下游 Mod 的 Def 只要帶有對應型別（CompProperties / compClass / DefModExtension / thingClass）就會自動顯示，不需任何設定；不想顯示時用 `InfoDisplayExtension.suppress` 排除。

---

## 3. 來源物件範圍（`autoAttachTo` / `IInfoProvider` 能比對到什麼）

| owner Def | 列舉的來源物件 |
|---|---|
| 任意 Def | Def 本身、`modExtensions[*]` |
| ThingDef | `thingClass`(Type)、`comps[*]`、`comps[*].compClass`(Type)、`verbs[*]`、`tools[*]`、`race`、`apparel`、`building` |
| ThingDef（武器） | 另含 `verbs[*].defaultProjectile` 的 `thingClass`(Type) 與 `modExtensions[*]`——投射物機制顯示在武器卡上 |
| ThingDef（有實例） | 另含 `ThingWithComps.AllComps[*]`（ThingComp 實例） |
| Hediff 實例（Hediff 卡 / Pawn 彙總） | Hediff 本身、`HediffWithComps.comps[*]` |
| Gene 實例（Pawn 彙總） | Gene 本身 |
| HediffDef | `hediffClass`(Type)、`comps[*]`、`comps[*].compClass`(Type)、`stages[*]` |
| AbilityDef | `abilityClass`(Type)、`comps[*]`、`comps[*].compClass`(Type)、`verbProperties` |
| GeneDef | `geneClass`(Type) |
| RecipeDef | `workerClass`(Type) |

下游自訂 Def 型別可擴充：

```csharp
InfoDisplayUtility.RegisterSourceEnumerator<MyCustomDef>(d => d.parts);
```

---

## 4. 翻譯

InfoDef 是 Def，走 DefInjected（`label` / `description`）；同一 InfoDef 被多處引用時只需翻譯一次。

**文本流程**：英文 Def 文本（`1.6/Defs/InfoDefs/*.xml`）是母本，但**撰寫與修訂以繁中 DefInjected 為起點**——先寫或改繁中，再把繁中回翻成英文母本；簡中由繁中轉換生成（zhconv 字級轉換 + 詞彙修正）。三者內容必須一致。

```
Languages/<語言>/DefInjected/Fortified.FFF_InfoDef/<任意檔名>.xml
```

```xml
<LanguageData>
  <MyMod_Info_Overheat.label>過熱</MyMod_Info_Overheat.label>
  <MyMod_Info_Overheat.description>持續射擊會累積熱量…</MyMod_Info_Overheat.description>
</LanguageData>
```

- 分類標題 `FFF_Mechanics.label` 在 `DefInjected/StatCategoryDef/`。

---

## 5. 動態文字：`InfoWorker`

同一段模板要帶數值（「可承受 {0} 次」）時，覆寫 Worker 從上下文取值。

```csharp
public class InfoWorker_Heat : InfoWorker
{
    public override bool Visible(FFF_InfoDef def, InfoContext ctx)
        => ctx.SourceAs<CompProperties_Heat>()?.maxHeat > 0f;

    public override string GetDescription(FFF_InfoDef def, InfoContext ctx)
    {
        var props = ctx.SourceAs<CompProperties_Heat>();
        var comp  = ctx.Thing?.TryGetComp<CompHeat>();          // 有實例時可讀執行期狀態
        return def.description.Formatted(props.maxHeat, comp?.CurrentHeat ?? 0f);
    }
}
```

`InfoContext` 提供：

| 成員 | 說明 |
|---|---|
| `owner` | 訊息卡所屬 Def |
| `req` | `StatRequest`；`req.HasThing` 時有實例 |
| `Thing` | `req.Thing` 或 null |
| `source` | 觸發此條目的來源物件（CompProperties / Extension / ThingComp / Type…） |
| `SourceAs<T>()` | `source as T` |

可覆寫：`Visible` / `GetLabel` / `GetDescription`。條目右欄固定留空，內文放 `description`（同原版 Description 條目）。
Worker 拋例外時訊息卡不會壞，會 `Log.ErrorOnce` 並退回 Def 的靜態文本。

---

## 6. 排序與去重規則

- 分類：`FFF_Mechanics`（displayOrder 14，緊接 Basics 之後）；InfoDef 可改 `category`。
- 分類內：`priorityOverride` > `displayPriority - 列舉順序`。同 priority 依標題字母排。
- 同一 InfoDef 在同一張卡只顯示一次，**先到先贏**。列舉順序：Def 本身 → modExtensions（手動 Extension 在此）→ comps / verbs / … → ThingComp 實例。
  所以手動 Extension 的 `priorityOverride` 會優先於自動附加的同一 InfoDef。
- `suppress` 對該 Def 上所有來源生效。

---

## 7. C# API 總覽（`InfoDisplayUtility`）

```csharp
// 輸出到訊息卡（Harmony Postfix 已接好，一般不需自己呼叫）
IEnumerable<StatDrawEntry> BuildEntries(Def owner, StatRequest req);
IEnumerable<StatDrawEntry> BuildEntries(Def owner, StatRequest req, HashSet<FFF_InfoDef> seen, bool pawnCardOnly = false, string sourceLabel = null);
IEnumerable<StatDrawEntry> BuildEntriesFromSources(Def owner, StatRequest req, IEnumerable<object> sources, HashSet<FFF_InfoDef> seen, bool pawnCardOnly = false, string sourceLabel = null);
IEnumerable<StatDrawEntry> BuildPawnAggregateEntries(Pawn pawn);

// 只收集不輸出：給 tooltip / gizmo 說明等其他 UI 重用
List<CollectedEntry> CollectEntries(Def owner, StatRequest req);
List<CollectedEntry> CollectEntriesFromSources(Def owner, StatRequest req, IEnumerable<object> sources, HashSet<FFF_InfoDef> seen, bool pawnCardOnly);
void MarkSeen(Def owner, StatRequest req, HashSet<FFF_InfoDef> seen);

// 註冊
void RegisterAutoAttach(Type sourceType, FFF_InfoDef def, bool inherit = true);
void RegisterSourceEnumerator<TDef>(Func<TDef, IEnumerable<object>> enumerator);

// 列舉來源物件
IEnumerable<object> EnumerateSources(Def owner, StatRequest req);
IEnumerable<object> HediffInstanceSources(Hediff hediff);
IEnumerable<object> ThingInstanceSources(Thing thing);
```

已接好的 Hook：`Def.SpecialDisplayStats`（涵蓋 ThingDef / BuildableDef / TerrainDef / GeneDef / AbilityDef）、`HediffDef.SpecialDisplayStats`、`RecipeDef.SpecialDisplayStats`、`Hediff.SpecialDisplayStats`（實例）、`Pawn.SpecialDisplayStats`（彙總，見 §9）。
掛在 race ThingDef 上的條目直接出現在 Pawn 卡。

---

## 8. 建議寫法

- **一個機制一條 InfoDef**，描述寫「機制怎麼運作、玩家該怎麼互動」，數值交給既有 Stat 條目或 Worker。
- 顯示與否取決於欄位內容 → `IInfoProvider`；型別存在就顯示 → `autoAttachTo`；個別 Def 的補充說明 → `InfoDisplayExtension`。
- 內建 CompProperties 的條目文本要客製 → 用 `infoDef` 欄位換掉，不要重寫 provider。
- `autoAttachTo` 只對具體型別宣告；對 `CompProperties` 這類基底宣告會附加到所有 Def。

---

## 9. Pawn 卡彙總與實例層級來源

**Pawn 訊息卡**會自動彙總身上物件的機制條目（`InfoDisplayUtility.BuildPawnAggregateEntries`）：

| 來源 | 掃描範圍 | 標題後綴 |
|---|---|---|
| Hediff（`Visible`） | HediffDef 來源 + Hediff 實例 + HediffComp 實例 | Hediff 標籤 |
| 服裝 `WornApparel` | ThingDef 來源 + Apparel 實例 + ThingComp 實例 | 服裝名 |
| 武器 `AllEquipmentListForReading` | 同上 | 武器名 |
| 基因（Biotech，`Active`） | GeneDef 來源 + Gene 實例 | 基因名 |

- 只收 `InfoEntry.showOnPawnCard == true` 的條目（預設 true；手動 Extension 可設 false）。
- race ThingDef 本身的條目先標記為已見，同一 InfoDef 全卡只顯示一次，先到先贏（race → Hediff → 服裝 → 武器 → 基因）。
- 後綴格式由 Keyed `FFF.InfoDisplay.SourceSuffix`（`{0} ({1})`）決定。

**Hediff 訊息卡**（健康頁點擊）除 HediffDef 層級外，也會列舉 Hediff 實例與 HediffComp 實例；
`HediffComp` 實作 `IInfoProvider` 可依執行期狀態（層數、嚴重度）決定顯示。

```csharp
public class HediffComp_Shield : HediffComp, IInfoProvider
{
    public IEnumerable<InfoEntry> GetInfoEntries(InfoContext ctx)
    {
        if (charges > 0) yield return new InfoEntry(MyDefOf.MyMod_Info_Shield);
    }
}
```

實例來源列舉可直接取用：`InfoDisplayUtility.HediffInstanceSources(hediff)`、`ThingInstanceSources(thing)`；
自訂來源清單走 `BuildEntriesFromSources(owner, req, sources, seen, pawnCardOnly, sourceLabel)`；
`MarkSeen(owner, req, seen)` 只標記不輸出，用於避免與另一條輸出路徑重複。
