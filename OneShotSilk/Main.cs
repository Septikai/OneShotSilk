using Silk;
using Logger = Silk.Logger; // Alias for Silk.Logger to Logger
using HarmonyLib;
using Interfaces;
using Unity.Netcode; // Library for runtime method patching
using UnityEngine; // Unity's core namespace

namespace OneShot
{
    [SilkMod("Oneshot", new[] { "Septikai" }, "1.3.0", "0.4.0", "one-shot-silk")]
    public class Main : SilkMod
    {
        public override void Initialize()
        {
            // your manifest is the mw.mod.toml file
            // use Metadata to access the values you provided in the manifest. Manifest is also available, and provides the other data such as your dependencies and incompats
            Logger.LogInfo("Loading OneShot 1.3.0 by Septikai!");
            
            // this section currently patches any [HarmonyPatch]s you use, like the one named NoMoreLaserCubes below. if you don't patch anything, you can remove these
            // you should keep the logging messages as they help users and developers with debugging
            Logger.LogInfo("Setting up patcher...");
            Harmony harmony = new Harmony("me.septikai.oneshot"); 
            Logger.LogInfo("Patching...");
            harmony.PatchAll();
            Logger.LogInfo("Patches applied!");
        }

        public void Awake()
        {
            Logger.LogInfo("Loaded InfiniteAmmo!");
        }

        public override void Unload()
        {
            Logger.LogInfo("Unloaded InfiniteAmmo!");
        }
    }
    
    [HarmonyPatch(typeof(Weapon), nameof(Weapon.ammo), MethodType.Setter)]
    public class AmmoPatchSetter
    {
        public static void Postfix(Weapon __instance, float value, ref NetworkVariable<float> ___networkAmmo)
        {
            if (__instance.type.Contains(Weapon.WeaponType.Particle)) return;
            if (value != __instance.maxAmmo) ___networkAmmo.Value = 0;
        }
    }
    
    [HarmonyPatch(typeof(ForceField), "Damage")]
    public class ForceFieldDamagePatch
    {
        public static void Postfix(ForceField __instance, Collision2D other)
        {
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
    
    [HarmonyPatch(typeof(RailShot), "ReflectShot")]
    public class ShotReflectionPatch
    {
        public static void Postfix(RaycastHit2D hit)
        {
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