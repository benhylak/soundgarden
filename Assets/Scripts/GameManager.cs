using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bose.Wearable;
using DG.Tweening;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : Singleton<GameManager>
{
//    [CustomEditor(typeof(GameManager))]
//    public class ObjectBuilderEditor : Editor
//    {
//        public override void OnInspectorGUI()
//        {
//            DrawDefaultInspector();
//        
//            GameManager myScript = (GameManager)target;
//            if(GUILayout.Button("Calibrate"))
//            {
//                myScript.CalibrateRotation();
//            }
//        }
//    }
    
    public GameObject calibrationButton;
    public GameObject head;

    public AudioClip calibrationStartSound;
    public AudioClip calibrationInProgressSound;
    public AudioClip calibrationCompleteSound;

    public Image bottomMenu;
    public Image recordButton;

    public GameObject recordingPrefab;
    
    private AudioSource _audioSource;
    
    private State _calibrationState;

    private bool _wasTapped;

    private bool _isRecording = false;

    private string _recordingDeviceName;
    
    private AudioClip _recordedClip;
    private float _recordStartTime;

    public GameObject radioPrefab;
    
    // Start is called before the first frame update
    void Start()
    {
        _audioSource = GetComponent<AudioSource>();
        recordButton.DOFade(0, 0);
        recordButton.gameObject.SetActive(false);
    }

    public void AddRadio()
    {     
        var radio = Instantiate(radioPrefab);
        radio.transform.position = Camera.main.transform.position + Camera.main.transform.forward*0.6f;
        radio.transform.forward = -Camera.main.transform.forward;
    }
    
    public void OnNewSound()
    {
        Debug.Log("On New Sound");
        
        var seq = DOTween.Sequence();       
        
        recordButton.gameObject.SetActive(true);
        
        seq.Append(
            bottomMenu.DOFade(0, 0.2f).SetEase(Ease.OutExpo));

        seq.Append(
            recordButton.DOColor(Color.white, 0.2f).SetEase(Ease.OutExpo));

        seq.Play();
    }

    public void TrimAudioClip(AudioClip clipToTrim)
    {
        var position = Microphone.GetPosition(_recordingDeviceName);
        var soundData = new float[clipToTrim.samples * clipToTrim.channels];
        clipToTrim.GetData (soundData, 0);
 
        //Create shortened array for the data that was used for recording
        var newData = new float[position * clipToTrim.channels];
 
        //Copy the used samples to a new array
        for (int i = 0; i < newData.Length; i++) {
            newData[i] = soundData[i];
        }
 
        //One does not simply shorten an AudioClip,
        //    so we make a new one with the appropriate length
        var newClip = AudioClip.Create (clipToTrim.name,
            position,
            clipToTrim.channels,
            clipToTrim.frequency,
            false);
    
        newClip.SetData (newData, 0);        //Give it the data from the old clip
 
        //Replace the old clip
        Destroy (clipToTrim);
        clipToTrim = newClip;    
    }
    
    public void OnRecordButtonPressed()
    {
        if(recordButton.isActiveAndEnabled == false) return; //shouldn't be in this condition
        
        if (_isRecording == false)
        {
            _recordingDeviceName = Microphone.devices.First();
            
            _recordedClip = Microphone.Start(_recordingDeviceName, true, 5, 44100);
            _recordStartTime = Time.time;
            
            recordButton.DOColor(Color.red, 0.3f);
            _isRecording = true;
        }
        else
        {
            Microphone.End(_recordingDeviceName);

            //rimAudioClip(_recordedClip);

            _isRecording = false;
            
            var recordingOrb = Instantiate(recordingPrefab);
            recordingOrb.transform.position = Camera.main.transform.position + Camera.main.transform.forward;
            recordingOrb.GetComponent<AudioSource>().clip = _recordedClip;
            recordingOrb.GetComponent<AudioSource>().Play();
            
            var seq = DOTween.Sequence();
    
            seq.Append(
                recordButton.DOColor(new Color(0, 0, 0, 0), 0.3f).SetEase(Ease.InExpo)
                    .OnComplete(() => recordButton.gameObject.SetActive(false)));
    
            seq.Append(
                bottomMenu.DOFade(1, 0.3f).SetEase(Ease.InExpo));
        }  
    }

    public Task Play(AudioClip clip)
    {
        _audioSource.clip = clip;
        _audioSource.Play();
        
        return Task.Delay((int)(_audioSource.time*1000f));
    }
    
    // Update is called once per frame
    void Update()
    {
        if (Input.touches.Length > 0 )
        {
            var ray  = Camera.main.ScreenPointToRay(Input.touches[0].position);
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit, 100) &&  hit.transform.gameObject == calibrationButton) {
                CalibrateRotation();
            }
        }

        if (_calibrationState != null)
        {
            _calibrationState = _calibrationState.Update();
        }
    }

    public void CalibrateRotation()
    {
        head.GetComponent<ARRotationMatcher>().SetRelativeReference();
        calibrationButton.GetComponentInChildren<ARRotationMatcher>().SetRelativeReference();
    }

    public void OnHeadphonesTapped()
    {
        if (_calibrationState == null)
        {
            _calibrationState = new CalibrationStates.StartCalibration();
        }
        else _wasTapped = true;
    }

    public bool headphonesWereTapped()
    {
        var returnVal = _wasTapped;
        _wasTapped = false;

        return returnVal;
    }
}
