using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace friendly_tp
{
    [BepInPlugin(modGUID, modName, modVer)]
    public class BasePatch : BaseUnityPlugin
    {
        private const string modGUID = "BB.friendlyTP";
        private const string modName = "friendlyTP";
        private const string modVer = "0.0.1";

        private readonly Harmony harmony = new Harmony(modGUID);

        private static BasePatch Instance;
        public static ManualLogSource logSrc;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }

            logSrc = BepInEx.Logging.Logger.CreateLogSource(modGUID);
            logSrc.LogInfo("awake invoked");
            harmony.PatchAll();

        }
    }
}
