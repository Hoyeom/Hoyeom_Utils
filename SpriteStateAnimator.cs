using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;

namespace Main.Utils.Component
{

    
    public class SpriteStateAnimator : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [field:SerializeField] private int StateIndex { get; set; }
        [field:SerializeField] public int MillisecondsDelay { get; set; } = 200;
        
        public bool unscaledTime;
        public SpriteAnimatorState[] states;
        private CancellationTokenSource cts;

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

        public async UniTaskVoid PlayRoutine(int millisecondsDelay = 200, int stateIndex = 0)
        {
            if(states == null)
                return;
            
            var stateLength = states.Length;
            
            if(stateLength == 0)
                return;



            MillisecondsDelay = millisecondsDelay;
            StateIndex = stateIndex;
            
            cts?.Cancel();
            cts?.Dispose();
            
            cts = new CancellationTokenSource();
            
            var token = cts.Token;

            while (stateLength > StateIndex)
            {
                var state = states[StateIndex];

                var prevStateIndex = StateIndex;
                var hasExitTime = state.hasExitTime;
                var exit = true;

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
            
            cts?.Cancel();
            cts?.Dispose();
        }


    }
}
