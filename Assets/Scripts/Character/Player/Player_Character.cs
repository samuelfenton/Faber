using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Character : Character
{
    protected Player_StateMachine m_playerStateMachine = null;
    [HideInInspector]
    public CustomInput m_input = null;

    /// <summary>
    /// Initiliase the entity
    /// setup varible/physics
    /// </summary>
    public override void InitEntity()
    {
        base.InitEntity();

        m_input = gameObject.AddComponent<CustomInput>();

        //Init
        m_playerStateMachine = gameObject.AddComponent<Player_StateMachine>();

        m_playerStateMachine.InitStateMachine(this);//Run first as animation depends on states being created
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

        float verticalInput = m_input.GetAxis(CustomInput.INPUT_AXIS.DEPTH);

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
