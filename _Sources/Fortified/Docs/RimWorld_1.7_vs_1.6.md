# RimWorld 1.7 與 1.6 差異總結

只談遊戲本體的差異，不涉及任何模組的更新內容。

| | 1.6 | 1.7 |
|---|---|---|
| 遊戲版本 | 1.6.4871 rev590 | 1.7.4976 rev1043 |
| `Assembly-CSharp` 版本 | 1.6.9676.17735 | 1.7.9781.31297 |

**資料來源**：反編譯並比對兩版的 `Assembly-CSharp.dll`、`UnityStubs.dll`、`Waylith.AssetBake.dll`，加上 1.7 遊戲目錄與執行記錄檔（2026-10-13）。
原版 XML（`Data/`）只做了局部檢查，沒有逐檔比對；遊戲內容平衡面的變動不在本文範圍。

---

## 1. 一句話總結

**1.7 是一次引擎移植，不是內容改版。** 遊戲從 Unity 換成自研的 Waylith 引擎（.NET 10 + Vulkan），但遊戲邏輯層的 C# API 幾乎沒變，Def 結構也沒有欄位被移除。對玩家而言是效能與畫面底層的改變；對模組而言，主要衝擊在建置方式、Harmony 版本、著色器與資源格式。

---

## 2. 引擎與執行環境

| 項目 | 1.6 | 1.7 |
|---|---|---|
| 引擎 | Unity 2022.3 | Waylith（自研） |
| 執行環境 | Mono（.NET Framework 4.7.2 API） | CoreCLR 客製版（`.NET 10.0.10-waylithclr.dev`） |
| 繪圖 API | Windows：D3D11；macOS：Metal；Linux：OpenGL／Vulkan | 全平台 Vulkan |
| 視窗／輸入 | Unity | GLFW |
| 著色器 | Unity ShaderLab（預先編譯） | GLSL，執行時編譯；附 Slang 編譯器 |
| 音訊 | Unity Audio + NVorbis／RuntimeAudioClipLoader | 引擎內建（`waylithaudio.dll`） |
| `UnityEngine` 型別 | Unity 本體 | `UnityStubs.dll` 相容層（在 Waylith 上實作 Unity API） |
| 啟動程式 | `RimWorldWin64.exe` | `RimWorld.exe` → `RimWorld.dll` |

1.7 的 `RimWorld.runtimeconfig.json` 對執行環境做了大量放寬，目的是讓舊模組盡量能直接載入：

- 組件名稱重導：`UnityEngine*`、`Unity.*`、`Assembly-CSharp-firstpass` → `UnityStubs`；
- 寬鬆的組件綁定、型別與成員簽章解析，並略過存取檢查；
- 字串雜湊、浮點數格式化採用 Mono 的行為，避免結果和 1.6 不一致；
- `Assembly-CSharp` 標記為可修補，JIT 不會把它的方法內聯進動態方法，讓 Harmony 補丁穩定生效。

---

## 3. 安裝目錄結構

| | 1.6 | 1.7 |
|---|---|---|
| 遊戲 DLL | `RimWorldWin64_Data/Managed/` | 遊戲根目錄（`Assembly-CSharp.dll`、`UnityStubs.dll`、`Waylith*.dll`、`Brutal.*.dll`、整套 .NET 執行環境） |
| 原版資源 | Unity 資源檔（`*_Data/`） | 遊戲根目錄的 `Assets.zip`（約 357 MB） |
| 引擎著色器 | 內建於資源檔 | `Rendering/Shaders/`、`Data/Core/Shaders/` |
| 執行記錄檔 | `…/LocalLow/Ludeon Studios/RimWorld by Ludeon Studios/Player.log` | 同資料夾下的 `Logs/Player.log` |
| 新增快取 | — | 同資料夾下的 `ShaderCache/`、`TextureCache/` |

`Data/` 底下的 DLC 沒有變化：Core、Royalty、Ideology、Biotech、Anomaly、Odyssey。

1.7 的記錄檔格式也不同：每行帶時間戳、等級與分類（例如 `WARN [Verse]`），預設附上呼叫堆疊。

