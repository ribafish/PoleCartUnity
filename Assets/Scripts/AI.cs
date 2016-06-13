using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;
using System;

public class AI : MonoBehaviour {
    // Constants
    public float max_pos = 2.5F;
    public float max_deg = 15F;

    [Range(10f, 10000f)] public float alpha = 1000F;     // Learning rate for action weights, a
    [Range(0f, 1.0f)] public float beta = 0.5F;       // Learning rate for critic weights, c
    [Range(0f, 1.0f)] public float gamma = 0.95F;     // Discount factor for critic
    [Range(0f, 1.0f)] public float lambdaAction = 0.9F;    // Decay rate for a eligibilities
    [Range(0f, 1.0f)] public float lambdaCritic = 0.8F;    // Decay rate for c eligibilities

    public float force = 1500f;
    public float speed = 1f;
    public float negativeReward = -1;

    public Rigidbody pole;
    public Rigidbody weight;
    public Text trialTxt;
    public Text timeTxt;
    public Text stepsTxt;
    public Text maxTimeTxt;

    Rigidbody cart;

    agent agentX, agentZ;

    float start_time, maxTime;

    // Use this for initialization
    void Start () {
        Debug.Log("AI constructor");
        if (pole == null)
        {
            Debug.Log("Pole is null");
            pole = GameObject.Find("Pole").GetComponent<Rigidbody>();
        }

        cart = GetComponent<Rigidbody>();

        agentX = new agent(this, cart, pole, weight, negativeReward, true);
        agentZ = new agent(this, cart, pole, weight, negativeReward, false);

        agentX.loadState("trainingX.data");
        agentZ.loadState("trainingZ.data");

        Time.timeScale = speed;
        maxTime = agentX.getState().maxTime;
        if (agentZ.getState().maxTime > maxTime) maxTime = agentZ.getState().maxTime;
    }

    // Update at regular time intervals - useful for physics
    void FixedUpdate()
    {
        bool failedX = agentX.applyForce(Time.deltaTime);
        bool failedZ = agentZ.applyForce(Time.deltaTime);

        int failures = agentX.getState().failures + agentZ.getState().failures;

        trialTxt.text = "Trial " + failures;
        timeTxt.text = "Time: " + Time.timeSinceLevelLoad.ToString("n3") + "s";
        stepsTxt.text = "Steps: " + agentX.getState().steps;
        maxTimeTxt.text = "Max time: " + maxTime.ToString("n3") + "s";
        if (Time.timeSinceLevelLoad > maxTime) maxTime = Time.timeSinceLevelLoad;

        if (failedX || failedZ)
        {
            agentX.setMaxTime(maxTime);
            agentZ.setMaxTime(maxTime);

            String text = "Trial: " + failures +
                "\tSteps: " + agentX.getState().steps + 
                "\tTime: " + Time.timeSinceLevelLoad.ToString("n3") + "s" +
                "\tAngle: (" + agentX.getState().angle + ", " + agentZ.getState().angle + ")" +
                "\t Angle velocity: (" + agentX.getState().angle_speed + ", " + agentZ.getState().angle_speed + ")" +
                "\tCart pos: " + cart.position;

            Debug.Log(text);
            LogToFile("PoleCart3D.log", text);

            if (failedX) agentX.saveState("trainingX.data");
            else agentZ.saveState("trainingZ.data");
            SceneManager.LoadScene(0);
        }
         
    }

    public void resetSimulation(string logFileName)
    {
        SaveLoad.Save(new State(), "trainingX.data");
        SaveLoad.Save(new State(), "trainingZ.data");
        maxTime = 0;
        agentX = new agent(this, cart, pole, weight, negativeReward, true);
        agentZ = new agent(this, cart, pole, weight, negativeReward, false);
        File.CreateText(Application.persistentDataPath + "/" + logFileName);
        SceneManager.LoadScene(0);
    }

    public void LogToFile(string logFileName, string text)
    {
        try
        {
            string path = Application.persistentDataPath + "/" + logFileName;
            if (!File.Exists(path))
            {
                File.CreateText(path);
            }
            using (StreamWriter sw = File.AppendText(path))
            {
                sw.WriteLine(text);
            }
        }
        catch (Exception e)
        {

        }
    }
}
