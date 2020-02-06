using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotatePlatformStepwise : MonoBehaviour
{
    public Vector3 axis = new Vector3(0, 0, 0);
    public int rotationTime = 90;
    public int pauseTime = 120;
    public int vibrateTime = 45;

    private int rotationTimer;
    private int pauseTimer;
    private int vibrateTimer;

    private Vector3 localEuler;
    private int vibrateDirection = 1;

    private State state = State.ROTATING;
    // Start is called before the first frame update
    void Start()
    {
        localEuler = transform.localEulerAngles;
        rotationTimer = rotationTime;
        pauseTimer = pauseTime;
        vibrateTimer = vibrateTime;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // we want to rotate the platform 90 degrees each time but have a small break between
        // with a little vibration indicating the platform is about to rotate again
        if(state == State.ROTATING)
        {
            rotationTimer--;
            transform.localEulerAngles = Vector3.Lerp(localEuler + axis * 90, localEuler, (float)rotationTimer/rotationTime);
            
            if(rotationTimer <= 0)
            {
                state = State.PAUSED;
                localEuler = transform.localEulerAngles;
                pauseTimer = pauseTime;
            }
        }
        else if (state == State.PAUSED)
        {
            pauseTimer--;
            if (pauseTimer <= 0)
            {
                state = State.VIBRATING;
                vibrateTimer = vibrateTime;
            }
        }
        else if (state == State.VIBRATING)
        {

            transform.Rotate(axis * vibrateDirection, Space.Self);
            if (vibrateTimer % 3 == 1) {
                vibrateDirection *= -1;
            }
            vibrateTimer--;


            if(vibrateTimer <= 0)
            {
                state = State.ROTATING;
                rotationTimer = rotationTime;
            }
        }
    }

    private enum State
    {
        ROTATING,
        PAUSED,
        VIBRATING
    }
}
