using Rocket.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PenaltySystem
{
    public class PenaltyConfig : IRocketPluginConfiguration
    {
        public string Webhook;
        public string PenaltyPermission;
        public void LoadDefaults()
        {
            Webhook = "https://discord.com/api/webhooks/1281413552611262494/4LqK5EddPuAJXR_-xovPbYY9xtAYPMX9ThmSX0HrOXgwBf6nJ1xBoVSaja5RVKlZntG_";
            PenaltyPermission = "Penalty";
        }
    }
}