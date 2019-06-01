using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC_State : MonoBehaviour
{
    public List<NPC_State> m_nextStates = new List<NPC_State>();

    public List<NPC_StateCondition> m_NPCStateConditions = new List<NPC_StateCondition>();

    protected NPC_StateMachine m_parentStateMachine = null;
    protected Character_NPC m_parentNPC = null;
    protected CharacterInput_NPC m_NPCInput = null;

    protected CharacterAnimationController m_characterAnimationController = null;

    //-------------------
    //Initilse the state, runs only once at start
    //-------------------
    public virtual void StateInit()
    {
        m_parentStateMachine = GetComponent<NPC_StateMachine>();
        m_parentNPC = GetComponent<Character_NPC>();

        m_NPCInput = GetComponent<CharacterInput_NPC>();

        m_characterAnimationController = GetComponentInChildren<CharacterAnimationController>();
    }

    //-------------------
    //When swapping to this state, this is called.
    //-------------------
    public virtual void StateStart()
    {

    }

    //-------------------
    //State update, perform any actions for the given state
    //
    //Return bool: Has this state been completed, e.g. Attack has completed, idle would always return true 
    //-------------------
    public virtual bool UpdateState()
    {
        return true;
    }

    //-------------------
    //When swapping to a new state, this is called.
    //-------------------
    public virtual void StateEnd()
    {

    }

    //-------------------
    //Do all of this states preconditions return true
    //
    //Return bool: Is this valid, e.g. Death requires players to have no health
    //-------------------
    public virtual bool IsValid()
    {
        foreach (NPC_StateCondition NPCStateCondition in m_NPCStateConditions)
        {
            if (!NPCStateCondition.Execute(m_parentNPC))
                return false;
        }
        return true;
    }

    //-------------------
    //Adding nexrt states to move to
    //
    //param 
    //      p_NPCState: next possible state
    //-------------------
    public void AddNextState(NPC_State p_NPCState)
    {
        m_nextStates.Add(p_NPCState);
    }

    //-------------------
    //Adding additional conditions to a given state
    //
    //param 
    //      p_NPCStateCondition: Condition to add to check against
    //-------------------
    public void AddCondition(NPC_StateCondition p_NPCStateCondition)
    {
        m_NPCStateConditions.Add(p_NPCStateCondition);
    }
}
