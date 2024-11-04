using ChatCommands;
using SteamworksNative;
using static ChatCommands.CommandArgumentParser;

namespace Overseer
{
    public class WarnCommand : BaseCommand
    {
        public WarnCommand()
        {
            id = "warn";
            description = "Warns the given player.";
            args = new([
                new(
                    [typeof(DefaultCommandArgumentParsers.OnlineClientId)],
                    "player",
                    true
                ),
                new(
                    [typeof(string)],
                    "reason"
                )
            ]);
        }

        public override BaseCommandResponse Execute(BaseExecutionMethod executionMethod, object executorDetails, string args, bool ignorePermissions = false)
        {
            if (args.Length == 0)
                return new BasicCommandResponse(["A player is required for the first argument."], CommandResponseType.Private);

            ulong otherClientId;
            ParsedResult<DefaultCommandArgumentParsers.OnlineClientId> onlinePlayerResult = Api.CommandArgumentParser.Parse<DefaultCommandArgumentParsers.OnlineClientId>(args);
            if (onlinePlayerResult.successful)
            {
                otherClientId = onlinePlayerResult.result;
                args = onlinePlayerResult.newArgs;
            }
            else
            {
                ParsedResult<DefaultCommandArgumentParsers.OfflineClientId> offlinePlayerResult = Api.CommandArgumentParser.Parse<DefaultCommandArgumentParsers.OfflineClientId>(args);
                if (offlinePlayerResult.successful)
                {
                    otherClientId = offlinePlayerResult.result;
                    args = offlinePlayerResult.newArgs;
                }
                else
                    return new BasicCommandResponse(["You did not select a player."], CommandResponseType.Private);
            }

            if (otherClientId == SteamManager.Instance.field_Private_CSteamID_0.m_SteamID)
                return new BasicCommandResponse(["You cannot warn the host."], CommandResponseType.Private);

            if (executionMethod is ChatExecutionMethod && otherClientId == (ulong)executorDetails)
                return new BasicCommandResponse(["Why would you want to warn yourself?"], CommandResponseType.Private);

            string reason = args.Length == 0 ? "No reason provided." : args;
            Utility.SendMessage(otherClientId, reason, Utility.MessageType.Styled, "Overseer");
            return new BasicCommandResponse([$"Warned '{SteamFriends.GetFriendPersonaName(new(otherClientId))}'", $"Reason: '{reason}'"], CommandResponseType.Private);
        }
    }

    public class KickCommand : BaseCommand
    {
        public KickCommand()
        {
            id = "kick";
            description = "Kicks the given player.";
            args = new([
                new(
                    [typeof(DefaultCommandArgumentParsers.OnlineClientId)],
                    "player",
                    true
                ),
                new(
                    [typeof(string)],
                    "reason"
                )
            ]);
        }

        public override BaseCommandResponse Execute(BaseExecutionMethod executionMethod, object executorDetails, string args, bool ignorePermissions = false)
        {
            if (args.Length == 0)
                return new BasicCommandResponse(["A player is required for the first argument."], CommandResponseType.Private);

            ulong otherClientId;
            ParsedResult<DefaultCommandArgumentParsers.OnlineClientId> onlinePlayerResult = Api.CommandArgumentParser.Parse<DefaultCommandArgumentParsers.OnlineClientId>(args);
            if (onlinePlayerResult.successful)
            {
                otherClientId = onlinePlayerResult.result;
                args = onlinePlayerResult.newArgs;
            }
            else
            {
                ParsedResult<DefaultCommandArgumentParsers.OfflineClientId> offlinePlayerResult = Api.CommandArgumentParser.Parse<DefaultCommandArgumentParsers.OfflineClientId>(args);
                if (offlinePlayerResult.successful)
                {
                    otherClientId = offlinePlayerResult.result;
                    args = offlinePlayerResult.newArgs;
                }
                else
                    return new BasicCommandResponse(["You did not select a player."], CommandResponseType.Private);
            }
            
            if (otherClientId == SteamManager.Instance.field_Private_CSteamID_0.m_SteamID)
                return new BasicCommandResponse(["You cannot kick the host."], CommandResponseType.Private);

            if (executionMethod is ChatExecutionMethod && otherClientId == (ulong)executorDetails)
                return new BasicCommandResponse(["Why would you want to kick yourself?"], CommandResponseType.Private);

            string reason = args.Length == 0 ? "No reason provided." : args;
            string otherUsername = SteamFriends.GetFriendPersonaName(new(otherClientId));
            LobbyManager.Instance.KickPlayer(otherClientId);
            return new BasicCommandResponse([$"Kicked '{otherUsername}'", $"Reason: '{reason}'"], CommandResponseType.Private);
        }
    }

