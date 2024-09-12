using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using System.Collections.Generic;

namespace PenaltySystem
{
    public class DebtManager
    {
        private Dictionary<ulong, uint> debts = new Dictionary<ulong, uint>();

        public void AddDebt(UnturnedPlayer player, uint amount)
        {
            if (debts.ContainsKey(player.CSteamID.m_SteamID))
            {
                debts[player.CSteamID.m_SteamID] += amount;
            }
            else
            {
                debts.Add(player.CSteamID.m_SteamID, amount);
            }
        }

        public void ProcessExperienceUpdate(UnturnedPlayer player, uint newExperience)
        {
            if (debts.ContainsKey(player.CSteamID.m_SteamID))
            {
                uint currentDebt = debts[player.CSteamID.m_SteamID];
                uint totalExperience = newExperience;

                if (totalExperience >= currentDebt)
                {
                    // Borç tamamen ödendi
                    totalExperience -= currentDebt;
                    debts.Remove(player.CSteamID.m_SteamID);
                    UnturnedChat.Say(player, $"Borcunuz olan {currentDebt} deneyim tamamen ödendi.");
                }
                else
                {
                    // Borç kısmi olarak ödendi
                    debts[player.CSteamID.m_SteamID] = currentDebt - totalExperience;
                    UnturnedChat.Say(player, $"Borcunuza {totalExperience} ödeme yaptınız. Kalan borç: {debts[player.CSteamID.m_SteamID]}.");
                    totalExperience = 0;
                }

                player.Experience = totalExperience;
            }
        }

        public uint GetDebt(UnturnedPlayer player)
        {
            return debts.ContainsKey(player.CSteamID.m_SteamID) ? debts[player.CSteamID.m_SteamID] : 0;
        }
    }
}