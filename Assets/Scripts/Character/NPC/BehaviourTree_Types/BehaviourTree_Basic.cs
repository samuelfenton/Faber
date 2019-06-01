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

        //Attacking Logic
        CompositeAND attackingLogic = gameObject.AddComponent<CompositeAND>();

        CompositeOR singleAttackLogic = gameObject.AddComponent<CompositeOR>();
        CompositeOR firstComboAttackLogic = gameObject.AddComponent<CompositeOR>();
        CompositeOR secondComboAttackLogic = gameObject.AddComponent<CompositeOR>();

        CompositeAND singleAttackBranch = gameObject.AddComponent<CompositeAND>();
        CompositeAND firstComboAttackBranch = gameObject.AddComponent<CompositeAND>();
        CompositeAND secondComboAttackBranch = gameObject.AddComponent<CompositeAND>();

        Qualifier_AttackingDistance isAttackingDistance = gameObject.AddComponent<Qualifier_AttackingDistance>();
        isAttackingDistance.m_closeEnoughDistance = parentNPC.m_attackingDistance;

        Qualifier_CanCombo canCombo = gameObject.AddComponent<Qualifier_CanCombo>();

        Attacking_SingleAttack singleLightAttack = gameObject.AddComponent <Attacking_SingleAttack>();
        Attacking_ComboAttack comboAttack = gameObject.AddComponent<Attacking_ComboAttack>();
        Attack_EndAttack endAttack = gameObject.AddComponent<Attack_EndAttack>();

        singleAttackBranch.m_childBehaviours.Add(singleLightAttack);
        singleAttackBranch.m_childBehaviours.Add(isAttackingDistance);

        firstComboAttackBranch.m_childBehaviours.Add(comboAttack);
        firstComboAttackBranch.m_childBehaviours.Add(isAttackingDistance);

        secondComboAttackBranch.m_childBehaviours.Add(comboAttack);

        //Attacking Branch
        singleAttackLogic.m_childBehaviours.Add(singleAttackBranch);
        singleAttackLogic.m_childBehaviours.Add(endAttack);

        firstComboAttackLogic.m_childBehaviours.Add(firstComboAttackBranch);
        firstComboAttackLogic.m_childBehaviours.Add(endAttack);

        secondComboAttackLogic.m_childBehaviours.Add(secondComboAttackBranch);
        secondComboAttackLogic.m_childBehaviours.Add(endAttack);

        attackingLogic.m_childBehaviours.Add(isAttackingDistance);
        attackingLogic.m_childBehaviours.Add(singleAttackLogic);
        attackingLogic.m_childBehaviours.Add(firstComboAttackLogic);
        attackingLogic.m_childBehaviours.Add(secondComboAttackLogic);

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
        topBranch.m_childBehaviours.Add(attackingLogic);
        topBranch.m_childBehaviours.Add(pathingLogic);

        m_topNode = topBranch;
    }
}
