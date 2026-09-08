# TinySword — Codex 项目开发规则

## 1. 项目背景

TinySword 是一个已有完整玩法链路的 Unity 2D 俯视角 ARPG，目前正在进行 AI-assisted redevelopment（AI 辅助二次开发）。

项目环境：

* Unity：`6000.0.47f1`
* Language：C#
* UI：uGUI
* Input：Legacy Input Manager
* 主场景：

  * `Assets/Scenes/StartScene.unity`
  * `Assets/Scenes/GameScene.unity`
* 第一方业务脚本：

  * `Assets/Scripts`

当前项目已经存在可玩的稳定基线。

目标不是从零重写游戏，而是在每一步保持项目可运行的前提下，逐步提升：

* Correctness
* Maintainability
* Extensibility
* Testability
* Performance
* Engineering quality

---

## 2. 沟通语言

* 默认使用中文与用户沟通。
* Unity、C#、Git 和软件工程中的常见技术术语可以保留英文。
* 类名、方法名、文件路径、命令和代码标识符保持原样。
* 不要为了使用英文而降低解释的清晰度。

---

## 3. 修改权限

除非用户已经明确批准实施，否则保持在 Analysis / Planning 阶段。

用户要求：

* 分析
* 审计
* Review
* 解释
* 制定方案
* 查找问题

不代表允许修改文件。

开始修改代码之前，必须先：

1. 阅读相关代码和配置。
2. 说明 Current Behavior（当前行为）。
3. 提出最小可行修改方案。
4. 列出预计修改的文件。
5. 列出明确不需要修改的文件。
6. 分析 Unity Serialization、Lifecycle 和 Regression Risk。
7. 给出 Acceptance Criteria（验收标准）。
8. 等待用户明确批准。

未经用户批准，不得修改代码或项目文件。

---

## 4. Scope Control

所有改动都应尽可能小，并能够作为独立 Commit / Pull Request 进行 Review。

禁止在一个任务中顺手进行无关重构。

不要把以下操作随意混入同一个改动：

* Bug fix
* Architecture refactor
* 类型重命名
* 资源移动
* Scene 重构
* Prefab 重构
* Package 升级
* 全项目格式化

如果在当前任务之外发现其他问题：

1. 单独报告。
2. 说明严重程度和影响。
3. 不自动修复。
4. 除非该问题阻塞当前已批准任务。

---

## 5. Unity 文件安全

除非当前任务明确要求并且用户批准，否则不要修改：

* Scene
* Prefab
* `.meta`
* ProjectSettings
* Packages
* Animator Controller
* Animation Clip
* Animation Event

不要修改 Unity 自动生成目录：

* `Library/`
* `Temp/`
* `Logs/`
* `obj/`
* `.vs/`

必须尽量保留现有 Serialized Reference。

当前项目大量依赖 Inspector、Scene Reference 和 Prefab Instance Override，因此不能假设只修改基础 Prefab 就能安全完成迁移。

---

## 6. 第三方资源

默认禁止修改以下第三方资源目录：

* `Assets/Tiny Swords`
* `Assets/Tiny Swords (Enemy Pack)`
* `Assets/Pixel Art/PixelArtRPGVFXLite`

可以分析第一方代码如何引用这些资源，但不要修改第三方资源本身，除非用户明确要求。

---

## 7. 当前架构约束

以下内容描述的是 CURRENT Architecture，而不是最终目标。

### GameManger

`GameManger` 当前是 `DontDestroyOnLoad` Singleton，同时负责：

* Scene transition
* Audio
* Coin state
* Save / Load
* 部分全局生命周期逻辑

不要在无关任务中顺手拆分或重命名它。

### Player

`Player` 当前同时包含：

* Input
* Movement
* Normal Attack
* Combo
* Guard
* Skill
* Cooldown
* Animation
* HP / Damage
* Death
* UI interaction

不要因为职责较多就自动进行大规模拆分类重构。

### EnemyBase

`EnemyBase` 当前实现：

* idle
* walk
* pursuit
* attack
* getHit
* dead

并同时负责：

* Movement
* Combat
* Health
* Animation
* Death
* Drop

在修复局部 FSM Bug 时，应优先保留现有 Prefab 和 Serialized Field。

---

## 8. 现有命名兼容性

以下名称虽然存在拼写问题，但已经是现有代码和 Unity 引用的一部分：

* `GameManger`
* `CanvasManger`
* `AttackPerfab`
* `EnemypursuitColider`

