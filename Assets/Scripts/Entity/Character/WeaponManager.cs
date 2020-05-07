using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    public const float COMBO_START = 0.7f;
    public const float COMBO_END = 0.95f;

    public ManoeuvreData m_weaponData = null;

    public GameObject m_primaryWeaponPrefab = null;
    public GameObject m_secondaryWeaponPrefab = null;

    private GameObject m_primaryWeaponObject = null;
    private GameObject m_secondaryWeaponObject = null;

    private Character m_character = null;
    private CustomAnimation m_customAnimation = null;

    //Attacking sequence
    private CustomAnimation.ATTACK_TYPE m_attackType = CustomAnimation.ATTACK_TYPE.GROUND;
    private CustomAnimation.ATTACK_STANCE m_attackStance = CustomAnimation.ATTACK_STANCE.LIGHT;
    private int m_manoeuvreIndex = 0; //Combo position, 0 is the first position and does not require the combo flag, goes up to 3

    //Attacking manoeurve
    private enum ATTACK_MANOEUVRE_STATE { WINDUP, COMBO_CHECK, COOLOFF }
    private ATTACK_MANOEUVRE_STATE m_currentState = ATTACK_MANOEUVRE_STATE.WINDUP;

    private bool m_comboFlag = false;
    private float m_previousXTranslation = 0.0f;
    private float m_previousYTranslation = 0.0f;

    private float m_primaryDamageStart = 0.0f;
    private float m_primaryDamageEnd = 0.0f;
    private float m_secondaryDamageStart = 0.0f;
    private float m_secondaryDamageEnd = 0.0f;

    private float m_canComboPercent = 0.0f;

    private AnimationCurve m_translationXCurve;
    private AnimationCurve m_translationYCurve;

    private WeaponTrigger m_primaryWeaponScript = null;
    private WeaponTrigger m_secondaryWeaponScript = null;

    /// <summary>
    /// Init manager
    /// </summary>
    /// <param name="p_character">Character that uses this manager</param>
    public virtual void Init(Character p_character)
    {
        m_character = p_character;
        m_customAnimation = p_character.GetComponentInChildren<CustomAnimation>();

        if (m_primaryWeaponPrefab != null)
        {
            m_primaryWeaponObject = Instantiate(m_primaryWeaponPrefab);

            m_primaryWeaponScript = m_primaryWeaponObject.GetComponent<WeaponTrigger>();
            m_primaryWeaponObject.transform.SetParent(m_character.m_rightHand.transform, false);
        }
        if (m_secondaryWeaponPrefab != null)
        {
            m_secondaryWeaponObject = Instantiate(m_secondaryWeaponPrefab);

            m_secondaryWeaponScript = m_secondaryWeaponObject.GetComponent<WeaponTrigger>();
            m_secondaryWeaponObject.transform.SetParent(m_character.m_leftHand.transform, false);
        }

        if (m_primaryWeaponScript != null)
        {
            m_primaryWeaponScript.Init(p_character);
        }
        if (m_secondaryWeaponScript != null)
        {
            m_secondaryWeaponScript.Init(p_character);
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

        m_currentState = ATTACK_MANOEUVRE_STATE.WINDUP;

        ManoeuvreData.ManoeuvreDetails attackDetails;

        m_attackType = DetermineAttackType();

        //Determine attack details
        switch (m_attackType)
        {
            case CustomAnimation_Humanoid.ATTACK_TYPE.GROUND:
                if(m_attackStance == CustomAnimation_Humanoid.ATTACK_STANCE.LIGHT)
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
            case CustomAnimation_Humanoid.ATTACK_TYPE.IN_AIR:
                if (m_attackStance == CustomAnimation_Humanoid.ATTACK_STANCE.LIGHT)
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
            case CustomAnimation_Humanoid.ATTACK_TYPE.SPRINTING:
                if (m_attackStance == CustomAnimation_Humanoid.ATTACK_STANCE.LIGHT)
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
        m_previousXTranslation = 0.0f;
        m_previousYTranslation = 0.0f;

        m_primaryDamageStart = attackDetails.m_primaryDamageStart;
        m_primaryDamageEnd = attackDetails.m_primaryDamageEnd;
        m_secondaryDamageStart = attackDetails.m_secondaryDamageStart;
        m_secondaryDamageEnd = attackDetails.m_secondaryDamageEnd;
        m_canComboPercent = attackDetails.m_canComboPercent;
        m_translationXCurve = attackDetails.m_translationXCurve;
        m_translationYCurve = attackDetails.m_translationYCurve;

        //Setup weapons
        if(m_primaryWeaponScript!=null)
            m_primaryWeaponScript.StartManoeuvre(m_primaryDamageStart, m_primaryDamageEnd, m_attackStance);
        if (m_secondaryWeaponScript != null)
            m_secondaryWeaponScript.StartManoeuvre(m_secondaryDamageStart, m_secondaryDamageEnd, m_attackStance);

        m_customAnimation.PlayAnimation(m_customAnimation.GetAttack(m_attackType, m_attackStance, m_manoeuvreIndex));

        return true;
    }

    /// <summary>
    /// Update the manoeuvre
    /// </summary>
    /// <returns>True when completed</returns>
    protected bool UpdateAttackManoeuvre()
    {
        float animationPercent = m_customAnimation.GetAnimationPercent();

        //Update Translation
        float modelToSplineForwardDot = Vector3.Dot(m_character.m_characterModel.transform.forward, m_character.m_splinePhysics.m_currentSpline.GetForwardDir(m_character.m_splinePhysics.m_currentSplinePercent));

        float expectedTranslation = m_translationXCurve.Evaluate(animationPercent);
        if(modelToSplineForwardDot >= 0.0f)//Facing correct way
            m_character.Translate(expectedTranslation - m_previousXTranslation);
        else
            m_character.Translate(-expectedTranslation + m_previousXTranslation);

        m_previousXTranslation = expectedTranslation;

        //Update manoeuvre
        switch (m_currentState)
        {
            case ATTACK_MANOEUVRE_STATE.WINDUP:
                if (animationPercent > COMBO_START)
                    m_currentState = ATTACK_MANOEUVRE_STATE.COMBO_CHECK;

                break;
            case ATTACK_MANOEUVRE_STATE.COMBO_CHECK:
                if (animationPercent > COMBO_END)
                    m_currentState = ATTACK_MANOEUVRE_STATE.COOLOFF;

                if (m_character.DetermineLightInput())
                {
                    m_comboFlag = true;
                    m_attackStance = CustomAnimation_Humanoid.ATTACK_STANCE.LIGHT;
                    m_currentState = ATTACK_MANOEUVRE_STATE.COOLOFF;
                }
                if (m_character.DetermineHeavyInput())
                {
                    m_comboFlag = true;
                    m_attackStance = CustomAnimation_Humanoid.ATTACK_STANCE.HEAVY;
                    m_currentState = ATTACK_MANOEUVRE_STATE.COOLOFF;
                }

                break;
            case ATTACK_MANOEUVRE_STATE.COOLOFF:
                return (m_comboFlag && m_customAnimation.GetAnimationPercent() > m_canComboPercent) || m_customAnimation.IsAnimationDone();
        }

        //Update weapons
        if (m_primaryWeaponScript != null)
            m_primaryWeaponScript.UpdateManoeuvre(animationPercent);
        if (m_secondaryWeaponScript != null)
            m_secondaryWeaponScript.UpdateManoeuvre(animationPercent);

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

    /// <summary>
    /// Force the end attack, e.g. chaarcter egts hit, inturrupt runs
    /// </summary>
    public void ForceEndAttack()
    {
        EndAttackManoeuvre();
    }

    #endregion

    /// <summary>
    /// Determine what attack type should be performed?
    /// grounded and sprinting = sprinting
    /// just grounded = grounded
    /// in the air = in_air
    /// </summary>
    /// <returns>Correct type, defualt to grounded</returns>
    public CustomAnimation_Humanoid.ATTACK_TYPE DetermineAttackType()
    {
        if (!m_character.m_splinePhysics.m_downCollision) //In the air
        {
            return CustomAnimation_Humanoid.ATTACK_TYPE.IN_AIR;
        }
        if (Mathf.Abs(m_character.m_localVelocity.x) > m_character.m_groundRunVel)//Is it sprinting or just grounded
            return CustomAnimation_Humanoid.ATTACK_TYPE.SPRINTING;

        return CustomAnimation_Humanoid.ATTACK_TYPE.GROUND;
    }

    /// <summary>
    /// Determine attacking stance, light or heavy
    /// </summary>
    /// <returns>Based off input</returns>
    public CustomAnimation_Humanoid.ATTACK_STANCE DetermineStance()
    {
        return m_character.DetermineLightInput() ? CustomAnimation_Humanoid.ATTACK_STANCE.LIGHT : CustomAnimation_Humanoid.ATTACK_STANCE.HEAVY;
    }
}
