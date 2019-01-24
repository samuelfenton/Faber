using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterState : MonoBehaviour
{
    public List<CharacterState> m_nextStates = new List<CharacterState>();
    public CharacterStateMachine.STATE m_stateType = CharacterStateMachine.STATE.IDLE;

    protected CharacterStateMachine m_parentStateMachine = null;
    protected Character m_parentCharacter = null;
    protected CharacterInput m_characterInput = null;

    //-------------------
    //Initilse the state, runs only once at start
    //-------------------
    public virtual void StateInit()
    {
        m_parentStateMachine = GetComponent<CharacterStateMachine>();
        m_parentCharacter = GetComponent<Character>();

        m_characterInput = GetComponent<CharacterInput>();
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
        return false;
    }
}
