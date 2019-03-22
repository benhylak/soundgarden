
using UnityEngine;

public static class CalibrationStates
{
    public class StartCalibration : State
    {
        public StartCalibration()
        {
            GameManager.Instance.Play(GameManager.Instance.calibrationStartSound);
            //play noise to start
        }
        
        public override State Update()
        {          
            //someday, will handle saving calibrations from pocket to view mode.
            
            GameManager.Instance.head.transform.parent = null;
            GameManager.Instance.head.transform.position = Camera.main.transform.position;
            
            GameManager.Instance.CalibrateRotation();
            
            return new MovingToPocket();
        }
    }

    public class MovingToPocket : State
    {
        private float _lastPlayedTime = 0;

        public MovingToPocket()
        {
            _lastPlayedTime = Time.time + 0.3f; //delay the first beep a bit
        }
        
        public override State Update()
        {
            if (GameManager.Instance.headphonesWereTapped())
            {
                return new CalibrationComplete();
            }
            else if (Time.time - _lastPlayedTime > 1f)
            {
                _lastPlayedTime = Time.time;
                GameManager.Instance.Play(GameManager.Instance.calibrationInProgressSound);
            }

            return this;
        }
    }

    public class CalibrationComplete : State
    {
        public CalibrationComplete()
        {
            GameManager.Instance.Play(GameManager.Instance.calibrationCompleteSound);
        }

        public override State Update()
        {
            //play noise
            GameManager.Instance.head.transform.parent = Camera.main.transform;
                      
            return null;
        }
    }
    
}
