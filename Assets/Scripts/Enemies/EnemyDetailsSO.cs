using UnityEngine;

[CreateAssetMenu(fileName = "EnemyDetails_", menuName = "Scriptable Objects/Enemy/EnemyDetails")]
public class EnemyDetailsSO : ScriptableObject
{
    #region Header BASE ENEMY DETAILS
    [Space(10)]
    [Header("BASE ENEMY DETAILS")]
    #endregion

    #region Tooltip
    [Tooltip("The name of the enemy")]
    #endregion
    public string enemyName;

    #region Tooltip
    [Tooltip("The prefab for the enemy")]
    #endregion
    public GameObject enemyPrefab;

    #region Tooltip
    [Tooltip("Distance to the player before enemy starts chasing")]
    #endregion
    public float chaseDistance = 50f;

    #region Header ENEMY MATERIAL
    [Space(10)]
    [Header("ENEMY MATERIAL")]
    #endregion
    #region Tooltip
    [Tooltip("This is the standard lit shader material for the enemy (used after the enemy materializes")]
    #endregion
    public Material enemyStandardMaterial;

    #region Header ENEMY MATERIALIZE SETTINGS
    [Space(10)]
    [Header("ENEMY MATERIALIZE SETTINGS")]
    #endregion
    #region Tooltip
    [Tooltip("The time in seconds that it takes the enemy to materialize")]
    #endregion
    public float enemyMaterializeTime;
    #region Tooltip
    [Tooltip("The shader to be used when the enemy materializes")]
    #endregion
    public Shader enemyMaterializeShader;
    [ColorUsage(true, true)]
    #region Tooltip
    [Tooltip("The colour to use when the enemy materializes.  This is an HDR color so intensity can be set to cause glowing / bloom")]
    #endregion
    public Color enemyMaterializeColor;

    // Weapon Details
    public WeaponDetailsSO enemyWeapon;
    
    // Enemy'nin iki attagi arasindaki fark ne kadardir olmali? Random btw 0.1 ve 1 gibi.
    public float firingIntervalMin = 0.1f;
    public float firingIntervalMax = 1f;
    // Bir attack ne kadar surecek. Random btw min and max.
    public float firingDurationMin = 1f;
    public float firingDurationMax = 2f;

    // Enemy'in sikmasi icin onunde engel olmamali mi?
    // Eger bu deger 1'se enemy gorus alaninda engel oldugu icin sikmaz.
    // Bordo bereli enemy. 
    public bool firingLineOfSightRequired;    

    // Enemy Health Details
    public EnemyHeathDetails[] enemyHeathDetailsArray;
    // Only for player
    public bool isImmuneAfterHit = false;
    public float hitImmuneTime;
    public bool isHealthBarDisplayed = false;

    #region Validation
#if UNITY_EDITOR
    // Validate the scriptable object details entered
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckEmptyString(this, nameof(enemyName), enemyName);
        HelperUtilities.ValidateCheckNullValue(this, nameof(enemyPrefab), enemyPrefab);
        HelperUtilities.ValidateCheckPositiveValue(this, nameof(chaseDistance), chaseDistance, false);
        HelperUtilities.ValidateCheckNullValue(this, nameof(enemyStandardMaterial), enemyStandardMaterial);
        HelperUtilities.ValidateCheckPositiveValue(this, nameof(enemyMaterializeTime), enemyMaterializeTime, true);
        HelperUtilities.ValidateCheckNullValue(this, nameof(enemyMaterializeShader), enemyMaterializeShader);
        HelperUtilities.ValidateCheckPositiveRange(this,nameof(firingIntervalMin), firingIntervalMin, nameof(firingIntervalMax), firingIntervalMax ,false);
        HelperUtilities.ValidateCheckPositiveRange(this,nameof(firingDurationMin), firingDurationMin, nameof(firingDurationMax), firingDurationMax ,false);
        if(isImmuneAfterHit)
        {
            HelperUtilities.ValidateCheckPositiveValue(this, nameof(hitImmuneTime), hitImmuneTime, false);
        }
    }

#endif
    #endregion

}