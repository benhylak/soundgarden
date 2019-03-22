using System;
using UnityEngine;

public abstract class State
{
    public abstract State Update();
    
    //Makes sure that a function (toEvaluate) returns true for a minimum of a certain time period.  
    // if it does, state -> toProceed
    // if it doesn't, state -> toReturn
    
    public class BufferState : State
    {
        float _startTime;

        private readonly State _toReturn;
        private readonly Func<State> _toProceed;
        private readonly Func<bool> _toEvaluate;
        private readonly float _bufferTime;
        
        public BufferState(State toReturn, Func<State> toProceed, Func<bool> toEvaluate, float bufferTime)
        {
            _toReturn = toReturn;
            _toProceed = toProceed;
            _toEvaluate = toEvaluate;
            _bufferTime = bufferTime;
            
            _startTime = Time.time;
        }

        public override State Update()
        {
            if (!_toEvaluate())
            {
                return _toReturn;
            }
            else if (Time.time - _startTime > _bufferTime)
            {
                return _toProceed();
            }
            else
            {
                return null;
            }
        }
    }
}