using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;

namespace Main.Utils.Component
{
    
    /// <summary>
    /// https://github.com/Hoyeom/Hoyeom_Utils
    /// </summary>

    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteStateAnimator : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [field:SerializeField] private int StateIndex { get; set; }
        [field:SerializeField] public int MillisecondsDelay { get; set; } = 200;
        
        public bool unscaledTime;
        public SpriteAnimatorState[] states;
        private CancellationTokenSource _cts;

        [Serializable]
        public class SpriteAnimatorState
        {
            public Sprite[] sprites;
            public bool hasExitTime;
            public bool loop;
        }

        private void Awake()
        {
            if (TryGetComponent<SpriteRenderer>(out var component))
                spriteRenderer = component;

            PlayRoutine();
        }

        public SpriteStateAnimator SetState(int index)
        {
            StateIndex = index;
            return this;
        }

        public SpriteStateAnimator SetSpeed(int millisecondsDelay)
        {
            MillisecondsDelay = millisecondsDelay;
            return this;
        }

        public SpriteStateAnimator Stop()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            return this;
        }

        public async UniTaskVoid PlayRoutine(int millisecondsDelay = 200, int stateIndex = 0)
        {
            if(states == null)
                return;
            
            var stateLength = states.Length;
            
            if(stateLength == 0)
                return;
            
            MillisecondsDelay = millisecondsDelay;
            StateIndex = stateIndex;
            
            _cts?.Cancel();
            _cts?.Dispose();
            
            _cts = new CancellationTokenSource();
            
            var token = _cts.Token;

            while (true)
            {
                StateIndex = Mathf.Clamp(StateIndex, 0, stateLength - 1);
                
                var state = states[StateIndex];

                var prevStateIndex = StateIndex;
                var hasExitTime = state.hasExitTime;

                var spriteLength = state.sprites.Length;
                var loop = state.loop;

                int index = 0;

                Func<int, int> calculator = loop ? LoopCalculator : Calculator;

                while (true)
                {
                    if (StateIndex != prevStateIndex)
                    {
                        if (!hasExitTime || index == spriteLength - 1)
                            break;
                    }
                    
                    spriteRenderer.sprite = state.sprites[index];
                    index = calculator.Invoke(index);
                    await UniTask.Delay(MillisecondsDelay, unscaledTime, PlayerLoopTiming.Update, token);
                }
                
                int LoopCalculator(int value) => (value + 1) % spriteLength;
                int Calculator(int value) => Mathf.Clamp(value + 1, 0, spriteLength - 1);
            }
        }


    }
}
