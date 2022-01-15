using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RW_NodeTree.Patch
{
    [HarmonyPatch(typeof(StatWorker), "GetValue")]
    internal static class StatWorker_GetValue_Patcher
    {

    }
}
