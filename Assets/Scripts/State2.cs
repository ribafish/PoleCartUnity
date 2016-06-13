using UnityEngine;
using System.Collections;


[System.Serializable]
public class State2
{
    public float position = 0;
    public float speed = 0;
    public float angle = 0;
    float last_angle = 0;
    public float angle_speed = 0;

    public int steps = 0;
    public int failures = 0;

    public float[] action = new float[384];    // Array of action weights
    public float[] critic = new float[384];    // Array of critic weights
    public float[] eAction = new float[384];    // Array of action weights eligibilities
    public float[] eCritic = new float[384];    // Array of critic weights eligibilities

    public float maxTime = 0;

    public State2()
    {
        Debug.Log("State constructor");
    }

    public void reset()
    {
        position = 0;
        speed = 0;
        angle = 0;
        last_angle = 0;
        angle_speed = 0;
        steps = 0;
    }

    public int getBox(float max_x, float max_deg)
    {
        int box = 0;

        if (position < -max_x ||
            position > max_x ||
            angle < -max_deg ||
            angle > max_deg) return (-1); // to signal failure

        if (position < -max_x * 0.5F) box = 0;
        else if (position < 0) box = 1;
        else if (position < max_x * 0.5F) box = 2;
        else box = 3;

        if (speed < -4F) ;              // lower than -4
        else if (speed < 0) box += 4;   // -4 to 0
        else if (speed < 4F) box += 8;  // 0 to -4
        else box += 12;                 // higher than 4

        if (angle < -max_deg / 2) ;
        else if (angle < -max_deg / 12) box += 16;
        else if (angle < 0) box += 32;
        else if (angle < max_deg / 12) box += 48;
        else if (angle < max_deg / 2) box += 64;
        else box += 80;

        if (angle_speed < -50) ;
        else if (angle_speed < 0) box += 96;
        else if (angle_speed < 50) box += 192;
        else box += 288;

        return (box);
    }

    public void setAngleSpeed(float angle, float time)
    {
        angle_speed = (angle - last_angle) / time;

        last_angle = angle;
    }
}
