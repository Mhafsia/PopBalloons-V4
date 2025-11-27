using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit;
using UnityEngine;
using PopBalloons.Utilities;

namespace PopBalloons.Data
{
    public enum GazableElement { BALLOON, JULIE_BODY, JULIE_HEAD, PANEL, PROPS }
    /// <summary>
    /// Handle Gaze data processing info
    /// </summary>
    public class GazeManager : MonoBehaviour
    {
        

        private static GazeManager instance;

        public static GazeManager Instance { get { return instance; } }

        private void Awake()
        {
            if(Instance != null)
            {
                Destroy(this);
            }
            else
            {
                instance = this;
            }
        }

        public void RegisterGazeItemData(string targetType, string timestamp, float duration, string date, string targetName)
        {
            //TODO: Add object id / position for balloon identification
            GazeItemDatas datas = new GazeItemDatas();
            datas.targetName = targetName;
            datas.objectType = targetType;
            datas.timeOfLook = timestamp;
            datas.duration = duration;

            if (DataManager.instance != null)
                DataManager.instance.AddGazeItemData(datas);
        }

        public IEnumerator RegisterGazeData(float timeDelay,  string targetName)
        {
            yield return new WaitForSeconds(timeDelay);
            GazeDatas data = new GazeDatas();
            data.targetName = targetName;
            data.isCalibrationValid = CoreServices.InputSystem.EyeGazeProvider.IsEyeGazeValid;
            //data.timeStamp = TimerManager.GetTimeStamp();
            data.timeStamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fff");
            data.targetIsValid = CoreServices.InputSystem.EyeGazeProvider.GazeOrigin.IsValidVector();
            data.origin = CoreServices.InputSystem.EyeGazeProvider.GazeOrigin;
            data.direction = CoreServices.InputSystem.EyeGazeProvider.GazeDirection;
            data.eyeGazeTarget = CoreServices.InputSystem.EyeGazeProvider.HitPosition;
             
            if (DataManager.instance != null)
                DataManager.instance.AddGazeData(data);
        }
    }
}