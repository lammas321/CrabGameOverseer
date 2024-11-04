using HarmonyLib;

namespace Overseer
{
    internal static class Patches
    {
        [HarmonyPatch(typeof(LobbyManager), nameof(LobbyManager.StartLobby))]
        [HarmonyPatch(typeof(LobbyManager), nameof(LobbyManager.StartPracticeLobby))]
        [HarmonyPostfix]
        internal static void PostLobbyManagerStartLobby()
        {
            LobbyManager.bannedPlayers.Clear();

            foreach (ulong clientId in PersistentDataCompatibility.GetPersistentClientDataIds())
                if (PersistentDataCompatibility.HasClientData(clientId, "Banned"))
                    LobbyManager.bannedPlayers.Add(clientId);
        }

        [HarmonyPatch(typeof(LobbyManager), nameof(LobbyManager.CloseLobby))]
        [HarmonyPostfix]
        internal static void PostLobbyManagerCloseLobby()
            => LobbyManager.bannedPlayers.Clear();
        
        // Prevents disconnecting the host, and kills the player when they are forcefully disconnected, helping to prevent them from continuing to effect the game (because it can take a few moments for them to disconnect for other clients)
        [HarmonyPatch(typeof(LobbyManager), nameof(LobbyManager.KickPlayer))]
        [HarmonyPatch(typeof(LobbyManager), nameof(LobbyManager.BanPlayer))]
        [HarmonyPrefix]
        internal static bool PreLobbyManagerForceDisconnectPlayer(ulong param_1)
        {
            if (param_1 == Utility.HostClientId)
                return false;

            if (GameManager.Instance && GameManager.Instance.activePlayers.ContainsKey(param_1) && !GameManager.Instance.activePlayers[param_1].dead)
                GameServer.PlayerDied(param_1, param_1, UnityEngine.Vector3.zero);
            return true;
        }

        // Fixes the player list not updating after banning a player
        [HarmonyPatch(typeof(PlayerListManagePlayer), nameof(PlayerListManagePlayer.BanPlayer))]
        [HarmonyPostfix]
        internal static void PostPlayerListManagePlayerBanPlayer()
            => PlayerList.Instance.UpdateList();

        // Gives a ban reason if a player is banned without one
        [HarmonyPatch(typeof(LobbyManager), nameof(LobbyManager.BanPlayer))]
        [HarmonyPostfix]
        internal static void PostLobbyManagerBanPlayer(ulong param_1)
        {
            if (LobbyManager.bannedPlayers.Contains(param_1) && PersistentDataCompatibility.Enabled)
                PersistentDataCompatibility.AddClientData(param_1, "Banned", "No reason found.");
        }
    }
}