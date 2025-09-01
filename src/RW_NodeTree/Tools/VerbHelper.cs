using System;
using System.Collections.Generic;
using Verse;

namespace RW_NodeTree.Tools
{
    public static class VerbHelper
    {

        public static bool IsVerbOwnerController(this IVerbOwner owner, CompChildNodeProccesser? proccess)
        {
            if (owner == null) throw new ArgumentNullException(nameof(owner));
            if (proccess == null) return false;
            CompChildNodeProccesser? comp = (owner as Thing) ?? (owner as ThingComp)?.parent;
            return proccess == comp;
        }
    }
}
