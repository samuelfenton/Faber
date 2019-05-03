using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BehaviourTree_Basic : BehaviourTree
{
    private void Start()
    {
        Character_NPC parentNPC = GetComponent<Character_NPC>();

        //Top
        CompositeOR topBranch = gameObject.AddComponent<CompositeOR>();

        CompositeAND attackingBranch = gameObject.AddComponent<CompositeAND>();

        //Attacking Logic
        CompositeAND attackingLogic = gameObject.AddComponent<CompositeAND>();

        CompositeAND singleAttackBranch = gameObject.AddComponent<CompositeAND>();
        CompositeAND firstComboAttackBranch = gameObject.AddComponent<CompositeAND>();
        CompositeAND secondComboAttackBranch = gameObject.AddComponent<CompositeAND>();

        Qualifier_Distance isAttackingDistance = gameObject.AddComponent<Qualifier_Distance>();
        isAttackingDistance.m_closeEnoughDistance = parentNPC.m_attackingDistance;
        isAttackingDistance.InitNode();

        Qualifier_CanCombo canCombo = gameObject.AddComponent<Qualifier_CanCombo>();

        Attacking_SingleAttack singleLightAttack = gameObject.AddComponent <Attacking_SingleAttack>();
        Attacking_ComboAttack comboAttack = gameObject.AddComponent<Attacking_ComboAttack>();
        Attack_EndAttack endAttack = gameObject.AddComponent<Attack_EndAttack>();

        singleAttackBranch.m_childBehaviours.Add(isAttackingDistance);
        singleAttackBranch.m_childBehaviours.Add(singleLightAttack);

        firstComboAttackBranch.m_childBehaviours.Add(isAttackingDistance);
        firstComboAttackBranch.m_childBehaviours.Add(canCombo);
        firstComboAttackBranch.m_childBehaviours.Add(comboAttack);

        secondComboAttackBranch.m_childBehaviours.Add(isAttackingDistance);
        secondComboAttackBranch.m_childBehaviours.Add(canCombo);
        secondComboAttackBranch.m_childBehaviours.Add(comboAttack);

        attackingLogic.m_childBehaviours.Add(singleAttackBranch);
        attackingLogic.m_childBehaviours.Add(firstComboAttackBranch);
        attackingLogic.m_childBehaviours.Add(secondComboAttackBranch);
        attackingLogic.m_childBehaviours.Add(endAttack);

        //Attacking Branch
        attackingBranch.m_childBehaviours.Add(attackingLogic);
        attackingBranch.m_childBehaviours.Add(endAttack);

        //Moving Logic
        CompositeAND pathingLogic = gameObject.AddComponent<CompositeAND>();

        Qualifier_Distance isDetectionDistance = gameObject.AddComponent<Qualifier_Distance>();
        isDetectionDistance.m_closeEnoughDistance = parentNPC.m_detectionDistance;
        isDetectionDistance.InitNode();

        CompositeOR hasPathLogic = gameObject.AddComponent<CompositeOR>();

        Pathing_CanMoveTowards canMoveTowards = gameObject.AddComponent<Pathing_CanMoveTowards>();
        Pathing_GetPath getPath = gameObject.AddComponent<Pathing_GetPath>();

        hasPathLogic.m_childBehaviours.Add(canMoveTowards);
        hasPathLogic.m_childBehaviours.Add(getPath);

        Pathing_MoveTowardsTarget moveTowardsTarget = gameObject.AddComponent<Pathing_MoveTowardsTarget>();

        pathingLogic.m_childBehaviours.Add(isDetectionDistance);
        pathingLogic.m_childBehaviours.Add(hasPathLogic);
        pathingLogic.m_childBehaviours.Add(moveTowardsTarget);

        //Top branch
        topBranch.m_childBehaviours.Add(attackingBranch);
        topBranch.m_childBehaviours.Add(pathingLogic);

        m_topNode = topBranch;
    }
}
