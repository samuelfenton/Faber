using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character_Player : Character
{
    private VoxeliserHandler_Trail_SpeedBased m_voxelHandler = null;

    protected override void Start()
    {
        base.Start();

        m_voxelHandler = GetComponentInChildren<VoxeliserHandler_Trail_SpeedBased>();
    }

    protected override void Update()
    {
        base.Update();

        Vector3 trailDir = new Vector3(0.0f, m_localVelocity.y, -m_localVelocity.x);

        m_voxelHandler.UpdateTrailingVelocity(transform.localToWorldMatrix * trailDir, trailDir.magnitude / m_groundedHorizontalSpeedMax);
    }

    public override NavigationController.TURNING GetDesiredTurning(Vector3 p_triggerForwardVector)
    {
        float relativeDot = Vector3.Dot(transform.forward, p_triggerForwardVector);

        float verticalInput = m_currentCharacterInput.m_vertical;

        if(relativeDot >= 0)//Right is positive on vertical, left is negative
        {
            if (verticalInput < 0)
                return NavigationController.TURNING.RIGHT;
            if (verticalInput > 0)
                return NavigationController.TURNING.LEFT;
        }
        else//Right is negative on vertical, left is positive
        {
            if (verticalInput < 0)
                return NavigationController.TURNING.LEFT;
            if (verticalInput > 0)
                return NavigationController.TURNING.RIGHT;
        }
        return NavigationController.TURNING.CENTER;
    }
}
