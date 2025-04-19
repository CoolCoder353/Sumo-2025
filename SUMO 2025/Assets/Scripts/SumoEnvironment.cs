using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using UnityEngine;


public class SumoEnvironment : MonoBehaviour
{
    public SumoAgent trainingAgent; // The agent that is being trained

    public List<GameObject> Other_Agents; // List of all other agents
    public List<SumoAgent> Agents;

    public List<GameObject> SpawnedBots; // List of all hard coded bots

    public List<GameObject> StartPositions; // List of all possible start positions

    public int chanceForHardCodedRandomAgent = 10; // The chance for a random agent to be added to the environment

    public List<GameObject> possibleHardBots; // List of all possible hard bots

    private SimpleMultiAgentGroup m_BlueAgentGroup;
    private SimpleMultiAgentGroup m_PurpleAgentGroup;
    // Start is called before the first frame update
    void Start()
    {

        m_BlueAgentGroup = new SimpleMultiAgentGroup();
        m_PurpleAgentGroup = new SimpleMultiAgentGroup();

        if (Agents.Count == 0)
        {
            Agents.Add(trainingAgent);
            trainingAgent.Environment = this;
        }

        PickRandomBots();


        //        //Add random agents to the enivronment until the amount of agents reaches the amount of start pos
        // while (Agents.Count < StartPositions.Count)
        // {
        //     //Pick a random agent
        //     int randomAgent = Random.Range(0, Other_Agents.Count);

        //     GameObject newAgent = Instantiate(Other_Agents[randomAgent], StartPositions[Agents.Count].transform.position, Quaternion.identity);

        //     //Add the agent to the list of agents

        //     Agents.Add(newAgent.GetComponent<SumoAgent>());
        //     //Instantiate the agent at a random start position



        // }

        // for (int i = 0; i < Agents.Count; i++)
        // {
        //     if (i % 2 == 0)
        //     {
        //         Agents[i].team = 0;
        //         Agents[i].startingPos = StartPositions[0].transform.position;
        //         m_BlueAgentGroup.RegisterAgent(Agents[i]);
        //     }
        //     else
        //     {
        //         Agents[i].team = 1;
        //         Agents[i].startingPos = StartPositions[1].transform.position;
        //         m_PurpleAgentGroup.RegisterAgent(Agents[i]);
        //     }
        //     Agents[i].Environment = this;

        // }

    }

    private void ResetBots()
    {
        Debug.Log("Resetting bots");

        // Destroy all spawned bots except the training bot
        for (int i = SpawnedBots.Count - 1; i >= 0; i--)
        {
            if (SpawnedBots[i] != trainingAgent.gameObject)
            {
                Destroy(SpawnedBots[i]);
                SpawnedBots.RemoveAt(i);
            }
        }

        // Clear all agents except the training bot
        for (int i = Agents.Count - 1; i >= 0; i--)
        {
            if (Agents[i] != trainingAgent)
            {
                Agents.RemoveAt(i);
            }
        }

        // Ensure the training bot is in the agents list
        if (!Agents.Contains(trainingAgent))
        {
            Agents.Add(trainingAgent);
        }
    }
    private void PickRandomBots()
    {
        bool hardBot = Random.Range(0, 100) < chanceForHardCodedRandomAgent;

        if (hardBot)
        {
            PickRandomHardBot();
        }
        else
        {
            PickMLBot();
        }

        for (int i = 0; i < Agents.Count; i++)
        {
            if (i % 2 == 0)
            {
                Agents[i].team = 0;
                Agents[i].startingPos = StartPositions[0].transform.position;
                m_BlueAgentGroup.RegisterAgent(Agents[i]);
            }
            else
            {
                Agents[i].team = 1;
                Agents[i].startingPos = StartPositions[1].transform.position;
                m_PurpleAgentGroup.RegisterAgent(Agents[i]);
            }

            // Debug.Log($"Agent {i} team: {Agents[i].team}, startingPos: {Agents[i].startingPos}");
            Agents[i].Environment = this;

            if (!SpawnedBots.Contains(Agents[i].gameObject))
            {
                SpawnedBots.Add(Agents[i].gameObject);
            }


        }

    }
    private void PickMLBot()
    {
        //Add random agents to the enivronment until the amount of agents reaches the amount of start pos
        while (Agents.Count < StartPositions.Count)
        {
            // Debug.Log($"Adding random agent to the environment. Current count: {Agents.Count}, Start positions: {StartPositions.Count}");
            //Pick a random agent
            int randomAgent = Random.Range(0, Other_Agents.Count);

            GameObject newAgent = Instantiate(Other_Agents[randomAgent], StartPositions[Agents.Count].transform.position, Quaternion.identity);

            //Add the agent to the list of agents

            Agents.Add(newAgent.GetComponent<SumoAgent>());
            //Instantiate the agent at a random start position
        }

    }

    private void PickRandomHardBot()
    {
        while (SpawnedBots.Count < StartPositions.Count)
        {
            // Debug.Log($"Adding random hard bot to the environment. Current count: {Agents.Count}, Start positions: {StartPositions.Count}");
            //Pick a random agent
            int randomAgent = Random.Range(0, possibleHardBots.Count);

            GameObject newAgent = Instantiate(possibleHardBots[randomAgent], StartPositions[Agents.Count].transform.position, Quaternion.identity);

            SpawnedBots.Add(newAgent);
            //Dont add the bot to the list of agents cause it will run on its own.
        }

        Agents[0].team = 0;
        Agents[0].startingPos = StartPositions[0].transform.position;
        m_BlueAgentGroup.RegisterAgent(Agents[0]);

    }

    public void Reset(int losingTeam)
    {


        if (losingTeam == 0)
        {
            //// Debug.Log("Removed 200 points to " + losingTeam);
            ////Debug.Log("Added 100 points to " + 1);

            m_BlueAgentGroup.AddGroupReward(1000);
            m_PurpleAgentGroup.AddGroupReward(-200);
        }
        else
        {
            //// Debug.Log("Removed 200 points to " + losingTeam);
            //// Debug.Log("Added 100 points to " + 0);
            m_BlueAgentGroup.AddGroupReward(-200);
            m_PurpleAgentGroup.AddGroupReward(1000);
        }
        m_BlueAgentGroup.EndGroupEpisode();
        m_PurpleAgentGroup.EndGroupEpisode();

        ResetBots();
        PickRandomBots();
    }






}
