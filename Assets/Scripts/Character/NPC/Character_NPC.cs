using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character_NPC : Character
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

        m_NPCStateMachine.InitStateMachine(this);
        m_NPCStateMachine.InitStates();
    }

    protected override void Update()
    {
        base.Update();

        m_NPCStateMachine.UpdateStateMachine();
    }

    public override NavigationController.TURNING GetDesiredTurning(Navigation_Trigger_Junction p_trigger)
    {
        if(m_path.Count > 0)
        {
            Navigation_Spline desiredSpline = m_path[0];

            if (p_trigger.m_forwardLeftSplineInfo.m_spline == desiredSpline)
                return NavigationController.TURNING.LEFT;

            if (p_trigger.m_forwardRightSplineInfo.m_spline == desiredSpline)
                return NavigationController.TURNING.RIGHT;
        }

        return NavigationController.TURNING.CENTER;
    }

    public void FaceTowardsTarget()
    {
        if (m_targetCharacter == null)//Check has target
            return;

        if(Vector3.Dot(Vector3.Normalize(m_targetCharacter.transform.position - transform.position), transform.forward) < 0)//Should I turn to face?
        {
            Vector3 desiredForwards = m_characterCustomPhysics.m_currentSpline.GetForwardsDir(transform.position);
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
}
