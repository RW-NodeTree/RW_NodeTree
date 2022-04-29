using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RW_NodeTree
{
    public struct VerbToolRegiestInfo
    {
        public VerbToolRegiestInfo(string id, Tool berforConvertTool, Tool afterCobvertTool)
        {
            this.id = id;
            this.berforConvertTool = berforConvertTool;
            this.afterCobvertTool = afterCobvertTool;
        }

        public bool Vaildity => berforConvertTool != null && afterCobvertTool != null;

        public override string ToString()
        {
            return $"id={id}; berforConvertTool={berforConvertTool}; afterCobvertTool={afterCobvertTool}";
        }

        public string id;
        public Tool berforConvertTool, afterCobvertTool;
    }

    public struct VerbPropertiesRegiestInfo
    {
        public VerbPropertiesRegiestInfo(string id, VerbProperties berforConvertProperties, VerbProperties afterConvertProperties)
        {
            this.id = id;
            this.berforConvertProperties = berforConvertProperties;
            this.afterConvertProperties = afterConvertProperties;
        }
        public bool Vaildity => berforConvertProperties != null && afterConvertProperties != null;

        public override string ToString()
        {
            return $"id={id}; berforConvertProperties={berforConvertProperties}; afterConvertProperties={afterConvertProperties}";
        }

        public string id;
        public VerbProperties berforConvertProperties,afterConvertProperties;
    }
}
