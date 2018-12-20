using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character_Player : Character
{
    private InputController m_inputController = null;

    protected override void Start()
    {
        base.Start();
        m_inputController = m_gameController.m_inputController;
    }

    protected override void Update()
    {
        base.Update();

        m_characterStateMachine.UpdateStateMachine();
    }

    public override void PerformCombo()
    {
        if (!m_inputController.GetKeyInput(InputController.INPUT_KEY.ATTACK, InputController.INPUT_STATE.CURRENT))//We are attempting to attack
            return;

        PlayerState_LightAttack lightAttackState = m_characterStateMachine.m_currentState.GetComponent<PlayerState_LightAttack>();

        if(lightAttackState != null)
        {
            lightAttackState.PerformCombo();
        }
    }

    public override NavigationController.TURNING GetDesiredTurning()
    {
        //Right is negitive on vertical, left is positive
        float verticalInput = m_inputController.GetAxisFloat(InputController.INPUT_AXIS.VERTICAL);
        if (verticalInput > 0)
            return NavigationController.TURNING.LEFT;
        if (verticalInput < 0)
            return NavigationController.TURNING.RIGHT;
        return NavigationController.TURNING.CENTER;
    }

    public override NavigationController.FACING_DIR GetDesiredDirection()
    {
        NavigationController.FACING_DIR currentDir = NavigationController.GetFacingDir(transform.forward);

        return NavigationController.GetTurningDirection(currentDir, GetDesiredTurning());
    }


}
