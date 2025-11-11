using HarmonyLib;
using IPA;
using System.Reflection;
using UnityEngine;
using IPALogger = IPA.Logging.Logger;


namespace ColoredMiss
{

    [Plugin(RuntimeOptions.SingleStartInit)]
    public class Plugin
    {
        internal static Plugin Instance { get; private set; }
        internal static IPALogger Log { get; private set; }
        private Harmony harmony;
        private Assembly executingAssembly = Assembly.GetExecutingAssembly();


        [Init]
        public void Init(IPALogger logger)
        {
            harmony = new Harmony(nameof(ColoredMiss));
            Instance = this;
            Log = logger;
        }

        #region BSIPA Config
        //Uncomment to use BSIPA's config
        /*
        [Init]
        public void InitWithConfig(Config conf)
        {
            Configuration.PluginConfig.Instance = conf.Generated<Configuration.PluginConfig>();
            Log.Debug("Config loaded");
        }
        */
        #endregion

        [OnStart]
        public void OnApplicationStart()
        {
            harmony.PatchAll(executingAssembly);

        }

        [OnExit]
        public void OnApplicationQuit()
        {
            harmony.UnpatchAll(nameof(ColoredMiss));

        }
    }
}
