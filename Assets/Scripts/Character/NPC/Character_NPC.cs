using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC_Character : Character
{
    [Header("NPC Logic")]
    public List<Navigation_Spline> m_path = new List<Navigation_Spline>();
    public Character m_targetCharacter = null;
    public float m_attackingDistance = 0.0f;
    public float m_detectionDistance = 0.0f;

    private NPC_StateMachine m_NPCStateMachine = null;

    protected override void Start()
    {
        base.Start();

        m_NPCStateMachine = GetComponent<NPC_StateMachine>();

        m_NPCStateMachine.InitStateMachine();
        m_NPCStateMachine.StartStateMachine();
    }

    protected override void Update()
    {
        base.Update();

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
            Navigation_Spline desiredSpline = m_path[0];

            if (p_node.m_forwardLeftSpline == desiredSpline)
                return TURNING_DIR.LEFT;

            if (p_node.m_forwardRightSpline == desiredSpline)
                return TURNING_DIR.RIGHT;
        }

        return TURNING_DIR.CENTER;
    }

    public void FaceTowardsTarget()
    {
        if (m_targetCharacter == null)//Check has target
            return;

        if(Vector3.Dot(Vector3.Normalize(m_targetCharacter.transform.position - transform.position), transform.forward) < 0)//Should I turn to face?
        {
            //Vector3 desiredForwards = m_splinePhysics.m_currentSpline.GetForwardsDir(transform.position);
            //float relativeDot = Vector3.Dot(desiredForwards, transform.forward);
            //if (relativeDot > 0)
            //{
            //    m_characterModel.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
            //}
            //else
            //{
            //    m_characterModel.transform.localRotation = Quaternion.Euler(0.0f, 180.0f, 0.0f);
            //}
        }
    }
}
