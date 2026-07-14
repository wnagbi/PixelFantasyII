# Pixel Fantasy II

## 项目简介

`Pixel Fantasy II` 是一款基于 Unity 开发的 2D 像素风动作生存类项目。玩家在关卡中通过移动、攻击、击杀敌人、获取经验、升级武器和释放技能来推进战斗流程。

本项目当前主要作为学习与求职展示 Demo，重点展示单机核心玩法、xLua 玩法逻辑热更新、Addressables 资源热更新、JSON 本地数据存储以及启动热更新加载流程。

## 项目特色

- 像素风角色、敌人与道具表现。
- 武器攻击、升级、冷却和数量成长。
- 技能解锁、技能释放、冷却显示与特效生成。
- 敌人刷怪、追踪、受伤、死亡与掉落流程。
- 分数、技能解锁、设置项等本地 JSON 存储。
- 启动场景加载条展示脚本更新、资源检查和资源下载进度。

## 技术亮点

- **xLua 玩法逻辑热更新**  
  使用 C# 作为 Unity 外壳，Lua 接管武器、技能、敌人、掉落、刷怪、玩家数值等可变规则。

- **Lua Zip 服务器热更新流程**  
  启动时从本地 HTTP 服务器请求 Lua manifest，对比版本后下载 zip，校验大小和 SHA256，再解压到 `persistentDataPath/LuaHotfix`。

- **Addressables HTTP 资源热更新**  
  使用 Addressables 远端 Catalog 和 Bundle，通过本地 HTTP 服务模拟商业化资源热更新流程。

- **Prefab / Sprite / VFX 资源热更新**  
  技能图标、技能特效、武器投射物、敌人和掉落物 Prefab 可通过 Addressables key 加载更新，加载失败时回退 Inspector 原引用。

- **Start 场景启动加载流程**  
  `start.unity` 作为首场景，展示热更新进度并在完成后进入 `title.unity`。

- **JSON 本地数据存储**  
  使用 `Newtonsoft.Json` 保存游戏设置、分数和技能解锁状态，替代部分旧 PlayerPrefs 数据。

- **对象池与 Addressables Prefab 替换**  
  对敌人、掉落物等高频对象保留对象池生成流程，同时支持通过 Addressables 替换池内 Prefab。

- **C# Host + Lua Rule 架构**  
  C# 保留 MonoBehaviour 生命周期、Inspector 引用、对象池、动画、物理、UI 和资源加载；Lua 负责可变玩法规则。

- **热更新失败 fallback**  
  Lua、Addressables 或服务器不可用时不会阻塞游戏启动，系统会继续使用本地旧版本或包内默认资源。

## 热更新系统

当前启动流程：

```text
start
  -> 检查 Lua Zip 热更新
  -> 初始化 xLua
  -> 检查 Addressables Catalog
  -> 下载 gameplay 资源
  -> 进入 title
```

热更新分工：

```text
C#             Unity 生命周期、资源加载、对象池、UI、物理、动画
Lua            玩法规则、数值成长、武器逻辑、敌人规则、掉落规则
Addressables   Prefab、Sprite、VFX、资源引用
JSON           本地设置、分数、技能解锁状态
```

## 项目结构

```text
Assets/
  Lua/                         Lua 配置与玩法规则源码
  Scripts/
    Hotfix/                    xLua 热更新基础设施与 C# Host
    Controller/                游戏流程、UI、设置、存档、启动加载
    ObjPool/                   对象池
    Weapon/                    武器表现与控制器
    PickUp/                    掉落物逻辑
  Scenes/
    start.unity                启动加载场景
    title.unity                标题与主菜单场景
    level1*.unity              关卡场景
  StreamingAssets/
    Lua/                       包内默认 Lua 文件
  AddressableAssetsData/       Addressables 配置

AddressablesRemote/            本地资源热更新输出目录示例
Packages/                      Unity Package 配置
ProjectSettings/               Unity 项目设置
```

## 运行环境

- Unity `2022.3.55f1`
- Visual Studio 2022
- Windows
- xLua
- Addressables
- Newtonsoft.Json
- DOTween
- Unity Input System
- Unity Localization

## 启动与测试方式

1. 使用 Unity `2022.3.55f1` 打开项目。
2. 确认 Build Settings 第一场景为：

```text
Assets/Scenes/start.unity
```

3. 启动本地热更新 HTTP 服务：

```powershell
python D:\AddressablesServerRoot\start_addressables_server.py
```

4. 在 Unity 中 Play，或打包后运行客户端。
5. 游戏启动后会先进入 `start` 加载场景，完成 Lua 与 Addressables 检查后进入 `title`。

## 本地热更新测试

本项目使用本地 HTTP 服务器模拟远端热更新服务器。

服务器根目录：

```text
D:\AddressablesServerRoot
```

Addressables 资源测试地址示例：

```text
http://127.0.0.1:18080/AddressablesRemote/StandaloneWindows64/catalog_1.0.hash
```

Lua 热更新 manifest 地址示例：

```text
http://127.0.0.1:18080/LuaRemote/lua_manifest.json
```

测试思路：

- 修改 Lua 文件后重新构建 Lua 热更新 zip，不重新打包客户端，重启游戏验证玩法逻辑变化。
- 修改 Addressable Prefab、Sprite 或 VFX 后重新 Build Addressables，不重新打包客户端，重启游戏验证资源变化。
- 关闭服务器后启动游戏，验证 fallback 是否正常进入游戏。

## 参与人员

- **王正瀚**：程序开发、系统架构、热更新改造、存档系统、资源加载、玩法逻辑接入。
- **魏涛**：美术资源。

## 备注

- 本项目当前重点为单机核心玩法与热更新系统展示。
- 项目中保留的 Mirror / 联机相关内容不是当前主线展示内容。
- 本地 HTTP 服务器用于模拟商业化热更新流程，不代表已经接入正式线上服务器或 CDN。
- Addressables 可以热更新资源和 Prefab 序列化数据，但不能热更新客户端不存在的 C# 代码。
- C# 函数逻辑如需热更新，需要提前设计为 C# Host 调 Lua Rule 的形式。
