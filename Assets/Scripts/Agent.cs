using UnityEngine;
using System.Collections;

public class agent
{
    Rigidbody cart, pole, weight;
    bool isX;
    State state;
    int box;
    AI ai;
    float oldPrediction = 0f;

    public agent(AI ai, Rigidbody cart, Rigidbody pole, Rigidbody weight, float negReward, bool isX)
    {
        Debug.Log("Agent constructor");
        this.cart = cart;
        this.pole = pole;
        this.isX = isX;
        this.ai = ai;
        this.weight = weight;

        if (state == null)
        {
            state = new State();
        }
        state.reset();
        box = state.getBox(ai.max_pos, ai.max_deg);
    }

    public bool applyForce(float time)
    {
        if (update(time))
        {
            return true;
        }

        state.steps++;

        float goRightProb = (1.0F / (1.0F + Mathf.Exp(-max(-50.0F, min(state.action[box], 50.0F)))));

        float actionRight = Random.value < goRightProb ? 1f : 0f;

        state.eAction[box] += (1.0f - ai.lambdaAction) * (actionRight - 0.5f);
        state.eCritic[box] += 1.0f - ai.lambdaCritic;

        oldPrediction = state.critic[box];

        if (actionRight == 0f)
        {
            actionRight = -1f;
        }
        if (isX)
        {
            cart.AddForce(new Vector3(actionRight * ai.force, 0, 0) * time);
        }
        else
        {
            cart.AddForce(new Vector3(0, 0, actionRight * ai.force) * time);
        }
        return false;
    }

    float max(float a, float b)
    {
        return ((a >= b) ? a : b);
    }

    float min(float a, float b)
    {
        return ((a <= b) ? a : b);
    }

    bool update(float time)
    {
        if (isX)
        {
            state.position = cart.position.x;
            state.speed = cart.velocity.x;

            float height = weight.position.y - cart.position.y;
            float x = weight.position.x - cart.position.x;
            state.angle = Mathf.Atan2(x, height) * Mathf.Rad2Deg;
            state.setAngleSpeed(state.angle, time);
        }
        else
        {
            state.position = cart.position.z;
            state.speed = cart.velocity.z;

            float height = weight.position.y - cart.position.y;
            float z = weight.position.z - cart.position.z;
            state.angle = Mathf.Atan2(z, height) * Mathf.Rad2Deg;
            state.setAngleSpeed(state.angle, time);
        }

        box = state.getBox(ai.max_pos, ai.max_deg);

        bool failed = false;
        float reward = 0;
        float fail_prediction = 0;

        if (box < 0)
        {
            failed = true;

            reward = -1f;
            fail_prediction = 0f;
        }
        else
        {
            failed = false;
            reward = 0f;
            fail_prediction = state.critic[box];
        }

        float heuristic = reward + ai.gamma * fail_prediction - oldPrediction;

        for (int i = 0; i < state.action.Length; i++)
        {
            state.action[i] += ai.alpha * heuristic * state.eAction[i];
            state.critic[i] += ai.beta * heuristic * state.eCritic[i];

            if (failed)
            {
                state.eAction[i] = 0;
                state.eCritic[i] = 0;
            }
            else
            {
                state.eAction[i] *= ai.lambdaAction;
                state.eCritic[i] *= ai.lambdaCritic;
            }
        }

        return failed;

    }

    public void saveState(string filename)
    {
        state.reset();
        state.failures++;
        SaveLoad.Save(state, filename);
    }

    public void loadState(string filename)
    {
        state = SaveLoad.Load(filename);
        if (state == null)
        {
            state = new State();
        }
    }

    public void setMaxTime(float time)
    {
        state.maxTime = time;
    }

    public State getState()
    {
        return state;
    }
}
