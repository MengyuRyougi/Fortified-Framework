# Recycleable 使用手冊 — 倒地／死亡單位原地拆解

`Fortified.CompProperties_Recycleable` 掛在 Pawn 的 ThingDef 上（通常與 `CompProperties_Drone` 並列），
讓該單位在**倒地**或**死亡（屍體）**時可以被殖民者就地拆解成資源，不需要搬回工作台。

## 1. 最小用法

```xml
<comps>
  <li Class="Fortified.CompProperties_Drone">
    <returnToDraftPlatformJob>FFF_ReturnToDronePlatform</returnToDraftPlatformJob>
  </li>
  <li Class="Fortified.CompProperties_Recycleable" />
</comps>
<butcherProducts>
  <Steel>25</Steel>
</butcherProducts>
```

不填 `products` 時直接沿用 race 的 `butcherProducts`。以上設定的結果：
倒地拆解得到 25 鋼，屍體拆解得到 25 × 0.5 ≈ 12 鋼。

## 2. 全部欄位

| 欄位 | 預設 | 說明 |
|---|---|---|
| `products` | null | `ThingDefCountClass` 清單。留空 = 用 `butcherProducts`。支援 `stuff`、`chance`。 |
| `yieldFactor` | 1.0 | 基礎產出倍率。 |
| `deadYieldFactor` | 0.5 | 屍體狀態額外乘上的倍率。 |
| `scaleByHealth` | false | 倒地時依 `SummaryHealthPercent` 縮放產出。 |
| `efficiencyStat` | null | 再乘上工作者的 StatDef（例如 `ButcheryMechanoidEfficiency`）。 |
| `workTicks` | 600 | 基礎工作量。 |
| `workSpeedStat` | null | 工作量除以工作者的這個 stat（例如 `GeneralLaborSpeed`）。 |
| `allowDowned` | true | 倒地時可拆。 |
| `allowDead` | true | 屍體可拆。 |
| `dropEquipment` | true | 拆解前把裝備／衣物／物品欄丟到地上。 |
| `playerFactionOnly` | false | 只允許拆玩家陣營的個體。false 時屍體不限陣營；倒地個體限玩家、無陣營或敵對（不能拆友軍）。 |
| `workEffecter` | null | 作業中的 effecter，null = `ConstructMetal`。 |
| `finishSound` | null | 完成時播放的音效。 |
| `gizmoIconPath` | `UI/Designators/Deconstruct` | gizmo 圖示。 |

產出計算：`count = RoundRandom(base × yieldFactor × (dead ? deadYieldFactor : 1) × (scaleByHealth ? health% : 1) × efficiencyStat)`。

## 3. 玩家操作

- 選取倒地單位或屍體 → gizmo「拆解回收」（Command_Toggle），tooltip 顯示預估產出。
- 右鍵：選取殖民者後右鍵目標 → 「拆解 X（預估產出）」，會同時打上標記並立刻派工。
- 標記本身是 `DesignationDef FFF_RecycleDesignation`，用原版「取消」指定器即可移除。
- 自動工作：`WorkGiverDef FFF_RecycleWorkGiver`（Construction，priority 60）會派有建造工作的殖民者去處理已標記的目標。

## 4. 行為細節

- 倒地單位被搬走（DeSpawn）時標記會暫存，落地後若仍倒地會自動補回。
- 已標記的倒地單位死亡時，標記會自動轉移到屍體上（`allowDead` 為 true 時）。
- 倒地個體的移除方式與平台 Retract 相同（`DeSpawnOrDeselect` + `Destroy`），不走 `Kill`：
  不會發死亡通知、不觸發 `DeathActionWorker`（例如死亡爆炸）、也不影響陣營關係。
  `CompMechPlatform.CleanupSpawnedPawns` 會依 `Destroyed` 把它從已部署清單移除，**不會退款**。
- 屍體走 `Corpse.Destroy()`。
- 訊息卡：`FFF_Info_Recycleable` 透過 autoAttachTo 自動附加到所有帶 `CompProperties_Recycleable` 的 Def。

## 5. C# 介面

```csharp
CompRecycleable comp = CompRecycleable.Get(thing);   // thing 可以是 Pawn 或 Corpse
comp.CanRecycle();                                   // AcceptanceReport：目前狀態是否允許
comp.Designated;                                     // 是否已標記
CompRecycleable.SetDesignated(thing, true);          // [SyncMethod] 打／取消標記
comp.ProductsSummary(worker);                        // UI 用預估產出字串
comp.Recycle(worker);                                // 立即執行拆解（JobDriver 完成時呼叫）
```
