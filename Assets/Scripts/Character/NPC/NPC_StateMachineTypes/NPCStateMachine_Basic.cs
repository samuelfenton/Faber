using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCStateMachine_Basic : NPC_StateMachine
{
    public override void InitStateMachine(Character_NPC p_parentNPC)
    {
        m_parentNPC = p_parentNPC;

        //Add all states
        NPCState_Idle NPCIdle = gameObject.AddComponent<NPCState_Idle>();

        NPCState_MoveTowardsTarget NPCMoveTowardsTarget = gameObject.AddComponent<NPCState_MoveTowardsTarget>();
        NPCStateCondition_Distance NPCMoveTowardsTarget_TargetDistance = gameObject.AddComponent<NPCStateCondition_Distance>();
        NPCMoveTowardsTarget_TargetDistance.m_closeEnoughDistance = m_parentNPC.m_detectionDistance;

        //Attacking conditions
        NPCStateCondition_AttackingDistance NPCAttack_CloseEnough = gameObject.AddComponent<NPCStateCondition_AttackingDistance>();
        NPCAttack_CloseEnough.m_closeEnoughDistance = m_parentNPC.m_attackingDistance;
        NPCStateCondition_CanCombo NPCAttack_CanCombo = gameObject.AddComponent<NPCStateCondition_CanCombo>();

        NPCState_SingleAttack NPCSingleAttack = gameObject.AddComponent<NPCState_SingleAttack>();
        NPCState_ComboAttack NPCFirstComboAttack = gameObject.AddComponent<NPCState_ComboAttack>();
        NPCState_ComboAttack NPCSecondComboAttack = gameObject.AddComponent<NPCState_ComboAttack>();

        //Adding in conditions
        NPCMoveTowardsTarget.AddCondition(NPCMoveTowardsTarget_TargetDistance);

        NPCSingleAttack.AddCondition(NPCAttack_CloseEnough);

        NPCFirstComboAttack.AddCondition(NPCAttack_CloseEnough);
        NPCFirstComboAttack.AddCondition(NPCAttack_CanCombo);

        NPCSecondComboAttack.AddCondition(NPCAttack_CloseEnough);
        NPCSecondComboAttack.AddCondition(NPCAttack_CanCombo);

        //Adding state links
        NPCIdle.AddNextState(NPCMoveTowardsTarget);
        NPCIdle.AddNextState(NPCSingleAttack);

        NPCMoveTowardsTarget.AddNextState(NPCSingleAttack);
        NPCMoveTowardsTarget.AddNextState(NPCIdle);

        NPCSingleAttack.AddNextState(NPCFirstComboAttack);
        NPCSingleAttack.AddNextState(NPCMoveTowardsTarget);
        NPCSingleAttack.AddNextState(NPCIdle);

        NPCFirstComboAttack.AddNextState(NPCSecondComboAttack);
        NPCFirstComboAttack.AddNextState(NPCMoveTowardsTarget);
        NPCFirstComboAttack.AddNextState(NPCIdle);

        NPCSecondComboAttack.AddNextState(NPCMoveTowardsTarget);
        NPCSecondComboAttack.AddNextState(NPCIdle);
    }
}
