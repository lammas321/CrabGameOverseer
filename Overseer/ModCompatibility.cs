using BepInEx.IL2CPP;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Overseer
{
    internal static class PersistentDataCompatibility
    {
        internal static bool? enabled;
        internal static bool Enabled
            => enabled == null ? (bool)(enabled = IL2CPPChainloader.Instance.Plugins.ContainsKey("lammas123.PersistentData")) : enabled.Value;

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        internal static HashSet<ulong> GetPersistentClientDataIds()
            => PersistentData.Api.PersistentClientDataIds;

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        internal static bool HasClientData(ulong clientId, string key)
            => PersistentData.Api.GetClientDataFile(clientId).ContainsKey(key);

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        internal static string GetClientData(ulong clientId, string key)
            => PersistentData.Api.GetClientDataFile(clientId).Get(key);

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        internal static bool SetClientData(ulong clientId, string key, string value)
        {
            PersistentData.ClientDataFile file = PersistentData.Api.GetClientDataFile(clientId);
            bool valid = file.Set(key, value);
            file.SaveFile();
            return valid;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        internal static bool RemoveClientData(ulong clientId, string key)
        {
            PersistentData.ClientDataFile file = PersistentData.Api.GetClientDataFile(clientId);
            bool valid = file.Remove(key);
            file.SaveFile();
            return valid;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        internal static bool AddClientData(ulong clientId, string key, string value)
        {
            PersistentData.ClientDataFile file = PersistentData.Api.GetClientDataFile(clientId);
            if (file.ContainsKey(key))
                return true;
            
            bool valid = file.Set(key, value);
            file.SaveFile();
            return valid;
        }
    }
}