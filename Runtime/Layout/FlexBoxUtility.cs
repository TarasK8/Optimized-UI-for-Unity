using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TarasK8.UI.Layout
{
    public static class FlexBoxUtility
    {
        public static float CalculateElementSize(
            float flexGrow,
            float flexShrink,
            float flexBasis,
            float totalFlexGrow,
            float totalFlexShrink,
            float freeSpace)
        {
            float initialSize = flexBasis;

            if (freeSpace > 0f && totalFlexGrow > 0f)
            {
                float growFactor = (flexGrow / totalFlexGrow) * freeSpace;
                initialSize += growFactor;
            }
            else if (freeSpace < 0f && totalFlexShrink > 0f)
            {
                float shrinkFactor = (flexShrink / totalFlexShrink) * Mathf.Abs(freeSpace);
                initialSize -= shrinkFactor;
            }

            return initialSize;
        }
    }
}