禁止把它们作为“顺手清理”进行重命名。

如果以后需要规范命名，必须作为独立 Migration Task 处理。

---

## 9. Animation / Animator 兼容性

Animation Event 是当前 Gameplay 调用链的一部分。

重要 Animation Event Receiver 包括：

* `Player.Attack1`
* `Player.Skill`
* `PlayerSkill.Skill1`
* `PlayerSkill.Skill2`
* `PlayerSkill.Skill3`
* `EnemyBase.Attack1`

未经明确迁移计划，不要删除或重命名这些方法。

当前 Animator Parameter 包括例如：

Player：

* `IsRun`
* `Attack1`
* `Attack2`
* `IsGuard`
* `GetHit`
* `Dead`
* `Skill`

Enemy：

* `IsRun`
* `Attack1`
* `GetHit`
* `Dead`

不要在无关修改中重命名这些参数。

---

## 10. Gameplay 修改原则

Bug Fix：

* 优先修复已经确认的具体行为。
* 不顺便进行架构大改。
* 没有明确要求时保持现有 Gameplay Behavior。

Refactor：

* 默认要求保持外部可观察行为。
* 在实施前分析 Serialized Reference 风险。
* 优先 Incremental Refactor，不进行整体重写。

不要为了“架构看起来高级”而引入 Design Pattern。

优先选择：

* Simple
* Understandable
* Extensible
* Testable

且适合当前项目规模的方案。

避免新增不必要的全局 Singleton。

---

## 11. Lifecycle 风险

修改 Gameplay 时重点检查：

* `DontDestroyOnLoad`
* Singleton 生命周期
* Scene reload
* `Invoke` / `CancelInvoke`
* Animation Event
* Trigger Enter / Exit
* Player 注册时机
* Prefab Instance Override

不要假设一个延迟 Callback 在状态变化之后仍然有效。

不要假设 Scene 已加载就代表所有依赖 MonoBehaviour 已完成初始化。

---

## 12. Performance 原则

不要在没有测量数据的情况下声称性能提升。

例如：

* `Instantiate` / `Destroy`
* `GameObject.Find`
* Update polling
* UI Update
* Object Pool
* GC Allocation

必须区分：

1. 代码层面的潜在问题。
2. Unity Profiler 已确认的性能瓶颈。

Object Pool 应在 Gameplay 行为稳定后再实施，并尽可能保留 Before / After Profiler 数据。

---

## 13. Testing / Validation

修改 Gameplay 前应先明确 Regression Scenario。

有条件时优先使用 Unity Test Framework。

适合纯逻辑的内容优先考虑 Edit Mode Test。

涉及以下内容时通常需要 Play Mode 或实际 Unity Runtime 验证：

* MonoBehaviour lifecycle
* GameObject
* Scene
* Physics2D
* Trigger
* Animator
* Animation Event
* UI interaction

如果当前环境无法实际运行 Unity：

* 不得声称项目已经编译通过。
* 不得声称 Runtime 行为已经验证。
* 必须明确列出仍需用户手动验证的项目。

---

## 14. Git 安全

可以根据任务需要执行只读 Git 操作，例如：

* `git status`
* `git diff`
* `git log`
* `git show`
* `git branch`

未经用户明确要求，不进行：

* commit
* push
* pull
* merge
* rebase
* reset
* clean
* tag 创建或删除
* branch 创建或删除
* amend

不得丢弃用户已有修改。

不得擅自重写 Git 历史。

---

## 15. 实施后的报告格式

完成已批准修改后，必须说明：

### Changed Files

修改了哪些文件，以及为什么。

### Validation Performed

实际执行了哪些：

* Compilation
* Test
* Git diff
* Unity Runtime validation
* Console check

只能报告真正执行过的验证。

### Validation Not Performed

哪些项目仍需要：

* Unity Editor
* Play Mode
* Manual Test
* Profiler

进行验证。

### Remaining Risks

仍然存在的风险或后续工作。

---

## Definition of Done

代码写完不等于任务完成。

任务完成前，应根据实际情况确认：

* 修改范围符合批准内容。
* 没有修改无关文件。
* Serialized Reference 未被意外破坏。
* Animation Event / UnityEvent 兼容性得到保留。
* Acceptance Criteria 已检查。
* 可运行的相关测试已经执行。
* 没有在无证据情况下声称性能提升。
* 未完成的 Unity Runtime 验证已明确说明。

每个修改都应保持足够小，使其可以作为独立 Git Commit / Pull Request 被人工 Review。
