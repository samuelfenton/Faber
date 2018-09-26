using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Character
{
    protected override void Start()
    {
        base.Start();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        m_characterStateMachine.UpdateStateMachine();
    }

}
