# 隐狗插件 (HideDogPlugin)

## 简介

隐狗插件是为SCP:SL (SCP: Secret Laboratory) 游戏服务器开发的自定义插件。它引入了一种新的SCP角色 - "隐狗"，这是一种具有独特能力和机制的SCP-939变体。

## 主要特性

1. **自动生成**: 回合开始后自动生成隐狗。
2. **隐身能力**: 隐狗可以进入隐身状态。
3. **饥饿机制**: 隐狗需要定期击杀其他玩家，否则会受到伤害。
4. **平衡机制**: 
   - 受到或造成伤害时退出隐身状态。
   - 核弹引爆后无法再次隐身。
5. **管理员命令**: 提供手动生成隐狗的命令。

## 配置选项

插件提供多个可自定义的配置选项：

- `IsEnabled`: 是否启用插件
- `Debug`: 是否启用调试模式
- `SpawnDelay`: 隐狗生成延迟（秒）
- `MaxHealth`: 隐狗的最大生命值
- `InvisibilityCountdown`: 隐身倒计时时间（秒）
- `HungerStartTime`: 饥饿开始时间（秒）
- `HungerDamage`: 饥饿伤害值
- `MaxHumeShield`: 隐狗的护盾上限
- `MinPlayersForHiddenDog`: 生成隐狗所需的最小玩家数

## 安装说明

1. 确保服务器已安装EXILED框架。
2. 下载最新版本的HideDogPlugin.dll。
3. 将HideDogPlugin.dll放入服务器的`EXILED/Plugins`文件夹。
4. 重启服务器或重新加载插件。

## 使用方法

插件会在满足条件时自动生成隐狗。管理员也可以使用以下命令：

- `/spawnhiddendog` 或 `/shd`: 随机选择观察者变为隐狗
- `/spawnhiddendog <玩家ID>` 或 `/shd <玩家ID>`: 将指定玩家变为隐狗

## 权限

使用管理员命令需要`hiddendog.spawn`权限。

## 注意事项

- 确保配置合理，以保持游戏平衡。
- 建议在测试服务器上充分测试后再在正式服务器使用。
- 定期检查更新以获得bug修复和新功能。

## 支持与反馈

如遇问题或有改进建议，请通过以下方式联系：

- 在GitHub上提交Issue

## 版权信息

本插件由薄冰开发。版权所有 © 2024。
