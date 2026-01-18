using BepInEx;
using BepInEx.IL2CPP;
using HarmonyLib;
using System.Collections.Generic;
using System.Globalization;

namespace Overseer
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInDependency("lammas123.CrabDevKit")]
    [BepInDependency("lammas123.PersistentData", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("lammas123.ChatCommands")]
    public sealed class Overseer : BasePlugin
    {
        internal static Overseer Instance { get; private set; }

        public override void Load()
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

            Instance = this;

            ChatCommands.Api.RegisterCommand(new WarnCommand());
            ChatCommands.Api.RegisterCommand(new KickCommand());
            ChatCommands.Api.RegisterCommand(new BanCommand());
            ChatCommands.Api.RegisterCommand(new UnbanCommand());

            Harmony harmony = new(MyPluginInfo.PLUGIN_NAME);
            harmony.PatchAll(typeof(Patches));

            Log.LogInfo($"Initialized [{MyPluginInfo.PLUGIN_NAME} {MyPluginInfo.PLUGIN_VERSION}]");
        }
    }
}