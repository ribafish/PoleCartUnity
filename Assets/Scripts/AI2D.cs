using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;

public class AI2D : AI
{
    Rigidbody cart;

    agent agent;

    float start_time, maxTime;

    // Use this for initialization
    void Start()
    {
        Debug.Log("AI constructor");
        if (pole == null)
        {
            Debug.Log("Pole is null");
            pole = GameObject.Find("Pole").GetComponent<Rigidbody>();
        }

        cart = GetComponent<Rigidbody>();

        agent = new agent(this, cart, pole, weight, negativeReward, true);

        Time.timeScale = speed;
        agent.loadState("training.data");
        maxTime = agent.getState().maxTime;
    }

    void Update()
    {
        if (Input.GetKeyDown("left"))
        {
            weight.AddForce(new Vector3(-5, 0, 0));
            Debug.Log("Left");
        }
        else if (Input.GetKeyDown("right"))
        {
            weight.AddForce(new Vector3(5, 0, 0));
            Debug.Log("Right");
        }
    }

    // Update at regular time intervals - useful for physics
    void FixedUpdate()
    {
        bool failed = agent.applyForce(Time.deltaTime);

        trialTxt.text = "Trial " + agent.getState().failures.ToString();
        timeTxt.text = "Time: " + Time.timeSinceLevelLoad.ToString("n3") + "s";
        stepsTxt.text = "Steps: " + agent.getState().steps;
        maxTimeTxt.text = "Max time: " + maxTime.ToString("n3") + "s";
        if (Time.timeSinceLevelLoad > maxTime) maxTime = Time.timeSinceLevelLoad;

        if (failed)
        {
            float height = weight.position.y - cart.position.y;
            float x = weight.position.x - cart.position.x;
            float z = weight.position.z - cart.position.z;
            float angX = Mathf.Atan2(x, height) * Mathf.Rad2Deg;

            agent.setMaxTime(maxTime);

            String text = "Trial: " + agent.getState().failures +
                "\tSteps: " + agent.getState().steps +
                "\tTime: " + Time.timeSinceLevelLoad +
                "\tAngle: " + angX +
                "\t Angle velocity: " + agent.getState().angle_speed +
                "\tCart pos: " + cart.position;

            Debug.Log(text);
            LogToFile("PoleCart3D.log", text);

            agent.getState();

            
            agent.saveState("training.data");
            SceneManager.LoadScene(0);
        }

    }

    public void resetSimulation()
    {
        SaveLoad.Save(new State(), "training.data");
        maxTime = 0;
        agent = new agent(this, cart, pole, weight, negativeReward, true);
        SceneManager.LoadScene(0);
    }
}
