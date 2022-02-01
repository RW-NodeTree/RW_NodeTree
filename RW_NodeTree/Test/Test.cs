using HarmonyLib;
using RimWorld;
using RW_NodeTree.Patch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;
using Verse;

namespace RW_NodeTree.Test
{
    internal class Test
    {
        /*
        public static void Main(string[] args)
        {
            MethodInfo _GetExplanationUnfinalized = typeof(StatWorker_MeleeDamageAmountTrap).GetMethod(
                "GetExplanationUnfinalized",
                new Type[] { typeof(StatRequest), typeof(ToStringNumberSense) });
            Console.WriteLine(_GetExplanationUnfinalized?.DeclaringType);
            Console.WriteLine(_GetExplanationUnfinalized?.DeclaringType == typeof(StatWorker_MeleeDamageAmountTrap));
            MethodInfo DrawMeshInstanced = typeof(Graphics).GetMethod(
                "DrawMeshInstanced",
                new Type[] { 
                    typeof(Mesh), 
                    typeof(int), 
                    typeof(Material),
                    typeof(Matrix4x4[]),
                    typeof(int),
                    typeof(MaterialPropertyBlock),
                    typeof(ShadowCastingMode),
                    typeof(bool),
                    typeof(int),
                    typeof(Camera),
                    typeof(LightProbeUsage),
                    typeof(LightProbeProxyVolume)
                }
            );
            Console.WriteLine(DrawMeshInstanced);
            MethodInfo Internal_DrawMesh = typeof(Graphics).GetMethod(
                "Internal_DrawMesh",
                AccessTools.all
            );
            Console.WriteLine(Internal_DrawMesh);
            MethodInfo _FinalizeValue = typeof(StatWorker).GetMethod(
                "FinalizeValue"
            );
            Console.WriteLine(_FinalizeValue);
            ParameterInfo[] array = _FinalizeValue.GetParameters();
            Type[] StatWorker_FinalizeValue_ParmsType = new Type[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                ParameterInfo item = array[i];
                StatWorker_FinalizeValue_ParmsType[i] = item.ParameterType;
            }
            _FinalizeValue = typeof(StatWorker).GetMethod(
                "FinalizeValue",
                StatWorker_FinalizeValue_ParmsType
            );
            Console.WriteLine(_FinalizeValue);
            Console.ReadKey();
            string s = "a";
            string p = "ab*";
            Console.WriteLine(IsMatch(s, p));
            Console.ReadKey();
        }*/
        //public static bool IsMatch(string s, string p, int c = 0, int i = 0)
        //{
        //    w:;
        //    int o = i + 1;
        //    if (p.Length > o && p[o] == '*')
        //    {
        //        ++o;
        //        while (c < s.Length && (p[i] == '.' || s[c] == p[i]))
        //        {
        //            if (IsMatch(s, p, c, o)) return true;
        //            ++c;
        //        }
        //        if (IsMatch(s, p, c, o)) return true;
        //    }
        //    else if (c < s.Length && i < p.Length && (p[i] == '.' || s[c] == p[i]))
        //    {
        //        ++c;
        //        i = o;
        //        goto w;
        //    }
        //    else if (p.Length > i && p[i] == '*')
        //    {
        //        --c;
        //        --i;
        //        goto w;
        //    }
        //    return c == s.Length && i == p.Length;
        //}

    }
}
