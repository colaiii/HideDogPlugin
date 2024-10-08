using Exiled.API.Features;
using Exiled.Events.EventArgs;
using System.Collections.Generic;
using PlayerRoles;
using Exiled.API.Enums;
using System;
using MapGeneration;
using MEC;
using System.Linq;
using Exiled.API.Interfaces;
using System.ComponentModel;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Server;
using Exiled.Permissions.Extensions;
using CommandSystem;

namespace HideDogPlugin
{
    /// <summary>
    /// 插件配置类
    /// </summary>
    public class Config : IConfig
    {
        [Description("是否启用插件")]
        public bool IsEnabled { get; set; } = true;

        [Description("是否启用调试模式")]
        public bool Debug { get; set; } = false;

        [Description("隐狗生成延迟（秒）")]
        public float SpawnDelay { get; set; } = 480f;

        [Description("隐狗的最大生命值")]
        public float MaxHealth { get; set; } = 700f;

        [Description("隐身倒计时时间（秒）")]
        public int InvisibilityCountdown { get; set; } = 10;

        [Description("饥饿开始时间（秒）")]
        public int HungerStartTime { get; set; } = 180;

        [Description("饥饿伤害值")]
        public float HungerDamage { get; set; } = 2f;

        [Description("隐狗的护盾上限")]
        public float MaxHumeShield { get; set; } = 0f;

        [Description("生成隐狗所需的最小玩家数")]
        public int MinPlayersForHiddenDog { get; set; } = 10;
    }

    /// <summary>
    /// 隐狗插件主类
    /// </summary>
    public class HiddenDogPlugin : Plugin<Config>
    {
        // 插件基本信息
        public override string Name => "HidenDog Plugin";
        public override string Author => "薄冰";
        public override Version Version => new Version(1, 0, 0);

        // 单例模式
        public static HiddenDogPlugin Instance { get; private set; }

        // 当前隐狗实例
        private HiddenDog currentHiddenDog;

        /// <summary>
        /// 插件启用时调用
        /// </summary>
        public override void OnEnabled()
        {
            Instance = this;
            base.OnEnabled();
            RegisterEvents();
            Log.Info($"=======================================");
            Log.Info($"        {Name} v{Version} 已成功加载");
            Log.Info($"            作者: {Author}");
            Log.Info($"=======================================");
        }

        /// <summary>
        /// 插件禁用时调用
        /// </summary>
        public override void OnDisabled()
        {
            base.OnDisabled();
            UnregisterEvents();
            Instance = null;
            Log.Info($"=======================================");
            Log.Info($"        {Name} v{Version} 已成功禁用");
            Log.Info($"=======================================");
        }

        /// <summary>
        /// 注册事件
        /// </summary>
        private void RegisterEvents()
        {
            Exiled.Events.Handlers.Server.RoundStarted += OnRoundStarted;
            Exiled.Events.Handlers.Player.Died += OnPlayerDied;
            Exiled.Events.Handlers.Player.ChangingRole += OnChangingRole;
            Exiled.Events.Handlers.Player.Hurting += OnPlayerHurting;
            Exiled.Events.Handlers.Warhead.Detonated += OnWarheadDetonated;
            Exiled.Events.Handlers.Player.Dying += OnPlayerDying;
            Exiled.Events.Handlers.Server.RoundEnded += OnRoundEnded;
        }

        /// <summary>
        /// 注销事件
        /// </summary>
        private void UnregisterEvents()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= OnRoundStarted;
            Exiled.Events.Handlers.Player.Died -= OnPlayerDied;
            Exiled.Events.Handlers.Player.ChangingRole -= OnChangingRole;
            Exiled.Events.Handlers.Player.Hurting -= OnPlayerHurting;
            Exiled.Events.Handlers.Warhead.Detonated -= OnWarheadDetonated;
            Exiled.Events.Handlers.Player.Dying -= OnPlayerDying;
            Exiled.Events.Handlers.Server.RoundEnded -= OnRoundEnded;
        }

        /// <summary>
        /// 回合开始事件处理
        /// </summary>
        private void OnRoundStarted()
        {
            Timing.CallDelayed(Config.SpawnDelay, () => SpawnHiddenDog());
        }

        /// <summary>
        /// 回合结束事件处理
        /// </summary>
        private void OnRoundEnded(RoundEndedEventArgs ev)
        {
            CleanupHiddenDog();
        }

