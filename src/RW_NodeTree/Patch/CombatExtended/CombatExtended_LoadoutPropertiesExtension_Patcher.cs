using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Verse;

namespace RW_NodeTree.Patch.CombatExtended
{
    internal static class CombatExtended_LoadoutPropertiesExtension_Patcher
    {
        private static MethodInfo _LoadoutPropertiesExtension_LoadWeaponWithRandAmmo_Transpiler = typeof(CombatExtended_LoadoutPropertiesExtension_Patcher).GetMethod("LoadoutPropertiesExtension_LoadWeaponWithRandAmmo_Transpiler", BindingFlags.Static | BindingFlags.NonPublic);
        private static MethodInfo _LoadoutPropertiesExtension_TryGenerateAmmoFor_Transpiler = typeof(CombatExtended_LoadoutPropertiesExtension_Patcher).GetMethod("LoadoutPropertiesExtension_TryGenerateAmmoFor_Transpiler", BindingFlags.Static | BindingFlags.NonPublic);
        private static MethodInfo _LoadoutPropertiesExtension_TryGetComp = typeof(CombatExtended_LoadoutPropertiesExtension_Patcher).GetMethod("LoadoutPropertiesExtension_TryGetComp", BindingFlags.Static | BindingFlags.NonPublic);
        private static Type CombatExtended_LoadoutPropertiesExtension = GenTypes.GetTypeInAnyAssembly("CombatExtended.LoadoutPropertiesExtension");
        private static Type CombatExtended_CompAmmoUser = GenTypes.GetTypeInAnyAssembly("CombatExtended.CompAmmoUser");

        private static MethodInfo? TryGetComp;


        private static IEnumerable<CodeInstruction> LoadoutPropertiesExtension_LoadWeaponWithRandAmmo_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.Calls(TryGetComp)) yield return new CodeInstruction(OpCodes.Call, _LoadoutPropertiesExtension_TryGetComp);
                else yield return instruction;
            }
        }

        private static IEnumerable<CodeInstruction> LoadoutPropertiesExtension_TryGenerateAmmoFor_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.Calls(TryGetComp)) yield return new CodeInstruction(OpCodes.Call, _LoadoutPropertiesExtension_TryGetComp);
                else yield return instruction;
            }
        }

        private static ThingComp? LoadoutPropertiesExtension_TryGetComp(Thing? thing)
        {
            CompChildNodeProccesser? comp = thing;
            if (comp != null)
            {
                CompEquippable? equippable = thing.TryGetComp<CompEquippable?>();
                if (equippable != null)
                {
                    thing = comp.GetBeforeConvertThingWithVerb(typeof(CompEquippable), equippable.PrimaryVerb, true).Item1 as ThingWithComps;

                    ThingComp?
                    result = (ThingComp?)TryGetComp?.Invoke(null, new object?[] { thing });
                    if (result != null) return result;

                    thing = comp.GetBeforeConvertThingWithVerb(typeof(CompEquippable), equippable.PrimaryVerb).Item1 as ThingWithComps;

                    result = (ThingComp?)TryGetComp?.Invoke(null, new object?[] { thing });
                    if (result != null) return result;
                }
                thing = comp.parent;
            }
            return (ThingComp?)TryGetComp?.Invoke(null, new object?[] { thing });
        }


        public static void PatchLoadoutPropertiesExtension(Harmony patcher)
        {
            if (CombatExtended_CompAmmoUser != null)
            {
                TryGetComp = typeof(ThingCompUtility).GetMethod("TryGetComp", BindingFlags.Static | BindingFlags.Public, null, new Type[] { typeof(Thing) }, null).MakeGenericMethod(new Type[] { CombatExtended_CompAmmoUser });
            }
            if (CombatExtended_LoadoutPropertiesExtension != null)
            {
                MethodInfo target = CombatExtended_LoadoutPropertiesExtension.GetMethod("LoadWeaponWithRandAmmo", BindingFlags.Instance | BindingFlags.NonPublic);
                patcher.Patch(
                    target,
                    transpiler: new HarmonyMethod(_LoadoutPropertiesExtension_LoadWeaponWithRandAmmo_Transpiler)
                    );
                target = CombatExtended_LoadoutPropertiesExtension.GetMethod("TryGenerateAmmoFor", BindingFlags.Instance | BindingFlags.NonPublic);
                patcher.Patch(
                    target,
                    transpiler: new HarmonyMethod(_LoadoutPropertiesExtension_TryGenerateAmmoFor_Transpiler)
                    );
            }
        }
    }
}
