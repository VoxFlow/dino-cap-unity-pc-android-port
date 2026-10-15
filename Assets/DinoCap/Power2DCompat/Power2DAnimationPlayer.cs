using System;
using DinoCap.Data;
using UnityEngine;

namespace DinoCap.Power2DCompat
{
    public sealed class Power2DAnimationPlayer : MonoBehaviour
    {
        public Power2DAnimationAsset Current { get; private set; }
        public int FrameIndex { get; private set; }
        public bool IsComplete { get; private set; }

        public event Action<int> FrameEntered;
        public event Action<int> MarkedFrameEntered;
        public event Action Completed;

        private float _accumulator;
        private int _lastEventFrame = -1;

        public void Play(Power2DAnimationAsset animation, bool restartIfSame = false)
        {
            if (animation == null) return;
            if (!restartIfSame && Current == animation) return;
            Current = animation;
            FrameIndex = 0;
            IsComplete = false;
            _accumulator = 0f;
            _lastEventFrame = -1;
            EnterCurrentFrame();
        }

        private void Update()
        {
            Tick(Time.deltaTime);
        }

        public void Tick(float dt)
        {
            if (Current == null || Current.frames == null || Current.frames.Length == 0 || IsComplete)
                return;

            _accumulator += dt;
            if (_accumulator < Current.frameDuration)
                return;

            // Shipped quirk: at most ONE frame per Update; remainder is discarded.
            _accumulator = 0f;
            int next = FrameIndex + 1;
            if (next >= Current.frames.Length)
            {
                if (Current.loop)
                    next = 0;
                else
                {
                    FrameIndex = Current.frames.Length - 1;
                    IsComplete = true;
                    Completed?.Invoke();
                    return;
                }
            }

            FrameIndex = next;
            EnterCurrentFrame();
        }

        private void EnterCurrentFrame()
        {
            FrameEntered?.Invoke(FrameIndex);
            var frame = Current.frames[FrameIndex];
            if (frame != null && frame.hasEvent && _lastEventFrame != FrameIndex)
            {
                _lastEventFrame = FrameIndex;
                MarkedFrameEntered?.Invoke(FrameIndex);
            }
        }

        public Power2DFrameRecord GetCurrentFrame()
        {
            if (Current == null || Current.frames == null || Current.frames.Length == 0) return null;
            return Current.frames[Mathf.Clamp(FrameIndex, 0, Current.frames.Length - 1)];
        }
    }
}
