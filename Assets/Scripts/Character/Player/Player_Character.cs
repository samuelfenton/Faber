using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Character : Character
{
    [HideInInspector]
    public Player_StateMachine m_playerStateMachine = null;

    public CustomInput m_input = null;

    protected override void Start()
    {
        base.Start();

        m_playerStateMachine = GetComponent<Player_StateMachine>();

        m_input = GetComponent<CustomInput>();

        //Init
        if (m_playerStateMachine != null)
            m_playerStateMachine.InitStateMachine();//Run first as animation depends on states being created

        if (m_playerStateMachine != null)
            m_playerStateMachine.StartStateMachine();//Run intial state
    }

    protected override void Update()
    {
        base.Update();

        m_input.UpdateInput();

        m_playerStateMachine.UpdateStateMachine();
    }

    /// <summary>
    /// Get turning direction for junction navigation, based off current input
    /// </summary>
    /// <param name="p_node">junction entity will pass through</param>
    /// <returns>Path entity will desire to take</returns>
    public override TURNING_DIR GetDesiredTurning(Pathing_Node p_node)
    {
        float relativeDot = Vector3.Dot(transform.forward, p_node.transform.forward);

        float verticalInput = m_input.GetAxis(CustomInput.INPUT_AXIS.VERTICAL);

        if(relativeDot >= 0)//Right is positive on vertical, left is negative
        {
            if (verticalInput < 0)
                return TURNING_DIR.RIGHT;
            if (verticalInput > 0)
                return TURNING_DIR.LEFT;
        }
        else//Right is negative on vertical, left is positive
        {
            if (verticalInput < 0)
                return TURNING_DIR.LEFT;
            if (verticalInput > 0)
                return TURNING_DIR.RIGHT;
        }
        return TURNING_DIR.CENTER;
    }
}
