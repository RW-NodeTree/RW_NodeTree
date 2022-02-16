using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace RW_NodeTree.Tools
{
    public static class MathHelper
    {
        public static Vector3 toVector3OnMap(this Vector2 vector)
        {
            return new Vector3(vector.x, 0, vector.y);
        }
    }
}