---

## 4. 程式碼 API 差異

### 4.1 規模

| | 1.6 | 1.7 |
|---|---|---|
| 型別總數（含編譯器產生） | 16,158 | 16,215 |
| 方法總數 | 76,770 | 77,371 |
| 一般型別（不含編譯器產生） | 9,662 | 9,704 |

一般型別中：

- 移除 24 個型別、新增 66 個型別；
- 移除 54 個成員、新增 136 個成員。

其中相當一部分只是編譯產物的差異，並非 API 改動：委派建構子參數從 `IntPtr` 改為 `nint`、lambda 編號重排等。

**Def 類別沒有任何欄位被移除或改名**，1.6 的 XML 在結構上可以直接用於 1.7。

**DefOf 完全沒有變動**：

- 兩版都是 165 個 `[DefOf]` 類別、共 3,693 個欄位；
- 欄位名稱、型別與 `MayRequire` 等屬性一致，沒有新增或移除；
- 新增的 `ShaderDef`、`MaterialDef` 也沒有對應的 DefOf，而是由 `ShaderDatabase`／`MatLoader` 以名稱解析。

### 4.2 移除的部分

| 類別 | 內容 |
|---|---|
| Burst／Unity Jobs | `MapGenUtility`、`FastTileFinder`、`PlanetLayer` 的 Burst 編譯版本函式與委派 |
| 音訊載入 | `NVorbis.NAudioSupport.VorbisWaveReader`、`RuntimeAudioClipLoader.*`；`ModContentLoader` 的 `GetAudioTypeFromURI`、`GetFormat`、`ShouldStreamAudioClipFromFile` |
| 貼圖圖集內部 | `StaticTextureAtlas` 舊的壓縮與 Mipmap 流程（`ApplyTextureCompression`、`GenerateMipmapsWithCompute` 等） |
| 其他 | `SteamDeck.IsVR`、`MatLoader.loadedDirect`、`LoadedContentItem.extraDisposable`、`ModAssemblyHandler.globalResolverIsSet`、`ResolutionUtility.borderlessFullscreenCached`、自帶的 `PriorityQueue` 填充實作 |

另外，`CameraDriver.MyCamera` 由 private 改為 public。

### 4.3 編譯器產出的差異

1.7 的 `Assembly-CSharp` 以 .NET 10 為目標編譯，部分語法編出的 IL 和 1.6 不同。例如字串插值在 1.6 是 `string.Format`，1.7 改為 `DefaultInterpolatedStringHandler`。
**反編譯後的 C# 看起來一樣，IL 卻可能不同**，這會影響依賴特定 IL 樣式的 Transpiler。

---

## 5. 新增功能

### 5.1 著色器與材質

- **`ShaderDef`**：以 XML 定義著色器程式。GLSL 原始檔放在載入資料夾的 `Shaders/`（原版位於 `Data/Core/Shaders/`），定義位於 `Data/Core/Defs/ShaderDefs/`。
  - 可設定的欄位包括 `vertexShaderFileName`、`fragmentShaderFileName`、`transparent`、`depthWrite`、`renderQueue`、`cullMode`、`flipTextureST`、`textureWrapOverrides` 等。
  - 若 ShaderDef 尚未註冊對應程式，會警告「no engine program ported yet」並改用引擎預設著色器。
  - 從模組資料夾載入的模組，其著色器檔案支援熱重載。
- **`ShaderDatabase.LoadShader("Map/Xxx")` 的解析順序**：
  1. 取路徑最後一段 `Xxx`，找同名的 `ShaderDef`；
  2. 找不到時，到模組的 AssetBundle 中尋找；
  3. 再找不到時，建立延遲綁定的著色器。
- **`MaterialDef`**：以 XML 定義材質，可指定 `shader`（ShaderDef）、`renderQueue`、`textures`、`floats`、`colors`、`vectors`、`keywords`。`MatLoader` 載入材質時會先嘗試對應的 MaterialDef。
- `renderQueue` 改用 `RenderQueueValue`，可解析帶偏移量的字串（`ParseHelper.ParseRenderQueue`）。
- 新增 `ShaderWarmup`（著色器預熱）與 `LudeonTK.GenTexture`。

