using BepInEx.IL2CPP.Utils;
using HarmonyLib;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Overseer
{
    internal static class Patches
    {
        internal static ConcurrentQueue<ulong> pending = new();
        internal static volatile int loadGeneration = 0;
        internal static volatile bool done = false;

        internal static Thread thread;
        internal static Coroutine coroutine;

        [HarmonyPatch(typeof(LobbyManager), nameof(LobbyManager.StartLobby))]
        [HarmonyPatch(typeof(LobbyManager), nameof(LobbyManager.StartPracticeLobby))]
        [HarmonyPostfix]
        internal static void PostLobbyManagerStartLobby()
        {
            if (!PersistentDataCompatibility.Enabled)
                return;

            if (coroutine != null)
                LobbyManager.Instance.StopCoroutine(coroutine);

            int generation = Interlocked.Increment(ref loadGeneration);

            pending = new();
            ConcurrentQueue<ulong> localPending = pending;
            done = false;

            HashSet<ulong> clientDataIds = [.. PersistentDataCompatibility.GetPersistentClientDataIds()];
            LobbyManager.bannedPlayers.Clear();
            LobbyManager.bannedPlayers.EnsureCapacity(clientDataIds.Count);

            Overseer.Instance.Log.LogInfo("Loading all banned players...");

            thread = new Thread(() =>
            {
                foreach (ulong clientId in clientDataIds)
                {
                    if (PersistentDataCompatibility.HasClientData(clientId, "Banned"))
                        localPending.Enqueue(clientId);

                    if (loadGeneration != generation)
                        return;
                }

                done = true;
            });
            thread.IsBackground = true;
            thread.Start();

            coroutine = LobbyManager.Instance.StartCoroutine(CoroReadPending());
        }

        internal static IEnumerator CoroReadPending()
        {
            while (true)
            {
                yield return null;

                bool isDone = done;

                while (pending.TryDequeue(out ulong clientId) && !LobbyManager.bannedPlayers.Contains(clientId))
                {
                    LobbyManager.bannedPlayers.Add(clientId);

                    if (LobbyManager.steamIdToUID.ContainsKey(clientId))
                        LobbyManager.Instance.KickPlayer(clientId);
                }

                if (isDone)
                    break;
            }

            Overseer.Instance.Log.LogInfo("Loaded all banned players");
        }

        [HarmonyPatch(typeof(LobbyManager), nameof(LobbyManager.CloseLobby))]
        [HarmonyPostfix]
        internal static void PostLobbyManagerCloseLobby()
        {
            if (PersistentDataCompatibility.Enabled)
                LobbyManager.bannedPlayers.Clear();
        }
        
        // Prevents disconnecting the host, and kills the player when they are forcefully disconnected, helping to prevent them from continuing to effect the game (because it can take a few moments for them to disconnect for other clients)
        [HarmonyPatch(typeof(LobbyManager), nameof(LobbyManager.KickPlayer))]
        [HarmonyPatch(typeof(LobbyManager), nameof(LobbyManager.BanPlayer))]
        [HarmonyPrefix]
        internal static bool PreLobbyManagerForceDisconnectPlayer(ulong param_1)
        {
            if (param_1 == Utility.HostClientId)
                return false;

            if (GameManager.Instance && GameManager.Instance.activePlayers.ContainsKey(param_1) && !GameManager.Instance.activePlayers[param_1].dead)
                GameServer.PlayerDied(param_1, param_1, Vector3.zero);
            return true;
        }

        // Fixes the player list not updating after banning a player
        [HarmonyPatch(typeof(ManagePlayerListing), nameof(ManagePlayerListing.BanPlayer))]
        [HarmonyPostfix]
        internal static void PostManagePlayerListingBanPlayer()
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