using System.Collections.Generic;
using UnityEngine;

namespace ESGI.Common
{
    [CreateAssetMenu(menuName = "ESGI/Points")]
    public class Points : ScriptableObject
    {
        public List<Vector2> positions;
        
    }
}