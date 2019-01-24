using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character_Player : Character
{
    public override NavigationController.TURNING GetDesiredTurning(Vector3 p_triggerForwardVector)
    {
        float relativeDot = Vector3.Dot(transform.forward, p_triggerForwardVector);

        float verticalInput = m_currentCharacterInput.m_horizontal;

        if(relativeDot >= 0)//Right is positive on vertical, left is negative
        {
            if (verticalInput > 0)
                return NavigationController.TURNING.RIGHT;
            if (verticalInput < 0)
                return NavigationController.TURNING.LEFT;
        }
        else//Right is negative on vertical, left is positive
        {
            if (verticalInput > 0)
                return NavigationController.TURNING.LEFT;
            if (verticalInput < 0)
                return NavigationController.TURNING.RIGHT;
        }
        return NavigationController.TURNING.CENTER;
    }
}
