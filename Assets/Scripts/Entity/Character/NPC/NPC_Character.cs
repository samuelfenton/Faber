using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC_Character : Character
{
    [Header("NPC Logic")]
    public Character m_targetCharacter = null;
    public float m_attackingDistance = 1.0f;
    public float m_detectionDistance = 10.0f;

    [Header("Patrolling")]
    public List<Pathing_Spline> m_patrolSplines = new List<Pathing_Spline>();

    [HideInInspector]
    public List<Pathing_Spline> m_path = new List<Pathing_Spline>();

    private NPC_StateMachine m_NPCStateMachine = null;

    private NPCState_Attack m_attackState = null;

    /// <summary>
    /// Initiliase the entity
    /// setup varible/physics
    /// </summary>
    public override void InitEntity()
    {
        base.InitEntity();

        m_NPCStateMachine = gameObject.AddComponent<NPC_StateMachine>();

        m_NPCStateMachine.InitStateMachine(this);

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if(playerObject!= null)
            m_targetCharacter = playerObject.GetComponent<Character>();

        m_attackState = GetComponent<NPCState_Attack>();
    }

    /// <summary>
    /// Update an entity, this should be called from scene controller
    /// Used to handle different scene state, pause vs in game etc
    /// </summary>
    public override void UpdateEntity()
    {
        base.UpdateEntity();

        m_NPCStateMachine.UpdateStateMachine();
    }

    /// <summary>
    /// Get turning direction for junction navigation, based off current input
    /// </summary>
    /// <param name="p_node">junction entity will pass through</param>
    /// <returns>Path entity will desire to take</returns>
    public override TURNING_DIR GetDesiredTurning(Pathing_Node p_node)
    {
        if(m_path.Count > 0)
        {
            Pathing_Spline desiredSpline = m_path[0];

            if (p_node.m_pathingSplines[(int)Pathing_Spline.SPLINE_POSITION.FORWARD_RIGHT] == desiredSpline)
                return TURNING_DIR.RIGHT;

            if (p_node.m_pathingSplines[(int)Pathing_Spline.SPLINE_POSITION.FORWARD_LEFT] == desiredSpline)
                return TURNING_DIR.LEFT;
        }

        return TURNING_DIR.CENTER;
    }

    /// <summary>
    /// Face towards target character ignoring the y axis
    /// </summary>
    public void FaceTowardsTarget()
    {
        if (m_targetCharacter == null)//Check has target
            return;

        if(Vector3.Dot(Vector3.Normalize(m_targetCharacter.transform.position - transform.position), transform.forward) < 0)//Should I turn to face?
        {
            Vector3 desiredForwards = m_splinePhysics.m_currentSpline.GetForwardDir(m_splinePhysics.m_currentSplinePercent);
            float relativeDot = Vector3.Dot(desiredForwards, transform.forward);
            if (relativeDot > 0)
            {
                m_characterModel.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
            }
            else
            {
                m_characterModel.transform.localRotation = Quaternion.Euler(0.0f, 180.0f, 0.0f);
            }
        }
    }

    #region WEAPON FUNCTIONS - OVERRIDE
    /// <summary>
    /// Function desired to be overridden, should this character be attempting to perform light or heavy attack
    /// Example click by player, or logic for NPC
    /// </summary>
    /// <returns>Light,heavy or none based off logic</returns>
    public override ATTACK_INPUT_STANCE DetermineAttackStance()
    {
        //TODO implement attack logic

        return ATTACK_INPUT_STANCE.LIGHT;
    }
    #endregion
}
