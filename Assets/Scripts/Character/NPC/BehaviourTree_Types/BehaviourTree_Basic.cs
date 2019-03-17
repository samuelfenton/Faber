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

        //AttackingLogic
        CompositeAND attackingLogic = gameObject.AddComponent<CompositeAND>();
        Qualifier_Distance isAttackingDistance = gameObject.AddComponent<Qualifier_Distance>();
        isAttackingDistance.m_closeEnoughDistance = parentNPC.m_attackingDistance;
        isAttackingDistance.InitNode();

        Attacking_SingleAttack singleLightAttack = gameObject.AddComponent <Attacking_SingleAttack>();

        attackingLogic.m_childBehaviours.Add(isAttackingDistance);
        attackingLogic.m_childBehaviours.Add(singleLightAttack);

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

        topBranch.m_childBehaviours.Add(attackingLogic);
        topBranch.m_childBehaviours.Add(pathingLogic);

        m_topNode = topBranch;
    }
}
