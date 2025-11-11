using HarmonyLib;
using UnityEngine;

namespace ColoredMiss
{
    [HarmonyPatch(typeof(MissedNoteEffectSpawner), "HandleNoteWasMissed")]
    internal class MissedNoteEffectSpawner_HandleNoteWasMissed_Patch
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
    internal class FlyingSpriteSpawner_SpawnFlyingSprite_Patch
    {
        public static bool Prefix(Vector3 pos, Quaternion rotation, Quaternion inverseRotation, FlyingSpriteSpawner __instance, FlyingSpriteEffect.Pool ____flyingSpriteEffectPool, float ____xSpread, float ____targetYPos, float ____targetZPos, float ____duration, Sprite ____sprite, Material ____material, Color ____color, bool ____shake)
        {
            FlyingSpriteEffect flyingSpriteEffect = ____flyingSpriteEffectPool.Spawn();
            flyingSpriteEffect.didFinishEvent.Add(__instance);
            flyingSpriteEffect.transform.localPosition = pos;
            pos = Quaternion.Inverse(rotation) * pos;

            Color myColor = Color.white;
            if (inverseRotation == Quaternion.Euler(Vector3.forward))
            {
                myColor = MyColors.ColorA;
            }
            else if (inverseRotation == Quaternion.Euler(Vector3.back))
            {
                myColor = MyColors.ColorB;
            }
            flyingSpriteEffect.InitAndPresent(targetPos: rotation * new Vector3(Mathf.Sign(pos.x) * ____xSpread, ____targetYPos, ____targetZPos), duration: ____duration, rotation: rotation, sprite: ____sprite, material: ____material, color: myColor, shake: ____shake);
            return false;
        }
    }

    [HarmonyPatch(typeof(ColorManager), "SetColorScheme")]
    internal class ColorManager_SetColorScheme_Patch
    {
        public static void Postfix(ColorScheme ____colorScheme)
        {
            const float colorBoost = 0.7f;
            MyColors.ColorA = new Color(Mathf.Pow(____colorScheme.saberAColor.r, colorBoost), Mathf.Pow(____colorScheme.saberAColor.g, colorBoost), Mathf.Pow(____colorScheme.saberAColor.b, colorBoost));
            MyColors.ColorB = new Color(Mathf.Pow(____colorScheme.saberBColor.r, colorBoost), Mathf.Pow(____colorScheme.saberBColor.g, colorBoost), Mathf.Pow(____colorScheme.saberBColor.b, colorBoost));
        }
    }

    internal static class MyColors
    {
        public static Color ColorA;
        public static Color ColorB;
    }
}

