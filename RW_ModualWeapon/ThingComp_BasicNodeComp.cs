using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RW_ModularWeapon
{
    public abstract class ThingComp_BasicNodeComp:ThingComp
    {
        public abstract void UpdateNode();
        public abstract bool AllowNode(Comp_ThingsNode node);

    }
}
