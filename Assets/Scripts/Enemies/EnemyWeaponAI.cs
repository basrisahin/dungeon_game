using System.Net;
using Cinemachine;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;

[RequireComponent(typeof(Enemy))]
[DisallowMultipleComponent]
public class EnemyWeaponAI : MonoBehaviour
{
    // select the layers that enemy bullet can hit.
    [SerializeField] private LayerMask layerMask;
    // populate it from weaponShootPosition child gameobject.
    [SerializeField] private Transform weaponShootPosition;
    private Enemy enemy;
    private EnemyDetailsSO enemyDetails;
    private float firingIntervalTimer;
    private float firingDurationTimer;


    private void Awake() 
    {
        enemy = GetComponent<Enemy>();   
    }

    private void Start() 
    {
        enemyDetails = enemy.enemyDetails;
        firingIntervalTimer = WeaponShootInterval();
        firingDurationTimer = WeaponShootDuration();
    }

    private float WeaponShootInterval()
    {
        return Random.Range(enemyDetails.firingIntervalMin, enemyDetails.firingIntervalMax);
    }

    private float WeaponShootDuration()
    {
        return Random.Range(enemyDetails.firingDurationMin, enemyDetails.firingDurationMax);
    }

    private void Update() 
    {
        // update interval timer
        firingIntervalTimer -= Time.deltaTime;

        if (firingIntervalTimer < 0f)
        {
            if (firingDurationTimer > 0f)
            {
                firingDurationTimer -= Time.deltaTime;
                FireWeapon();
            }
            else
            {
                firingIntervalTimer = WeaponShootInterval();
                firingDurationTimer = WeaponShootDuration();
            }
        }        
    }

    private void FireWeapon()
    {
        // player'la enemy arasindaki uzakligi hesaplayalim
        Vector3 playerDirectionVector = GameManager.Instance.GetPlayer().GetPlayerPosition() - transform.position;
        // enemy'nin silahiyla player arasindaki uzakligi hesaplayalim
        Vector3 weaponDirection = GameManager.Instance.GetPlayer().GetPlayerPosition() - weaponShootPosition.position;
        // Get weapon to player angle
        float weaponAngleDegree = HelperUtilities.GetAngleFromVector(weaponDirection);
        // Get enemy to player angle
        float enemyAngleDegree = HelperUtilities.GetAngleFromVector(playerDirectionVector);
        // weapon'un acisina gore enemy'nin poziyonunu ayarlayalim.
        AimDirection enemyAimDirection = HelperUtilities.GetAimDirection(enemyAngleDegree);
        // trigger weapon aim event
        enemy.aimWeaponEvent.CallAimWeaponEvent(enemyAimDirection, enemyAngleDegree, weaponAngleDegree, weaponDirection);
        // eger bu enemy tipinin silahi varsa
        if (enemyDetails.enemyWeapon != null)
        {
            float enemyAmmoRange = enemyDetails.enemyWeapon.weaponCurrentAmmo.ammoRange;
            // is player in range
            if (playerDirectionVector.magnitude <= enemyAmmoRange)
            {
                // eger onunun acik olmasi gerekliyse ve onu kapaliysa no fire
                //eger onunun acik olmasi gerekliyse ve onu aciksa fire!!
                // eger onunun acik olmasi gerekli degilse ates!!
                if (enemyDetails.firingLineOfSightRequired && !IsPlayerInLineOfSight(weaponDirection, enemyAmmoRange)) return;
                // trigger fire weapon event
                enemy.fireWeaponEvent.CallFireWeaponEvent(true, true, enemyAimDirection, enemyAngleDegree, weaponAngleDegree, weaponDirection);
            }   
        }
    }

    private bool IsPlayerInLineOfSight(Vector3 weaponDirection, float enemyAmmoRange)
    {
        RaycastHit2D raycastHit2D = Physics2D.Raycast(weaponShootPosition.position, (Vector2)weaponDirection, enemyAmmoRange, layerMask);
        // if hit and hit to player.
        if (raycastHit2D && raycastHit2D.transform.CompareTag(Settings.playerTag))
        {
            return true;
        }

        return false;
    }

    #region Validation
#if UNITY_EDITOR
    private void OnValidate() 
    {
        HelperUtilities.ValidateCheckNullValue(this, nameof(weaponShootPosition), weaponShootPosition);
    }
#endif
    #endregion
    


}
