using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterStateMachine_Basic : CharacterStateMachine
{
    public override void InitStateMachine()
    {
        CharacterState_Idle idle = gameObject.AddComponent<CharacterState_Idle>();
        CharacterState_GroundMovement ground = gameObject.AddComponent<CharacterState_GroundMovement>();

        CharacterState_Jump jump = gameObject.AddComponent<CharacterState_Jump>();
        CharacterState_WallJump wallJump = gameObject.AddComponent<CharacterState_WallJump>();
        CharacterState_InAir inAir = gameObject.AddComponent<CharacterState_InAir>();
        CharacterState_Land land = gameObject.AddComponent<CharacterState_Land>();

        CharacterState_LightAttack lightAttack = gameObject.AddComponent<CharacterState_LightAttack>();

        idle.m_nextStates.Add(lightAttack);
        idle.m_nextStates.Add(jump);
        idle.m_nextStates.Add(ground);
        idle.m_nextStates.Add(inAir);

        ground.m_nextStates.Add(lightAttack);
        ground.m_nextStates.Add(jump);
        ground.m_nextStates.Add(idle);
        ground.m_nextStates.Add(inAir);

        jump.m_nextStates.Add(land);
        jump.m_nextStates.Add(inAir);

        wallJump.m_nextStates.Add(inAir);

        inAir.m_nextStates.Add(wallJump);
        inAir.m_nextStates.Add(land);

        land.m_nextStates.Add(ground);
        land.m_nextStates.Add(idle);

        lightAttack.m_nextStates.Add(ground);
        lightAttack.m_nextStates.Add(idle);
        lightAttack.m_nextStates.Add(inAir);

        m_currentState = inAir;

        base.InitStateMachine();
    }


}
