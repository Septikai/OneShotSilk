using BepInEx;
using HarmonyLib;
using HarmonyLib.Tools;
using Interfaces;
using UnityEngine;

namespace OneShot
{
    [BepInPlugin(ModName, ModGUID, ModVersion)]
    public class Main : BaseUnityPlugin
    {
        public const string ModName = "OneShot";
        public const string ModAuthor  = "Septikai";
        public const string ModGUID = "me.septikai.OneShot";
        public const string ModVersion = "1.0.0";
        internal Harmony Harmony;
        internal void Awake()
        {
            // Creating new harmony instance
            Harmony = new Harmony(ModGUID);

            // Applying patches
            Harmony.PatchAll();
            Logger.LogInfo($"{ModName} successfully loaded! Made by {ModAuthor}");
        }
    }
    
    [HarmonyPatch(typeof(VersionNumberTextMesh), nameof(VersionNumberTextMesh.Start))]
    public class VersionNumberTextMeshPatch
    {
        public static void Postfix(VersionNumberTextMesh __instance)
        {
            __instance.textMesh.text += $"\n<color=red>{Main.ModName} v{Main.ModVersion} by {Main.ModAuthor}</color>";
        }
    }

    [HarmonyPatch(typeof(Weapon), nameof(Weapon.ammo), MethodType.Setter)]
    public class AmmoPatchSetter
    {
        [HarmonyPostfix]
        public static void OneAmmo(Weapon __instance, float value)
        {
            var playerCount = LobbyController.instance.GetPlayerCount();
            if (playerCount != 1) return;
            if (__instance.type.Contains(Weapon.WeaponType.Particle)) return;
            if (value != __instance.maxAmmo) __instance.networkAmmo.Value = 0;
        }
    }
    
    [HarmonyPatch(typeof(ForceField), nameof(ForceField.Damage))]
    public class ForceFieldDamagePatch
    {
        [HarmonyPostfix]
        public static void OneUseBlades(ForceField __instance, Collision2D other)
        {
            var playerCount = LobbyController.instance.GetPlayerCount();
            if (playerCount != 1) return;
            var component = other.gameObject.GetComponent<IDamageable>();
            if (component.GetType().IsSubclassOf(typeof(Weapon))) return;
            var blades = UnityEngine.Object.FindObjectsOfType<ParticleBlade>();
            if (blades == null) return;
            foreach (var blade in blades)
            {
                if (blade.GetType() == typeof(DoubleParticleBlade))
                {
                    if (blade.particleField.forceField.GetInstanceID() != __instance.GetInstanceID() &&
                        ((DoubleParticleBlade) blade).secondaryParticleField.forceField.GetInstanceID() !=
                        __instance.GetInstanceID()) continue;
                }
                else
                {
                    if (blade.particleField.forceField.GetInstanceID() != __instance.GetInstanceID()) continue;
                }
                blade.Disintegrate();
            }
        }
    }
    
    [HarmonyPatch(typeof(RailShot), nameof(RailShot.ReflectShot))]
    public class ShotReflectionPatch
    {
        [HarmonyPostfix]
        public static void OneReflect(RaycastHit2D hit)
        {
            var playerCount = LobbyController.instance.GetPlayerCount();
            if (playerCount != 1) return;
            var forceField = hit.collider.gameObject.GetComponent<ForceField>();
            var blades = UnityEngine.Object.FindObjectsOfType<ParticleBlade>();
            if (blades == null) return;
            foreach (var blade in blades)
            {
                if (blade.GetType() == typeof(DoubleParticleBlade))
                {
                    if (blade.particleField.forceField.GetInstanceID() != forceField.GetInstanceID() &&
                        ((DoubleParticleBlade) blade).secondaryParticleField.forceField.GetInstanceID() !=
                        forceField.GetInstanceID()) continue;
                }
                else
                {
                    if (blade.particleField.forceField.GetInstanceID() != forceField.GetInstanceID()) continue;
                }
                blade.Disintegrate();
            }
        }
    }
    
}
