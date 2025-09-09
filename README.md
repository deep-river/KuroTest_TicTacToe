# 井字棋游戏

 使用Unity 实现的井字棋游戏，项目中主要展示了**可维护的代码架构、多档难度的游戏 AI、数据驱动且支持热重载的游戏难度配置与规范化的 UI 框架**。


## 玩法与模式

**井字棋规则**：3×3 棋盘，X/O 轮流落子；横/竖/斜任一连成 3 子即胜，否则棋满为和。
* **快速模式**：选择难度与局数，按过半局数获胜的规则判定胜负或平局。
* **无尽模式**：从简单难度起步，依据玩家的连胜/连败记录动态微调 AI 难度。

---

## 功能特性

### 游戏AI

* **Minimax** 策略，**深度加权评分**（快赢优先/慢输次之）：

  * `Win = +10 - ply`，`Lose = -10 + ply`，`Draw = 0`。
* **同分打破**采用**位置偏好**（中心＞角＞边），仅在评分相等时介入，不影响最优性。
* 难度“软化”参数：`depthLimit`、`mistakeRate`、`randomizeAmongBest`。

### 数据驱动的游戏配置

* 难度表存储为 **Resources/Config/difficulty.json**；运行时载入，可在运行时通过调试面板实现**热重载**。
* 在**快速游戏** 采用简单/普通/困难三档固定档位；**Endless** 将阶梯参数映射至三档显示名，细粒度控制 `mistakeRate` 等。

### UI 框架

* `UIManager` 提供**面板栈**与**Base/Modal** 分层；`ExclusivePanelBase` 支持**互斥组**（StartScreen 限单面板）。
* 面板基类 `UIPanelBase`：统一显隐（`CanvasGroup`）、遮罩、获取焦点通知。

### 多语言

* 使用Unity Localization插件实现了简体中文与英文两种显示语言。

### 游戏流程 & 解耦

* `GameStateManager` 作为**对局状态机**，通过**事件**驱动 UI/HUD/记录器：

  * `OnRoundStarted` / `OnTurnChanged` / `OnMoveCommitted` / `OnStepChanged` / `OnGameOver`
* `Board`/`Rules` 为**纯逻辑**，`BoardView` 仅负责渲染与点击路由；Binder 只做“数据→文案/控件绑定”。

### 游戏日志

* `GameRecorder` 订阅状态机事件，生成**会话级**日志（每步、胜负、用时、胜利线等），落地到持久目录；编辑器下镜像到 `Assets/Logs/`。

---

## 架构与模块

### Core（规则/状态/数据）

* **Board / Rules**：棋盘与胜负判定，纯数据结构；`LegalMoves()`、`GetWinningLine()`、`IsDraw()` 等。
* **GameStateManager**（状态机）：落子流程、人/机回合切换、暂停/恢复/结束；仅发布事件，不触碰 UI。
* **GameRecorder**：监听状态机事件聚合**会话日志**；实现轻量的审计与复盘基础。
* **GameLaunchService / Params**：跨场景传递模式/局数/先手/难度。

### AI

* **IAgent**：策略抽象；**MinimaxAgent**（深度加权 + 同分位置偏好）/ **RandomAgent**。
* **DifficultyManager**：统一难度入口；Quick 固定档位、Endless 阶梯调参；对外仅暴露 `GetAgent()` 与 `OnDifficultyChanged`。

### Data

* **DifficultyTableResources**：`Resources` 下 JSON 的读取/缓存/应用；对调试面板暴露 `TryApplyJsonAtRuntime()`。

### UI

* **UIManager / UIPanelBase / ExclusivePanelBase**：面板栈、模态层、互斥组管理。
* **Panels/**：具体面板（模式选择、设置、暂停、调试难度、结算、确认/信息等）的控制脚本。
* **Binder**：HUD/结果/按钮入口的轻逻辑绑定（订阅事件、更新文案、打开面板）。

### Services

* **Locator**：轻量服务定位（如 `UIManager`），减少硬依赖与查找样板。
* **BgmPlayer**：`DontDestroyOnLoad` 保持 BGM 跨场景连续播放；读取/响应主音量。

---

## 文件结构

```
Assets/Scripts
├─ Game/Core
│  ├─ Board.cs            // 棋盘数据结构与合法步
│  ├─ Rules.cs            // 胜负/和判定 & 取胜连线
│  └─ GameStateManager.cs // 对局状态机 + 事件源（含最强 Hint 触发）
│
├─ Game/AI
│  ├─ IAgent.cs
│  └─ MinimaxAgent.cs     // 深度加权、位置偏好、可配置软难度
│
├─ Game
│  ├─ DifficultyManager.cs      // 难度装配、Quick/Endless 档位/阶梯映射
│  ├─ QuickModeController.cs    // 系列赛/胜数过半/有效对局决策
│  ├─ EndlessModeController.cs  // 连胜/连败驱动的难度细粒度调整
│  └─ GameRecorder.cs           // 会话日志
│
├─ Data
│  └─ DifficultyTableResources.cs // Resources JSON 读取/热重载接口
│
├─ UI/Base
│  ├─ UIManager.cs              // 面板栈 + Base/Modal 分层
│  ├─ UIPanelBase.cs            // 统一显隐/遮罩/焦点
│  └─ ExclusivePanelBase.cs     // 互斥组（StartScreen 限单面板）
│
├─ UI/GamePlay
│  └─ BoardView.cs              // 渲染、点击到索引映射、提示展示
│
└─ UI/Panels (概述)
   // 具体面板控制逻辑（ModeSelect / Settings / Pause / DebugDifficulty / GameResult / ConfirmQuit / GameInfo 等），
   // 通过 UIManager 调度，遵循 UIPanelBase 接口，互斥/模态按需配置。
```


