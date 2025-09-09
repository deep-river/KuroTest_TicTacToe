# 井字棋游戏

 使用Unity 实现的井字棋游戏，项目中主要展示了**可维护的代码架构、多档难度的游戏 AI、数据驱动且支持热重载的游戏难度配置与规范化的 UI 框架**。

![Image text](https://raw.githubusercontent.com/deep-river/OnlinePortfolio/refs/heads/main/img/TTT-Screenshot-MainMenu.jpg)

![Image text](https://raw.githubusercontent.com/deep-river/OnlinePortfolio/refs/heads/main/img/TTT-Screenshot-GameScene.jpg)

## 玩法与模式

**井字棋规则**：3×3 棋盘，X/O 轮流落子；横/竖/斜任一连成 3 子即胜，否则棋满为和。
* **快速模式**：选择难度与局数，按过半局数获胜的规则判定胜负或平局。
* **无尽模式**：从简单难度起步，依据玩家的连胜/连败记录动态微调 AI 难度。

---

## 功能特性

### 游戏 AI

* 基于Minimax算法实现的游戏AI ，采用**深度加权评分**策略（快赢优先/慢输次之）。
* 分数相同时对多个落点采用**位置偏好**加权（中心＞角＞边），以获得理论最佳落点。
* 加入难度软化参数`mistakeRate`、`randomizeAmongBest`，用于模拟人类的失误情况，以调节游戏难度。

### 数据驱动的难度配置

* 难度配置表存储为 **Resources/Config/difficulty.json**；在运行时载入，可在游戏运行中通过调试面板实现**热重载**。
* 在**快速游戏** 采用简单/普通/困难三档固定档位；**Endless** 将阶梯参数映射至三档显示名，细粒度控制 `mistakeRate` 等。

### UI 框架

* `UIManager` 采用了栈的结构管理UI层级，并使用缓存字典管理UI窗口的实例，未来可扩展为配合对象池实现更高性能的UI管理。
* 在场景中将游戏固定UI和弹出面板通过 BaseCanvas 和 ModalCanvas 分层，以实现层级交互隔离，且避免高频 Canvas 重绘。
* UI面板基类 `UIPanelBase` 支持 `CanvasGroup` 统一显隐控制、遮罩控制、获取焦点通知等扩展功能。
* `ExclusivePanelBase` 类型的UI面板支持**互斥组**的逻辑，保证同时只显示一个互斥的面板。

### 多语言

* 使用Unity Localization插件实现了简体中文与英文两种显示语言。

### 游戏日志

* 通过 `GameRecorder` 订阅状态机事件，在游戏结束时生成详细的对局日志，包括回合数、步数、胜负、用时等，并保存到游戏目录实现持久化。

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

* **Locator**：轻量服务定位器，用于单例服务/管理器的查找与调用（如 `UIManager`），减少硬依赖与查找样板。
* **BgmPlayer**：`DontDestroyOnLoad` 保持 BGM 跨场景连续播放；读取/响应主音量。

---

## 文件结构

```
Assets/Scripts
├─ Game/Core
│  ├─ Board.cs            // 棋盘数据结构与核心行为
│  ├─ Rules.cs            // 游戏规则及判定方法
│  └─ GameStateManager.cs // 游戏流程、对局状态机管理器
│
├─ Game/AI
│  ├─ IAgent.cs           // AI行为接口
│  └─ MinimaxAgent.cs     // Minimax算法配置
│
├─ Game
│  ├─ DifficultyManager.cs      // 游戏难度配置管理器
│  ├─ QuickModeController.cs    // 快速游戏模式流程管理
│  ├─ EndlessModeController.cs  // 无尽模式游戏流程管理
│  └─ GameRecorder.cs           // 会话日志记录器
│
├─ Data
│  └─ DifficultyTableResources.cs // JSON格式数据表的读写模块
│
├─ UI/Base
│  ├─ UIManager.cs              // UI主管理器
│  ├─ UIPanelBase.cs            // UI弹出面板基类
│  └─ ExclusivePanelBase.cs     // 互斥类型的UI面板基类
│
├─ UI/GamePlay
│  └─ BoardView.cs              // 棋盘的表示层管理器
│
└─ UI/Panels (概述)
   // 具体面板控制逻辑（ModeSelect / Settings / Pause / DebugDifficulty / GameResult / ConfirmQuit / GameInfo 等），
   // 通过 UIManager 调度，遵循 UIPanelBase 接口，互斥/模态按需配置。
```


