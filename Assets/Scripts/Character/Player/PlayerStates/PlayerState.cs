using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState : CharacterState
{
    protected Character_Player m_parentCharacter = null;
    protected InputController m_inputController = null;

    //-------------------
    //Initilse the state, runs only once at start
    //-------------------
    public override void StateInit()
    {
        m_parentCharacter = GetComponent<Character_Player>();
        m_inputController = m_parentCharacter.m_gameController.m_inputController;
    }
}
