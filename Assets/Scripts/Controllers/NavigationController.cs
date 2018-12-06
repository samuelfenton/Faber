using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavigationController : MonoBehaviour
{
    public enum FACING_DIR{NORTH = 0, EAST = 1, SOUTH = 2, WEST = 3};

    public static FACING_DIR GetFacingDir(Vector3 p_forwards)
    {
        float angle = Vector3.SignedAngle(Vector3.forward, p_forwards, Vector3.up);
        if (angle < 45 && angle > -45)
            return FACING_DIR.NORTH;
        else if (angle < 135 && angle > 45)
            return FACING_DIR.EAST;
        else if (angle < -45 && angle > -135)
            return FACING_DIR.WEST;
        return FACING_DIR.SOUTH;
    }

    public static Quaternion GetFacingRotation(FACING_DIR p_direction)
    {
        return Quaternion.Euler(0, (int)p_direction * 90, 0);
    }

    public enum TURNING { LEFT = -1, CENTER = 0, RIGHT = 1 }
    public static FACING_DIR GetTurningDirection(FACING_DIR p_currentDir, TURNING p_turningDir)
    {
        int newDir = (int)p_currentDir + (int)p_turningDir;
        switch (newDir)
        {
            case 0:
            case 4:
                return FACING_DIR.NORTH;
            case 1:
                return FACING_DIR.EAST;
            case 2:
                return FACING_DIR.SOUTH;
            case -1:
            case 3:
                return FACING_DIR.WEST;
            default:
                return FACING_DIR.NORTH;
        }
    }
}
