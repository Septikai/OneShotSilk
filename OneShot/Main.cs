using HarmonyLib;
using modweaver.core;
using NLog;

namespace OneShot
{
    [ModMainClass]
    public class Main : Mod
    {
        public override void Init()
        {
            // your manifest is the mw.mod.toml file
            // use Metadata to access the values you provided in the manifest. Manifest is also available, and provides the other data such as your dependencies and incompats
            Logger.Info("Loading {0} v{1} by {2}!", Metadata.title, Metadata.version,
                string.Join(", ", Metadata.authors));
            
            // this section currently patches any [HarmonyPatch]s you use, like the one named NoMoreLaserCubes below. if you don't patch anything, you can remove these
            // you should keep the logging messages as they help users and developers with debugging
            Logger.Debug("Setting up patcher...");
            Harmony harmony = new Harmony(Metadata.id); 
            Logger.Debug("Patching...");
            harmony.PatchAll();
        }

        public override void Ready()
        {
            Logger.Info("Loaded {0}!", Metadata.title);
        }

        public override void OnGUI(ModsMenuPopup ui)
        {
            // you can add data to your mods page here
            // we recommend if you are going to add ui here, put
            // ui.CreateDivider() first
            // you'll see why :3
        }
    }
    
    [HarmonyPatch(typeof(Weapon), nameof(Weapon.ammo), MethodType.Setter)]
    public class AmmoPatchSetter
    {
        [HarmonyPostfix]
        public static void OneAmmo(Weapon __instance, float value)
        {
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