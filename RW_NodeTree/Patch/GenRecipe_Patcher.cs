using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RW_NodeTree;

namespace RW_NodeTree.Patch
{
    [HarmonyPatch(typeof(GenRecipe))]
    internal static class GenRecipe_Patcher
    {
        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(GenRecipe),
            "MakeRecipeProducts"
            )]
        public static void PostGenRecipe_MakeRecipeProducts(RecipeDef recipeDef, Pawn worker, List<Thing> ingredients, Thing dominantIngredient, IBillGiver billGiver, Precept_ThingStyle precept, ref IEnumerable<Thing> __result)
        {
            __result = ((CompChildNodeProccesser)dominantIngredient)?.PostGenRecipe_MakeRecipeProducts(recipeDef, worker, ingredients, dominantIngredient, billGiver, precept, RecipeInvokeSource.dominantIngredient, __result) ?? __result;
            foreach(Thing thing in ingredients)
            {
                __result = ((CompChildNodeProccesser)thing)?.PostGenRecipe_MakeRecipeProducts(recipeDef, worker, ingredients, dominantIngredient, billGiver, precept, RecipeInvokeSource.ingredients, __result) ?? __result;
            }
            __result = ((CompChildNodeProccesser)worker)?.PostGenRecipe_MakeRecipeProducts(recipeDef, worker, ingredients, dominantIngredient, billGiver, precept, RecipeInvokeSource.worker, __result) ?? __result;
            try
            {
                __result = new List<Thing>(__result);
                foreach (Thing thing in __result)
                {
                    __result = ((CompChildNodeProccesser)thing)?.PostGenRecipe_MakeRecipeProducts(recipeDef, worker, ingredients, dominantIngredient, billGiver, precept, RecipeInvokeSource.products, __result) ?? __result;
                }
            }
            catch (Exception ex)
            {
                Log.Message(ex.ToString());
            }
        }

    }
}

namespace RW_NodeTree
{
    /// <summary>
    /// Node function proccesser
    /// </summary>
    public partial class CompChildNodeProccesser : ThingComp, IThingHolder
    {
        public IEnumerable<Thing> PostGenRecipe_MakeRecipeProducts(RecipeDef recipeDef, Pawn worker, List<Thing> ingredients, Thing dominantIngredient1, IBillGiver billGiver, Precept_ThingStyle precept, RecipeInvokeSource InvokeSource, IEnumerable<Thing> result)
        {
            foreach (CompBasicNodeComp comp in AllNodeComp)
            {
                try
                {
                    result = comp.internal_PostGenRecipe_MakeRecipeProducts(recipeDef, worker, ingredients, dominantIngredient1, billGiver, precept, InvokeSource, result) ?? result;
                }
                catch(Exception ex)
                {
                    Log.Error(ex.ToString());
                }
            }
            return result;
        }
    }
    public abstract partial class CompBasicNodeComp : ThingComp
    {
        protected virtual IEnumerable<Thing> PostGenRecipe_MakeRecipeProducts(RecipeDef recipeDef, Pawn worker, List<Thing> ingredients, Thing dominantIngredient1, IBillGiver billGiver, Precept_ThingStyle precept, RecipeInvokeSource invokeSource, IEnumerable<Thing> result)
        {
            return result;
        }
        internal IEnumerable<Thing> internal_PostGenRecipe_MakeRecipeProducts(RecipeDef recipeDef, Pawn worker, List<Thing> ingredients, Thing dominantIngredient1, IBillGiver billGiver, Precept_ThingStyle precept, RecipeInvokeSource invokeSource, IEnumerable<Thing> result)
            => PostGenRecipe_MakeRecipeProducts(recipeDef, worker, ingredients, dominantIngredient1, billGiver, precept, invokeSource, result);
    }

    public enum RecipeInvokeSource
    {
        dominantIngredient,
        ingredients,
        worker,
        products
    }
}