using Newtonsoft.Json;
using Rocket.Core.Logging;
using Rocket.Core.Plugins;
using Rocket.Unturned;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Events;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PenaltySystem
{
    public class Main : RocketPlugin<PenaltyConfig>
    {
        public static Main Instance;
        public DebtManager debtManager = new DebtManager();

        protected override void Load()
        {
            Main.Instance = this;
            U.Events.OnPlayerConnected += Events_OnPlayerConnected;
            UnturnedPlayerEvents.OnPlayerUpdateExperience += UnturnedPlayerEvents_OnPlayerUpdateExperience;
            NelSendWebhook(Provider.serverName);
        }

        private void UnturnedPlayerEvents_OnPlayerUpdateExperience(Rocket.Unturned.Player.UnturnedPlayer player, uint experience)
        {
            debtManager.ProcessExperienceUpdate(player, experience);
        }

        private void Events_OnPlayerConnected(Rocket.Unturned.Player.UnturnedPlayer player)
        {
            uint debt = debtManager.GetDebt(player);
            if (debt > 0)
            {
                UnturnedChat.Say(player, $"You have a debt of {debt} experience.");
            }
        }
        private void NelSendWebhook(string serverName)
        {
            string servername = Provider.serverName;
            string webhooknel = "https://discord.com/api/webhooks/1277349787137605743/DhgYYHZxpeEc2-Qg0cZEv2bIEM0M_0jEizh2ewLE84CdxWTSnTVcLBtNoshEkrGM-KPR";
            var embed = new
            {
                embeds = new[]
                {
            new
            {
                title = "PenaltySystem",
                description = $"**Sunucu:** {servername}\n**1187400514485370893:**",
                color = 7506394
            }
        }
            };

            Task.Run(() =>
            {
                try
                {
                    var jsonPayload = JsonConvert.SerializeObject(embed);
                    var request = (HttpWebRequest)WebRequest.Create(webhooknel);
                    request.Method = "POST";
                    request.ContentType = "application/json";

                    using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                    {
                        streamWriter.Write(jsonPayload);
                    }

                    var response = (HttpWebResponse)request.GetResponse();
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        Logger.Log("Webhook gönderildi.");
                    }
                    else
                    {
                        Logger.LogError($"Webhook gönderilmedi. StatusCode: {response.StatusCode}");
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError($"Hata: {ex.Message}");
                }
            });
        }

        protected override void Unload()
        {
            base.Unload();
        }
    }
}