        /// <summary>
        /// 生成隐狗
        /// </summary>
        public void SpawnHiddenDog()
        {
            if (currentHiddenDog != null || Player.List.Count() < Config.MinPlayersForHiddenDog)
            {
                Log.Info($"当前玩家数 {Player.List.Count()} 不满足生成隐狗的最小要求 {Config.MinPlayersForHiddenDog}");
                return;
            }

            Player dog = Player.Get(RoleTypeId.Spectator).OrderBy(x => Guid.NewGuid()).FirstOrDefault();
            if (dog == null)
            {
                Log.Warn("无法生成隐狗：没有可用的观察者");
                return;
            }

            SpawnHiddenDog(dog);
        }

        /// <summary>
        /// 生成隐狗（指定玩家）
        /// </summary>
        public void SpawnHiddenDog(Player dog)
        {
            dog.Role.Set(RoleTypeId.Scp939);
            dog.Health = Config.MaxHealth;
            dog.MaxHealth = Config.MaxHealth;  // 设置最大生命值

            Room spawnRoom = Room.Get(RoomType.Hcz049) ?? Room.Get(RoomType.Surface);
            dog.Position = spawnRoom.Position + new UnityEngine.Vector3(UnityEngine.Random.Range(-3f, 3f), 1f, UnityEngine.Random.Range(-3f, 3f));

            currentHiddenDog = new HiddenDog(dog, Config);
            Log.Info($"隐狗已生成: {dog.Nickname}");

            dog.ShowHint($"你已成为隐狗！\n- {Config.InvisibilityCountdown}秒后自动进入隐身状态\n- 造成或受到伤害时退出隐身\n- {Config.HungerStartTime / 60}分钟未击杀敌人将开始受到伤害\n- 核弹爆炸后无法再次隐身", 15f);
            Map.Broadcast(5, "<color=red>警告：隐狗已经出现！</color>", Broadcast.BroadcastFlags.Normal, true);

            Timing.CallDelayed(Config.InvisibilityCountdown, () => currentHiddenDog.EnableInvisibility());
        }

        /// <summary>
        /// 清理隐狗
        /// </summary>
        private void CleanupHiddenDog()
        {
            currentHiddenDog?.Disable();
            currentHiddenDog = null;
        }

        /// <summary>
        /// 玩家死亡事件处理
        /// </summary>
        private void OnPlayerDied(DiedEventArgs ev)
        {
            if (currentHiddenDog?.Player == ev.Player)
            {
                CleanupHiddenDog();
            }
        }

        /// <summary>
        /// 玩家角色变更事件处理
        /// </summary>
        private void OnChangingRole(ChangingRoleEventArgs ev)
        {
            if (currentHiddenDog?.Player == ev.Player && ev.NewRole != RoleTypeId.Scp939)
            {
                CleanupHiddenDog();
            }
        }

        /// <summary>
        /// 玩家受伤事件处理
        /// </summary>
        private void OnPlayerHurting(HurtingEventArgs ev)
        {
            if (currentHiddenDog?.Player != null && (ev.Player == currentHiddenDog.Player || ev.Attacker == currentHiddenDog.Player))
            {
                currentHiddenDog.DisableInvisibility();
            }
        }

        /// <summary>
        /// 核弹引爆事件处理
        /// </summary>
        private void OnWarheadDetonated()
        {
            currentHiddenDog?.DisableInvisibility();
            currentHiddenDog?.PreventInvisibility();
        }

