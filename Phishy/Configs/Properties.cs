using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Phishy.Utils;
using YamlDotNet.Serialization;

namespace Phishy.Configs
{
    public sealed class Properties
    {
        [YamlMember()]
        public string KeyboardKeyStartFishing { get; set; }
        public string KeyboardKeyApplyLure { get; set; }
        public int LureBuffDurationMinutes { get; set; }
        public int FishingChannelDurationSeconds { get; set; }
        public string GameWindowName { get; set; }

        public Properties()
        {
            KeyboardKeyStartFishing = KeyboardUtils.ConvertToString(Keys.D1);
            KeyboardKeyApplyLure = KeyboardUtils.ConvertToString(Keys.D2);
            LureBuffDurationMinutes = TimeSpan.FromMinutes(10).Minutes;
            FishingChannelDurationSeconds = TimeSpan.FromSeconds(20).Seconds;
            GameWindowName = "Game Window Name";
        }
    }
}
