using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using UnityEngine;


public class SumoEnvironment : MonoBehaviour
{
    public SumoAgent trainingAgent; // The agent that is being trained

    public List<GameObject> Other_Agents; // List of all other agents
    public List<SumoAgent> Agents;
    public List<GameObject> StartPositions; // List of all possible start positions



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
        }

        //Add random agents to the enivronment until the amount of agents reaches the amount of start pos
        while (Agents.Count < StartPositions.Count)
        {
            //Pick a random agent
            int randomAgent = Random.Range(0, Other_Agents.Count);

            GameObject newAgent = Instantiate(Other_Agents[randomAgent], StartPositions[Agents.Count].transform.position, Quaternion.identity);

            //Add the agent to the list of agents
            Agents.Add(newAgent.GetComponent<SumoAgent>());
            //Instantiate the agent at a random start position



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
            Agents[i].Environment = this;

        }
    }

    public void Reset(int losingTeam)
    {


        if (losingTeam == 0)
        {
            Debug.Log("Removed 200 points to " + losingTeam);
            Debug.Log("Added 100 points to " + 1);

            m_BlueAgentGroup.AddGroupReward(100);
            m_PurpleAgentGroup.AddGroupReward(-200);
        }
        else
        {
            Debug.Log("Removed 200 points to " + losingTeam);
            Debug.Log("Added 100 points to " + 0);
            m_BlueAgentGroup.AddGroupReward(-200);
            m_PurpleAgentGroup.AddGroupReward(100);
        }
        m_BlueAgentGroup.EndGroupEpisode();
        m_PurpleAgentGroup.EndGroupEpisode();
    }






}