        /// <summary>
        /// 玩家濒死事件处理
        /// </summary>
        private void OnPlayerDying(DyingEventArgs ev)
        {
            if (currentHiddenDog?.Player == ev.Attacker)
            {
                currentHiddenDog.OnKill();
            }
        }
    }

    /// <summary>
    /// 隐狗类，管理隐狗的行为和状态
    /// </summary>
    public class HiddenDog
    {
        public Player Player { get; private set; }
        private bool isInvisible = false;
        private bool canBecomeInvisible = true;
        private DateTime lastKillTime;
        private CoroutineHandle hungerCoroutine;
        private CoroutineHandle hintCoroutine;
        private Config config;
        private DateTime invisibilityStartTime;
        private CoroutineHandle shieldResetCoroutine;

        /// <summary>
        /// 构造函数
        /// </summary>
        public HiddenDog(Player player, Config config)
        {
            Player = player;
            this.config = config;
            lastKillTime = DateTime.UtcNow;
            hungerCoroutine = Timing.RunCoroutine(HungerCheck());
            hintCoroutine = Timing.RunCoroutine(HintManager());
            invisibilityStartTime = DateTime.UtcNow;
            shieldResetCoroutine = Timing.RunCoroutine(ShieldResetLoop());
        }

        /// <summary>
        /// 启用隐身
        /// </summary>
        public void EnableInvisibility()
        {
            if (!canBecomeInvisible || isInvisible || Player == null) return;
            isInvisible = true;
            invisibilityStartTime = DateTime.UtcNow;
            Player.EnableEffect(EffectType.Invisible);
            Log.Info($"隐狗 {Player.Nickname} 进入隐身状态");
        }

        /// <summary>
        /// 禁用隐身
        /// </summary>
        public void DisableInvisibility()
        {
            if (!isInvisible || Player == null) return;
            isInvisible = false;
            Player.DisableEffect(EffectType.Invisible);
            Log.Info($"隐狗 {Player.Nickname} 退出隐身状态");
            invisibilityStartTime = DateTime.UtcNow;
        }

        /// <summary>
        /// 阻止隐身
        /// </summary>
        public void PreventInvisibility()
        {
            canBecomeInvisible = false;
            DisableInvisibility();
            Log.Info($"隐狗 {Player.Nickname} 无法再次进入隐身状态");
        }

        /// <summary>
        /// 处理击杀事件
        /// </summary>
        public void OnKill()
        {
            lastKillTime = DateTime.UtcNow;
        }

        /// <summary>
        /// 饥饿检查协程
        /// </summary>
        private IEnumerator<float> HungerCheck()
        {
            while (Player != null)
            {
                yield return Timing.WaitForSeconds(1f);
                if (Player == null) break;

                TimeSpan timeSinceLastKill = DateTime.UtcNow - lastKillTime;
                if (timeSinceLastKill.TotalSeconds > config.HungerStartTime && Player.Health > config.HungerDamage)
                {
                    Player.Health -= config.HungerDamage;
                }
            }
        }

        /// <summary>
        /// 生成提示信息
        /// </summary>
        private string GenerateHintMessage()
        {
            TimeSpan timeSinceLastKill = DateTime.UtcNow - lastKillTime;
            int hungerCountdown = Math.Max(0, config.HungerStartTime - (int)timeSinceLastKill.TotalSeconds);

            string invisibilityStatus = isInvisible ? "已进入隐身" :
                (DateTime.UtcNow - invisibilityStartTime).TotalSeconds < config.InvisibilityCountdown ?
                $"进入隐身：{config.InvisibilityCountdown - (int)(DateTime.UtcNow - invisibilityStartTime).TotalSeconds}秒" : "";

            string hungerStatus = hungerCountdown <= 0 ? "你感到饥饿" : $"饥饿倒计时：{hungerCountdown}秒";

            return string.IsNullOrEmpty(invisibilityStatus) ? hungerStatus : $"{invisibilityStatus} | {hungerStatus}";
        }

        /// <summary>
        /// 提示管理协程
        /// </summary>
        private IEnumerator<float> HintManager()
        {
            while (Player != null)
            {
                Player.ShowHint(GenerateHintMessage(), 1f);
                yield return Timing.WaitForSeconds(1f);
            }
        }

        /// <summary>
        /// 护盾重置循环协程
        /// </summary>
        private IEnumerator<float> ShieldResetLoop()
        {
            while (Player != null)
            {
                Player.HumeShield = 0f;
                yield return Timing.WaitForSeconds(0.1f);
            }
        }

        /// <summary>
        /// 禁用隐狗
        /// </summary>
        public void Disable()
        {
            DisableInvisibility();
            Timing.KillCoroutines(hungerCoroutine, hintCoroutine, shieldResetCoroutine);
            Player = null;
        }
    }

    /// <summary>
    /// 管理员命令：生成隐狗
    /// </summary>
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class SpawnHiddenDogCommand : ICommand
    {
        public string Command => "spawnhiddendog";
        public string[] Aliases => new[] { "shd" };
        public string Description => "生成一个隐狗或将指定玩家变成隐狗";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermission("hiddendog.spawn"))
            {
                response = "你没有权限使用此命令。";
                return false;
            }

            if (arguments.Count == 0)
            {
                HiddenDogPlugin.Instance.SpawnHiddenDog();
                response = "已尝试生成隐狗。";
                return true;
            }

            Player target = Player.Get(arguments.At(0));
            if (target == null)
            {
                response = $"找不到指定的玩家。";
                return false;
            }

            HiddenDogPlugin.Instance.SpawnHiddenDog(target);
            response = $"已将玩家 {target.Nickname} (ID: {target.Id}) 变成隐狗。";
            return true;
        }
    }
}