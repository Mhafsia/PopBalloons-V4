using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootStepManager : MonoBehaviour {


    private Vector2 previousPosition;
    private Vector2 currentPosition;

    float sumDistance = 0f;

   // Participant player;
    private Camera player;

    public static FootStepManager instance;

	// Use this for initialization
	void Start ()
    {
        //currentPosition = new Vector2();
        currentPosition = Vector2.zero;
       // if (SharingManager.getLocalPlayer() != null)
            initPlayer();
       // else
          // SharingManager.OnLocalPlayerSet += initPlayer;
                
	}

    private void initPlayer()
    {
      //  player = SharingManager.getLocalPlayer();
      //  previousPosition = new Vector2(player.transform.position.x, player.transform.position.z);
    }

    public void Awake()
    {
        player = Camera.main;
        if(instance != null)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }

    // Update is called once per frame
    void Update ()
    {
        if(player != null)
        {
            currentPosition.x = player.transform.position.x;
            currentPosition.y = player.transform.position.z;
            sumDistance += Vector2.Distance(previousPosition, currentPosition);
            previousPosition = currentPosition;
        }
    }


    public void initFootStep()
    {
        sumDistance = 0f;
       // previousPosition = new Vector2(player.transform.position.x, player.transform.position.z);
    }


    public float getDistance()
    {
        return sumDistance;
    }
}
