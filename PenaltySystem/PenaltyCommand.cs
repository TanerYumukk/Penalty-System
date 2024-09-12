using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SDG.Unturned;
using Rocket.Core.Logging;

namespace PenaltySystem
{
    public class PenaltyCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;
        public string Name => "PenaltySystem";
        public string Help => "Allows you to issue penalties";
        public string Syntax => "<name> <amount> <reason>";
        public List<string> Aliases => new List<string> { "penalty", "punish", "cezakes" };
        public List<string> Permissions => new List<string> { Main.Instance.Configuration.Instance.PenaltyPermission };

        public async void Execute(IRocketPlayer caller, string[] args)
        {
            UnturnedPlayer player = caller as UnturnedPlayer;
            if (args.Length < 3)
            {
                UnturnedChat.Say(player, "Kullanım: /cezakes <oyuncuismi> <miktar> <sebep>");
                return;
            }

            string TargetPlayer = args[0];
            UnturnedPlayer targetPlayer = UnturnedPlayer.FromName(TargetPlayer);

            if (targetPlayer == null)
            {
                UnturnedChat.Say(player, "Oyuncu bulunamadı");
                return;
            }

            if (!decimal.TryParse(args[1], out decimal fineAmount) || fineAmount <= 0)
            {
                UnturnedChat.Say(player, "Geçerli bir ceza miktarı girin.");
                return;
            }

            string reason = string.Join(" ", args, 2, args.Length - 2);
            uint fineAmountUInt = (uint)Math.Floor(fineAmount);
            uint playerExp = targetPlayer.Experience;

            if (playerExp >= fineAmountUInt)
            {
                targetPlayer.Experience -= fineAmountUInt;
                UnturnedChat.Say(caller, $"{targetPlayer.DisplayName} isimli oyuncuya {fineAmountUInt} deneyim cezası verdiniz. Sebep: {reason}");
                UnturnedChat.Say(targetPlayer, $"{caller.DisplayName} tarafından {fineAmountUInt} deneyim cezası aldınız. Sebep: {reason}");
                SendWebhook(caller.DisplayName, targetPlayer.DisplayName, fineAmountUInt, reason);
            }
            else
            {
                Main.Instance.debtManager.AddDebt(targetPlayer, fineAmountUInt - playerExp);
                targetPlayer.Experience = 0;
                UnturnedChat.Say(caller, $"{targetPlayer.DisplayName} isimli oyuncunun ceza miktarını karşılayacak yeterli deneyimi yok. {fineAmountUInt - playerExp} deneyim borcu kaydedildi. Sebep: {reason}");
                UnturnedChat.Say(targetPlayer, $"{fineAmountUInt} deneyim cezası aldınız ama yeterli deneyiminiz yok. {fineAmountUInt - playerExp} deneyim borcunuz oluştu. Sebep: {reason}");
                SendWebhook(caller.DisplayName, targetPlayer.DisplayName, fineAmountUInt, reason);
            }
        }

        private void SendWebhook(string issuer, string target, uint amount, string reason)
        {
            string serverName = Provider.serverName;
            var embed = new
            {
                embeds = new[]
                {
            new
            {
                title = "Ceza Kesme İşlemi",
                description = $"**Sunucu:** {serverName}\n**Ceza Kesen:** {issuer}\n**Ceza Alan:** {target}\n**Miktar:** {amount}\n**Sebep:** {reason}",
                color = 7506394
            }
        }
            };

            Task.Run(() =>
            {
                try
                {
                    var jsonPayload = JsonConvert.SerializeObject(embed);
                    var request = (HttpWebRequest)WebRequest.Create(Main.Instance.Configuration.Instance.Webhook);
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

    }
}
