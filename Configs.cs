using System.ComponentModel;
using Exiled.API.Interfaces;

namespace HideDogPlugin
{
    public class Configs : IConfig
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

        [Description("饥饿警告时间（秒）")]
        public int HungerWarningTime { get; set; } = 150;

        [Description("饥饿伤害值")]
        public float HungerDamage { get; set; } = 2f;

        [Description("隐狗生成提示持续时间（秒）")]
        public float SpawnHintDuration { get; set; } = 15f;

        [Description("隐狗生成广播持续时间（秒）")]
        public ushort SpawnBroadcastDuration { get; set; } = 5;
    }
}