using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

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
        internal static void PostGenRecipe_MakeRecipeProducts(RecipeDef recipeDef, Pawn worker, List<Thing> ingredients, Thing? dominantIngredient, IBillGiver billGiver, Precept_ThingStyle? precept, ref IEnumerable<Thing> __result)
        {
            if (recipeDef == null) throw new ArgumentNullException(nameof(recipeDef));
            if (worker == null) throw new ArgumentNullException(nameof(worker));
            if (ingredients == null) throw new ArgumentNullException(nameof(ingredients));
            if (billGiver == null) throw new ArgumentNullException(nameof(billGiver));
            __result = (dominantIngredient as IRecipePatcher)?.PostGenRecipe_MakeRecipeProducts(recipeDef, worker, ingredients, dominantIngredient, billGiver, precept, RecipeInvokeSource.dominantIngredient, __result) ?? __result;
            foreach (Thing thing in ingredients)
            {
                __result = (thing as IRecipePatcher)?.PostGenRecipe_MakeRecipeProducts(recipeDef, worker, ingredients, dominantIngredient, billGiver, precept, RecipeInvokeSource.ingredients, __result) ?? __result;
            }
            __result = (worker as IRecipePatcher)?.PostGenRecipe_MakeRecipeProducts(recipeDef, worker, ingredients, dominantIngredient, billGiver, precept, RecipeInvokeSource.worker, __result) ?? __result;
            try
            {
                __result = new List<Thing>(__result);
                foreach (Thing thing in __result)
                {
                    __result = (thing as IRecipePatcher)?.PostGenRecipe_MakeRecipeProducts(recipeDef, worker!, ingredients, dominantIngredient, billGiver, precept, RecipeInvokeSource.products, __result) ?? __result;
                }
            }
            catch (Exception ex)
            {
                Log.Message(ex.ToString());
            }
            __result = from x in __result where x != null select x;
        }

    }
}

namespace RW_NodeTree
{
    public partial interface IRecipePatcher
    {
        IEnumerable<Thing> PostGenRecipe_MakeRecipeProducts(RecipeDef recipeDef, Pawn worker, List<Thing> ingredients, Thing? dominantIngredient, IBillGiver billGiver, Precept_ThingStyle? precept, RecipeInvokeSource invokeSource, IEnumerable<Thing> result);
    }

    public enum RecipeInvokeSource
    {
        dominantIngredient,
        ingredients,
        worker,
        products
    }
}