using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PopBalloons.Utilities;


namespace PopBalloons.Data
{
    public class BalloonDataRegistrer : MonoBehaviour
    {

        public GameCreator instance;

        BalloonDatas currentBalloonData;


        // Use this for initialization
        void Start()
        {
            
            BalloonBehaviour.OnBalloonSpawned += balloonSpawned;
            BalloonBehaviour.OnDestroyBalloon += balloonDestroyed;
            BalloonBehaviour.OnBalloonMissed += balloonMissed;
            
        }

        void InitBalloonData()
        {
            currentBalloonData = new BalloonDatas();
        }

        private void OnDestroy()
        {
            
            BalloonBehaviour.OnBalloonSpawned -= balloonSpawned;
            BalloonBehaviour.OnDestroyBalloon -= balloonDestroyed;
            BalloonBehaviour.OnBalloonMissed -= balloonMissed;
            
        }

        private void balloonMissed(string timeStamp, float duration, bool timeout, string intendedBalloon, string poppedBalloon)
        {
            currentBalloonData.lifeTime = duration;
            currentBalloonData.balloonWasDestroyByUser = false;
            currentBalloonData.balloonTimout = timeout;
            currentBalloonData.balloonPointGain = 0;
            currentBalloonData.timeOfDestroy = timeStamp;
            //currentBalloonData.distance = (FootStepManager.instance != null)
            //? FootStepManager.instance.getDistance()
            //: 0;
            AddDatas(currentBalloonData);
        }

        private void balloonDestroyed(string timeStamp, float duration, bool isBonus, string intendedBalloon, string poppedBalloon)
        {
            currentBalloonData.lifeTime = duration;
            currentBalloonData.balloonWasDestroyByUser = true;
            currentBalloonData.balloonTimout = false;
            currentBalloonData.balloonPointGain = ScoreManager.instance.scoreToAdd;
            if (GameManager.Instance.CurrentGameType == GameManager.GameType.COGNITIVE)
            {
                currentBalloonData.poppedColor = poppedBalloon;
                currentBalloonData.intendedColor = intendedBalloon;
                //RegisterWavesData(intendedBalloon, nbOptions:0);
            }
            currentBalloonData.timeOfDestroy = timeStamp.ToString();
            currentBalloonData.distance = (FootStepManager.instance != null)
                ? FootStepManager.instance.getDistance()
                : 0;
            AddDatas(currentBalloonData);
        }

        private void balloonSpawned(string timeStamp, Vector3 position)
        {
            InitBalloonData();
            if (FootStepManager.instance != null)
                FootStepManager.instance.initFootStep();
            currentBalloonData.balloonInitialPosition = position;
            currentBalloonData.timeOfSpawn = timeStamp;
            
        }


        private void AddDatas(BalloonDatas data)
        {
            if (DataManager.instance != null)
            {
                DataManager.instance.AddBalloonsDatas(data);
            }
        }

        public static void RegisterWavesData(string intendedColor, List<Options> optionsList)
        {
            Waves waves = new Waves();
            //Options options = new Options();
            waves.intendedColor = intendedColor;
            //waves.nbOption = options.Count();
            foreach (var options in optionsList)
            {
                waves.balloonsOptions.Add(options);
            }

            for (int i = 0; i < 1; i++)
            {
                
            }
            if (DataManager.instance != null)
                DataManager.instance.AddWavesData(waves);
        }

    }

}
