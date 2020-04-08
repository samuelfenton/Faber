using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    public ManoeuvreData m_weaponData = null;

    public GameObject m_primaryWeaponObject = null;
    public GameObject m_secondaryWeaponObject = null;

    protected Character m_character = null;
    protected Animator m_animator = null;

    //Collision
    protected Dictionary<Character, float> m_collidingCharacters = new Dictionary<Character, float>();

    //Attacking sequence
    protected AnimController.ATTACK_TYPE m_attackType = AnimController.ATTACK_TYPE.GROUND;
    protected AnimController.ATTACK_STANCE m_attackStance = AnimController.ATTACK_STANCE.LIGHT;
    protected int m_manoeuvreIndex = 0; //Combo position, 0 is the first position and does not require the combo flag, goes up to 3

    //Attacking manoeurve
    protected enum ATTACK_MANOEUVRE_STATE { WIND_UP, DAMAGE, AWAITING_COMBO, COMBO_CHECK, END_ATTACK }
    protected ATTACK_MANOEUVRE_STATE m_currentState = ATTACK_MANOEUVRE_STATE.WIND_UP;

    protected bool m_comboFlag = false;
    protected float m_previousTranslation = 0.0f;

    protected float m_startDamage = 0.0f;
    protected float m_endDamage = 0.0f;
    protected float m_startCombo = 0.0f;
    protected float m_endCombo = 0.0f;

    protected AnimationCurve m_translationCurve;

    protected WeaponTrigger m_primaryWeaponScript = null;
    protected WeaponTrigger m_secondaryWeaponScript = null;

    /// <summary>
    /// Init manager
    /// </summary>
    /// <param name="p_character">Character that uses this manager</param>
    public virtual void Init(Character p_character)
    {
        m_character = p_character;
        m_animator = m_character.GetComponentInChildren<Animator>();

        if (m_primaryWeaponObject != null)
        {
            m_primaryWeaponScript = m_primaryWeaponObject.GetComponent<WeaponTrigger>();
            m_primaryWeaponObject.transform.SetParent(m_character.m_rightHand.transform, false);
        }
        if (m_secondaryWeaponObject != null)
        {
            m_secondaryWeaponScript = m_secondaryWeaponObject.GetComponent<WeaponTrigger>();
            m_secondaryWeaponObject.transform.SetParent(m_character.m_leftHand.transform, false);
        }

        if (m_primaryWeaponScript != null)
        {
            m_primaryWeaponScript.Init(this);
            m_primaryWeaponScript.ToggleTrigger(false);
        }
        if (m_secondaryWeaponScript != null)
        {
            m_secondaryWeaponScript.Init(this);
            m_secondaryWeaponScript.ToggleTrigger(false);
        }
    }

    #region ATTACK SEQUENCE

    /// <summary>
    /// Start of the attack sequence
    /// </summary>
    public void StartAttackSequence()
    {
        m_comboFlag = false;

        m_attackType = DetermineAttackType();
        m_attackStance = DetermineStance();
        m_manoeuvreIndex = 0;

        StartAttackManoeuvre();

        m_character.HardSetVelocity(0.0f);
    }

    /// <summary>
    /// Update the attack sequence
    /// </summary>
    /// <returns>true when completed</returns>
    public bool UpdateAttackSequence()
    {
        if (UpdateAttackManoeuvre())//current manoeuvre completed
        {
            EndAttackManoeuvre();

            if (!m_comboFlag) //No attempt to combo
                return true;

            return !StartAttackManoeuvre(); //Invalid next manoeuvre, stop here
        }

        return false;
    }

    #endregion

    #region ATTACK MANOEUVRE

    /// <summary>
    /// Start of attackin manoeurve
    /// </summary>
    /// <returns>flase when invalid manoeuvre</returns>
    private bool StartAttackManoeuvre()
    {
        m_character.m_gravity = false;
        m_character.HardSetUpwardsVelocity(0.0f);

        //Toggle Colliders
        if (m_primaryWeaponScript != null)
            m_primaryWeaponScript.ToggleTrigger(true);
        if (m_secondaryWeaponScript != null)
            m_secondaryWeaponScript.ToggleTrigger(true);

        ManoeuvreData.AttackDetails attackDetails;

        m_attackType = DetermineAttackType();

        //Determine attack details
        switch (m_attackType)
        {
            case AnimController.ATTACK_TYPE.GROUND:
                if(m_attackStance == AnimController.ATTACK_STANCE.LIGHT)
                {
                    if (m_weaponData.m_groundLight.Length <= m_manoeuvreIndex)
                        return false;
                    attackDetails = m_weaponData.m_groundLight[m_manoeuvreIndex];
                }
                else
                {
                    if (m_weaponData.m_groundHeavy.Length <= m_manoeuvreIndex)
                        return false;
                    attackDetails = m_weaponData.m_groundHeavy[m_manoeuvreIndex];
                }
                break;
            case AnimController.ATTACK_TYPE.IN_AIR:
                if (m_attackStance == AnimController.ATTACK_STANCE.LIGHT)
                {
                    if (m_weaponData.m_inAirLight.Length <= m_manoeuvreIndex)
                        return false;
                    attackDetails = m_weaponData.m_inAirLight[m_manoeuvreIndex];
                }
                else
                {
                    if (m_weaponData.m_inAirHeavy.Length <= m_manoeuvreIndex)
                        return false;
                    attackDetails = m_weaponData.m_inAirHeavy[m_manoeuvreIndex];
                }
                break;
            case AnimController.ATTACK_TYPE.SPRINTING:
                if (m_attackStance == AnimController.ATTACK_STANCE.LIGHT)
                {
                    if (m_weaponData.m_sprintLight.Length <= m_manoeuvreIndex)
                        return false;
                    attackDetails = m_weaponData.m_sprintLight[m_manoeuvreIndex];
                }
                else
                {
                    if (m_weaponData.m_sprintHeavy.Length <= m_manoeuvreIndex)
                        return false;
                    attackDetails = m_weaponData.m_sprintHeavy[m_manoeuvreIndex];
                }
                break;
            default:
                return false;
        }

        m_comboFlag = false;
        m_previousTranslation = 0.0f;

        m_startDamage = attackDetails.m_damageStart;
        m_endDamage = attackDetails.m_damageEnd;
        m_startCombo = attackDetails.m_comboStart;
        m_endCombo = attackDetails.m_comboEnd;
        m_translationCurve = attackDetails.m_translationCurve;

        m_currentState = ATTACK_MANOEUVRE_STATE.WIND_UP;

        AnimController.PlayAnimtion(m_animator, (AnimController.GetAttack(m_attackType, m_attackStance, m_manoeuvreIndex)));

        return true;
    }

    /// <summary>
    /// Update the manoeuvre
    /// </summary>
    /// <returns>True when completed</returns>
    protected bool UpdateAttackManoeuvre()
    {
        float animationPercent = AnimController.GetAnimationPercent(m_animator);

        float modelToSplineForwardDot = Vector3.Dot(m_character.m_characterModel.transform.forward, m_character.m_splinePhysics.m_currentSpline.GetForwardDir(m_character.m_splinePhysics.m_currentSplinePercent));

        //Update Translation
        float expectedTranslation = m_translationCurve.Evaluate(animationPercent);
        if(modelToSplineForwardDot >= 0.0f)//Facing correct way
            m_character.Translate(expectedTranslation - m_previousTranslation);
        else
            m_character.Translate(-expectedTranslation + m_previousTranslation);

        m_previousTranslation = expectedTranslation;

        switch (m_currentState)
        {
            case ATTACK_MANOEUVRE_STATE.WIND_UP:
                if (animationPercent > m_startDamage)
                    m_currentState = ATTACK_MANOEUVRE_STATE.DAMAGE;
                break;
            case ATTACK_MANOEUVRE_STATE.DAMAGE:
                UpdateWeaponDamage();
                if (animationPercent > m_endDamage)
                    m_currentState = ATTACK_MANOEUVRE_STATE.AWAITING_COMBO;
                break;
            case ATTACK_MANOEUVRE_STATE.AWAITING_COMBO:
                if (animationPercent > m_startCombo)
                    m_currentState = ATTACK_MANOEUVRE_STATE.COMBO_CHECK;
                break;
            case ATTACK_MANOEUVRE_STATE.COMBO_CHECK:
                if (animationPercent > m_endCombo)
                    m_currentState = ATTACK_MANOEUVRE_STATE.END_ATTACK;
                if (m_character.DetermineLightInput())
                {
                    m_comboFlag = true;
                    m_attackStance = AnimController.ATTACK_STANCE.LIGHT;
                    m_currentState = ATTACK_MANOEUVRE_STATE.END_ATTACK;
                }
                if (m_character.DetermineHeavyInput())
                {
                    m_comboFlag = true;
                    m_attackStance = AnimController.ATTACK_STANCE.HEAVY;
                    m_currentState = ATTACK_MANOEUVRE_STATE.END_ATTACK;
                }
                break;
            case ATTACK_MANOEUVRE_STATE.END_ATTACK:
                return (m_comboFlag || AnimController.IsAnimationDone(m_animator));
        }

        return false;
    }

    /// <summary>
    /// End of attacking manoeuvre
    /// </summary>
    private void EndAttackManoeuvre()
    {
        m_character.m_gravity = true;

        //Toggle Colliders
        if (m_primaryWeaponScript != null)
            m_primaryWeaponScript.ToggleTrigger(false);
        if (m_secondaryWeaponScript != null)
            m_secondaryWeaponScript.ToggleTrigger(false);

        m_manoeuvreIndex++;
    }

    #endregion

    #region WEAPON DAMAGE
    //Attacking states
    public void StartWeaponDamage()
    {
        m_collidingCharacters.Clear();
    }

    public void UpdateWeaponDamage()
    {
        foreach (KeyValuePair<Character, float> colliderDetails in m_collidingCharacters)
        {

        }
    }

    /// <summary>
    /// Collider has entered, add to list of objects being damaged 
    /// </summary>
    /// <param name="p_collider">Collider</param>
    public void ColliderEntered(Collider p_collider)
    {
        if (p_collider.gameObject.layer == LayerController.m_character && p_collider.gameObject != gameObject)
        {
            Character character = p_collider.gameObject.GetComponent<Character>();

            if (character != null && !m_collidingCharacters.ContainsKey(character))
            {
                m_collidingCharacters.Add(character, AnimController.GetAnimationPercent(m_animator));
            }
        }
    }

    #endregion

    /// <summary>
    /// Determine what attack type should be performed?
    /// grounded and sprinting = sprinting
    /// just grounded = grounded
    /// in the air = in_air
    /// </summary>
    /// <returns>Correct type, defualt to grounded</returns>
    public AnimController.ATTACK_TYPE DetermineAttackType()
    {
        if (!m_character.m_splinePhysics.m_downCollision) //In the air
        {
            return AnimController.ATTACK_TYPE.IN_AIR;
        }
        if (Mathf.Abs(m_character.m_localVelocity.x) > m_character.m_groundRunVel)//Is it sprinting or just grounded
            return AnimController.ATTACK_TYPE.SPRINTING;

        return AnimController.ATTACK_TYPE.GROUND;
    }

    /// <summary>
    /// Determine attacking stance, light or heavy
    /// </summary>
    /// <returns>Based off input</returns>
    public AnimController.ATTACK_STANCE DetermineStance()
    {
        return m_character.DetermineLightInput() ? AnimController.ATTACK_STANCE.LIGHT : AnimController.ATTACK_STANCE.HEAVY;
    }
}
