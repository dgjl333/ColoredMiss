using HarmonyLib;
using IPA;
using IPA.Config;
using IPA.Config.Stores;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using IPALogger = IPA.Logging.Logger;


namespace DetailedMiss
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
            harmony = new Harmony(nameof(DetailedMiss));
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
            Log.Debug("DetailedMiss-OnApplicationStart");
            harmony.PatchAll(executingAssembly);

        }

        [OnExit]
        public void OnApplicationQuit()
        {
            harmony.UnpatchAll(nameof(DetailedMiss));

        }
    }
    [HarmonyPatch(typeof(MissedNoteEffectSpawner), "HandleNoteWasMissed")]
    public class MissedNoteEffectSpawner_HandleNoteWasMissed_Patch
    {
        public static bool Prefix(NoteController noteController, AudioTimeSyncController ____audioTimeSyncController, float ____spawnPosZ, FlyingSpriteSpawner ____missedNoteFlyingSpriteSpawner)
        {
            if (!noteController.hidden && !(noteController.noteData.time + 0.5f < ____audioTimeSyncController.songTime) && noteController.noteData.colorType != ColorType.None)
            {
                Vector3 position = noteController.noteTransform.position;
                Quaternion worldRotation = noteController.worldRotation;
                position = noteController.inverseWorldRotation * position;
                position.z = ____spawnPosZ;
                position = worldRotation * position;
                ____missedNoteFlyingSpriteSpawner.SpawnFlyingSprite(position, noteController.worldRotation, noteController.noteData.colorType == ColorType.ColorA ? Quaternion.Euler(Vector3.forward) : Quaternion.Euler(Vector3.back));
            }
            return false;
        }
    }


    [HarmonyPatch(typeof(FlyingSpriteSpawner), "SpawnFlyingSprite")]
    public class FlyingSpriteSpawner_SpawnFlyingSprite_Patch
    {
        public static bool Prefix(Vector3 pos, Quaternion rotation, Quaternion inverseRotation, FlyingSpriteSpawner __instance, FlyingSpriteEffect.Pool ____flyingSpriteEffectPool, float ____xSpread, float ____targetYPos, float ____targetZPos, float ____duration, Sprite ____sprite, Material ____material, Color ____color, bool ____shake)
        {
            FlyingSpriteEffect flyingSpriteEffect = ____flyingSpriteEffectPool.Spawn();
            flyingSpriteEffect.didFinishEvent.Add(__instance);
            flyingSpriteEffect.transform.localPosition = pos;
            pos = Quaternion.Inverse(rotation) * pos;

            Color myColor = Color.green;
            if (inverseRotation == Quaternion.Euler(Vector3.forward))
            {
                myColor = Color.red;
            }
            else if(inverseRotation == Quaternion.Euler(Vector3.back))
            {
                myColor = Color.blue;
            }
                flyingSpriteEffect.InitAndPresent(targetPos: rotation * new Vector3(Mathf.Sign(pos.x) * ____xSpread, ____targetYPos, ____targetZPos), duration: ____duration, rotation: rotation, sprite: ____sprite, material: ____material, color: myColor, shake: ____shake);
            return false;
        }
    }
}
