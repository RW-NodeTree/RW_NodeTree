using HarmonyLib;
using Verse;

namespace RW_NodeTree.Patch.CombatExtended
{
    [StaticConstructorOnStartup]
    internal static class HarmonyInjector_CombatExtended
    {
        static HarmonyInjector_CombatExtended()
        {
            CombatExtended_PawnRenderer_Patcher.PatchDrawMesh(patcher);
            CombatExtended_CompAmmoUser_Patcher.PatchCompEquippable(patcher);
            CombatExtended_CompFireModes_Patcher.PatchVerb(patcher);
            CombatExtended_JobDriver_Reload_Patcher.PatchJobDriver_Reload(patcher);
            CombatExtended_Verb_LaunchProjectileCE_Patcher.PatchVerb_LaunchProjectileCE(patcher);
            CombatExtended_LoadoutPropertiesExtension_Patcher.PatchLoadoutPropertiesExtension(patcher);
            CombatExtended_BipodComp_Patcher.PatchCompResetVerbProps(patcher);
        }

        public static Harmony patcher = new Harmony("RW_NodeTree.Patch.CombatExtended");
    }
}
