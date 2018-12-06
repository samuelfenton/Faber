﻿using System.Collections;
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

    public override NavigationController.FACING_DIR GetDesiredDirection()
    {
        //Right is negitive on vertical, left is positive
        NavigationController.FACING_DIR currentDir = NavigationController.GetFacingDir(transform.forward);
        float verticalInput = m_inputController.GetAxisFloat(InputController.INPUT_AXIS.VERTICAL);
        if (verticalInput > 0)
            return NavigationController.GetTurningDirection(currentDir, NavigationController.TURNING.LEFT);
        if (verticalInput < 0)
            return NavigationController.GetTurningDirection(currentDir, NavigationController.TURNING.RIGHT);
        return NavigationController.GetFacingDir(m_characterModel.transform.forward);
    }

}