    public class BanCommand : BaseCommand
    {
        public BanCommand()
        {
            id = "ban";
            description = "Bans the given player.";
            args = new([
                new(
                    [typeof(DefaultCommandArgumentParsers.OnlineClientId), typeof(DefaultCommandArgumentParsers.OfflineClientId)],
                    "player",
                    true
                ),
                new(
                    [typeof(string)],
                    "reason"
                )
            ]);
        }

        public override BaseCommandResponse Execute(BaseExecutionMethod executionMethod, object executorDetails, string args, bool ignorePermissions = false)
        {
            if (args.Length == 0)
                return new BasicCommandResponse(["A player is required for the first argument."], CommandResponseType.Private);

            ulong otherClientId;
            ParsedResult<DefaultCommandArgumentParsers.OnlineClientId> onlinePlayerResult = Api.CommandArgumentParser.Parse<DefaultCommandArgumentParsers.OnlineClientId>(args);
            if (onlinePlayerResult.successful)
            {
                otherClientId = onlinePlayerResult.result;
                args = onlinePlayerResult.newArgs;
            }
            else
            {
                ParsedResult<DefaultCommandArgumentParsers.OfflineClientId> offlinePlayerResult = Api.CommandArgumentParser.Parse<DefaultCommandArgumentParsers.OfflineClientId>(args);
                if (offlinePlayerResult.successful)
                {
                    otherClientId = offlinePlayerResult.result;
                    args = offlinePlayerResult.newArgs;
                }
                else
                    return new BasicCommandResponse(["You did not select a player."], CommandResponseType.Private);
            }

            if (otherClientId == SteamManager.Instance.field_Private_CSteamID_0.m_SteamID)
                return new BasicCommandResponse(["You cannot ban the host."], CommandResponseType.Private);

            if (executionMethod is ChatExecutionMethod && otherClientId == (ulong)executorDetails)
                return new BasicCommandResponse(["Why would you want to ban yourself?"], CommandResponseType.Private);

            if (LobbyManager.bannedPlayers.Contains(otherClientId))
                return new BasicCommandResponse(["That player has already been banned."], CommandResponseType.Private);

            string reason = args.Length == 0 ? "No reason provided." : args;
            if (PersistentDataCompatibility.Enabled && !PersistentDataCompatibility.SetClientData(otherClientId, "Banned", reason))
                return new BasicCommandResponse(["Failed to save ban reason."], CommandResponseType.Private);

            string otherUsername;
            if (LobbyManager.steamIdToUID.ContainsKey(otherClientId))
            {
                otherUsername = SteamFriends.GetFriendPersonaName(new(otherClientId));
                LobbyManager.Instance.BanPlayer(otherClientId);
            }
            else
            {
                otherUsername = PersistentDataCompatibility.GetClientData(otherClientId, "Username");
                LobbyManager.bannedPlayers.Add(otherClientId);
            }

            return new BasicCommandResponse([$"Banned '{otherUsername}'", $"Reason: '{reason}'"], CommandResponseType.Private);
        }
    }

    public class UnbanCommand : BaseCommand
    {
        public UnbanCommand()
        {
            id = "unban";
            description = "Unbans the given player.";
            args = new([
                new(
                    [typeof(DefaultCommandArgumentParsers.OfflineClientId)],
                    "player",
                    true
                )
            ]);
        }

        public override BaseCommandResponse Execute(BaseExecutionMethod executionMethod, object executorDetails, string args, bool ignorePermissions = false)
        {
            if (args.Length == 0)
                return new BasicCommandResponse(["A player is required for the first argument."], CommandResponseType.Private);

            ParsedResult<DefaultCommandArgumentParsers.OfflineClientId> offlinePlayerResult = Api.CommandArgumentParser.Parse<DefaultCommandArgumentParsers.OfflineClientId>(args);
            if (!offlinePlayerResult.successful)
                return new BasicCommandResponse(["You did not select a player."], CommandResponseType.Private);
            
            ulong otherClientId = offlinePlayerResult.result;
            if (!LobbyManager.bannedPlayers.Contains(otherClientId))
                return new BasicCommandResponse(["That player has not been banned."], CommandResponseType.Private);

            if (PersistentDataCompatibility.Enabled)
                PersistentDataCompatibility.RemoveClientData(otherClientId, "Banned");

            LobbyManager.bannedPlayers.Remove(otherClientId);
            return new BasicCommandResponse([$"Unbanned '{PersistentDataCompatibility.GetClientData(otherClientId, "Username")}'"], CommandResponseType.Private);
        }
    }
}