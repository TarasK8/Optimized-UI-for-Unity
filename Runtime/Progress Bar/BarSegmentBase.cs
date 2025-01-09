using System;
using UnityEngine;

namespace TarasK8.UI
{
    public abstract class BarSegmentBase : MonoBehaviour
    {
        public event Action<float, float> OnStartPositionChange;
        public event Action<float, float> OnEndPositionChange;
        public (float start, float end) Position
        {
            get => (GetPositionStart(), GetPositionEnd());
            set => SetPosition(value.start, value.end);
        }
        public float PositionStart
        {
            get => GetPositionStart();
            set => SetPositionStartWithEvent(value);
        }
        public float PositionEnd
        {
            get => GetPositionEnd();
            set => SetPositionEndWithEvent(value);
        }
        
        public void SetPosition(float start, float end)
        {
            if (start > end)
            {
                (start, end) = (end, start);
            }

            SetPositionStartWithEvent(start);
            SetPositionEndWithEvent(end);
        }
        
        private void SetPositionStartWithEvent(float position)
        {
            float oldPosition = PositionStart;
            SetPositionStart(position);
            OnStartPositionChange?.Invoke(oldPosition, position);
        }

        private void SetPositionEndWithEvent(float position)
        {
            float oldPosition = PositionEnd;
            SetPositionEnd(position);
            OnEndPositionChange?.Invoke(oldPosition, position);
        }

        protected abstract void SetPositionStart(float position);

        protected abstract void SetPositionEnd(float position);

        protected abstract float GetPositionStart();

        protected abstract float GetPositionEnd();
    }
}