### 5.2 貼圖與繪圖

- `GraphicData` 新增 `borderMipMaps`、`textureRepeats` 兩個欄位。
- 靜態貼圖圖集會壓縮成 BC 格式（可用 GPU 編碼），並快取到磁碟（`TextureCache/`）；壓縮後的來源貼圖可能從 CPU 記憶體釋放（`EvictAtlasSourceTextures`）。
- 模組貼圖若有同名 `.dds`，會略過同名的 `.png`／`.jpg`（`ModContentLoader.ShadowedByDds`）。
- 模組檔案路徑會預先掃描（`ModContentLoader.PrefetchPaths`），加快載入。
- `ContentFinder<T>` 新增模組 AssetBundle 資產查詢與快取。

### 5.3 遊戲邏輯層的小幅新增

- `GlowGrid.RegisterCustomGlower` / `DeregisterCustomGlower`：不依附 Thing 的自訂光源。
- `Building_Door.willCloseSoonForJobs` / `SnapshotWillCloseSoonForJobs()`：門的關閉狀態快照。
- `Rand.InsideUnitSphere`。
- 音訊：`AudioGrain.forceToMono`、`Sample.startFrame`。

### 5.4 模組管理

- **命令列指定模組**：`-mod=id1;id2`（或 `-mod id1,id2`）。會啟用核心、所有相容的官方 DLC 與列出的模組，並自動排序。可搭配 `-quicktest`、`-savedatafolder=` 做隔離測試。
- `ModContentPack.AllLoadFolders(mod)`：列出一個模組所有版本的載入資料夾。
- **以 1.6 的模組設定首次啟動 1.7 時，遊戲會重設模組清單**為只有原版與 DLC（記錄檔：`Mods config data is from version 1.6.4871 … Resetting.`）。

### 5.5 創意工坊上傳：資源包

1.7 的上傳確認視窗新增兩個選項：

- 「建立資源包」：預設勾選；
- 「略過原始貼圖」：只在建立資源包時可選。

勾選建立資源包後，上傳流程變為：

1. 把每個載入資料夾的 `Textures/`、`Sounds/` 烘焙成該資料夾內的 `Assets.zip`（貼圖以 BC7 壓縮）。附屬檔 `Assets.zip.*` 只供本機增量烘焙使用。
2. 把整個模組複製到 `%TEMP%/RimWorldWorkshopUpload/<packageId>/`：
   - 排除 `Assets.zip.*` 附屬檔；
   - 若勾選「略過原始貼圖」，也排除已打包進資源包的原始貼圖。
3. 上傳這份複本（`ModMetaData.workshopUploadDirectoryOverride`），結束後刪除。

載入時，遊戲會自動掛載各載入資料夾內的 `Assets.zip`。

### 5.6 設定、視窗與開發工具

- 偏好設定新增 `exclusiveFullscreen`、視窗位置 `windowPositionX/Y`、`FullScreenMode`；選項畫面新增顯示模式選單。
- 新增完整的效能分析器介面 `Verse.ProfilerUI`（`Window_FrameProfiler`、各組件耗時、VRAM 表、火焰圖等，約 55 個型別）。
- `Log` 支援從非主執行緒寫入（訊息先排隊，再回到主執行緒輸出），並接上引擎的日誌系統。

---

## 6. 舊模組的相容機制

### 6.1 組件（DLL）

- 1.6 時代以 .NET Framework 4.7.2 / Unity 編譯的 DLL，靠第 2 節的重導與放寬設定，**在設計上仍可被 1.7 載入**；實際能否正常運作取決於用到的 Unity API 是否由 `UnityStubs` 完整實作。
- `UnityStubs` 實作了大部分常用 API，例如 `Graphics.Blit`（含材質）、`RenderTexture`、`Texture2D.ReadPixels`、`AssetBundle`。不保證涵蓋全部。
- Harmony 改為 **Harmony 3**（Harmony 模組 1.7 資料夾；與 NuGet `Lib.Harmony 3.0.0-rc.2` 的 net10.0 組件完全相同）。和 Harmony 2 的差別：
  - **型別檢查更嚴格**：補丁方法的 `__instance` 若宣告成與原方法無關的型別，Harmony 2 會放行，Harmony 3 會擲出例外。
  - **一個補丁類別失敗，`PatchAll` 就會中止**，其後的補丁全部不會套用。

