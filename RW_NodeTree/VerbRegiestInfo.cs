using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RW_NodeTree
{
    public class VerbToolRegiestInfo
    {
        public VerbToolRegiestInfo(string id, Tool berforConvertTool, Tool afterCobvertTool)
        {
            this.id = id;
            this.berforConvertTool = berforConvertTool;
            this.afterCobvertTool = afterCobvertTool;
        }

        public override string ToString()
        {
            return $"id={id}; berforConvertTool={berforConvertTool}; afterCobvertTool={afterCobvertTool}";
        }

        public readonly string id;
        public readonly Tool berforConvertTool, afterCobvertTool;
    }

    public class VerbPropertiesRegiestInfo
    {
        public VerbPropertiesRegiestInfo(string id, VerbProperties berforConvertProperties, VerbProperties afterConvertProperties)
        {
            this.id = id;
            this.berforConvertProperties = berforConvertProperties;
            this.afterConvertProperties = afterConvertProperties;
        }

        public override string ToString()
        {
            return $"id={id}; berforConvertProperties={berforConvertProperties}; afterConvertProperties={afterConvertProperties}";
        }

        public readonly string id;
        public readonly VerbProperties berforConvertProperties, afterConvertProperties;
    }
}
