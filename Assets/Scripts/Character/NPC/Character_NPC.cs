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

    //-------------------
    //Character setup
    //  Ensure all need componets are attached, and get initilised if needed
    //  Setup NPC state machine for input control
    //-------------------
    protected override void Start()
    {
        base.Start();

        m_NPCStateMachine = GetComponent<NPC_StateMachine>();

        m_NPCStateMachine.InitStateMachine(this);
        m_NPCStateMachine.InitStates();
        m_NPCStateMachine.StartStateMachine();
    }

    //-------------------
    //Character update
    //  Get input, apply physics, update character state machine
    //  Update NPC state machine for inputs
    //-------------------
    protected override void Update()
    {
        base.Update();

        m_NPCStateMachine.UpdateStateMachine();
    }

    //-------------------
    //Get turning direction for junction navigation, based off current input
    //  Rather than using input, desired path will be based off generated path to player
    //
    //Param p_trigger: junction character will pass through
    //
    //Return NavigationController.TURNING: Path character will desire to take
    //-------------------
    public override TURNING_DIR GetDesiredTurning(Navigation_Trigger_Junction p_trigger)
    {
        if(m_path.Count > 0)
        {
            Navigation_Spline desiredSpline = m_path[0];

            if (p_trigger.m_forwardLeftSplineInfo.m_spline == desiredSpline)
                return TURNING_DIR.LEFT;

            if (p_trigger.m_forwardRightSplineInfo.m_spline == desiredSpline)
                return TURNING_DIR.RIGHT;
        }

        return TURNING_DIR.CENTER;
    }

    //-------------------
    //Face NPC to look towards target when movement isnt needed
    //-------------------
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