### 6.2 載入資料夾（行為未變，但影響 1.7 的過渡期）

- `LoadFolders.xml` 沒有 `v1.7` 時，會改用「不大於目前版本的最高版本」設定，因此**只支援到 1.6 的模組會直接載入它的 1.6 資料夾**。
- 列在後面的載入資料夾優先：不同資料夾中相同相對路徑的檔案（DLL、XML、AssetBundle 皆然），只會採用最後一個。

### 6.3 AssetBundle

`UnityStubs` 以 AssetsTools.NET 直接讀取 Unity 的 AssetBundle（貼圖、網格、預製物、著色器），**仍依作業系統後綴挑選檔案**（Windows 讀 `_win`）；載入後的 bundle 名稱即為檔名。

著色器的限制最多：

- **只能使用 Vulkan（SPIR-V）變體。** Unity 預設只有 Linux 目標會編出 Vulkan，Windows 版（D3D11）與 macOS 版（Metal）的 bundle 中的著色器都無法使用（記錄檔：`no Vulkan variant in this bundle (not a linux bundle?)`）。
- 不支援幾何與曲面細分著色器。
- 常數緩衝區合計不得超過 512 bytes；儲存緩衝區最多 8 個。
- 多個關鍵字變體時，取關鍵字最少的那個。
- 混合模式會轉換為 Vulkan 對應設定，無法對應的會退回一般 Alpha 混合；GrabPass 會轉換為場景擷取。

---

## 7. 對模組開發的影響

| 項目 | 1.6 做法 | 1.7 做法 |
|---|---|---|
| 目標框架 | `net472` | `net10.0` |
| 遊戲參考 | NuGet `Krafs.Rimworld.Ref` | 直接參考遊戲根目錄的 `Assembly-CSharp.dll`、`UnityStubs.dll`、`Waylith.dll`（本文撰寫時 Krafs 尚無 1.7 版） |
| Harmony | `Lib.Harmony 2.4.x` | `Lib.Harmony 3.0.0-rc.2` |
| 自訂著色器 | AssetBundle（各平台） | GLSL + `ShaderDef`，或 AssetBundle 的 Vulkan 變體（linux 版 bundle） |
| 反編譯參考 | `RimWorldWin64_Data/Managed/` | 遊戲根目錄 |

常見問題：

- **IDE 自動加入的 `using` 會導致 1.7 編譯失敗**。`NVorbis`、`RuntimeAudioClipLoader`、`Mono.*`、`System.Runtime.Remoting` 等命名空間在 1.7 已不存在。
- **NuGet 套件可能連帶引入 1.6 的參考組件**，在 `net10.0` 下造成 Unity 型別重複（CS0433）。例如 `RimWorld.MultiplayerAPI` 會連帶引入 `Krafs.Rimworld.Ref`。
- **Transpiler 要以 1.7 的 IL 重新確認**，不能只看反編譯結果（見 4.3）。
- **以字串指定的反射目標**（私有欄位名、`<>c__DisplayClass…` 等編譯器產生的名稱）要逐一確認仍然存在。
- 同時支援 1.6 與 1.7 的模組上傳時**不要勾選「略過原始貼圖」**：1.6 無法讀取 `Assets.zip`，1.6 玩家會缺貼圖。

---

## 8. 尚未確認的部分

- 原版 XML 內容（數值、配方、事件等）沒有逐檔比對。
- `UnityStubs` 只抽查了常用 API，並非所有 Unity API 都確認過行為一致。
- 舊版 .NET Framework 模組在 1.7 下能否正常運作，取決於個別模組；本文只描述相容機制本身。
