using System.Collections;
using TMPro;
using Unity.VisualScripting;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(HealthEvent))]
[DisallowMultipleComponent]
public class Health : MonoBehaviour
{
    [SerializeField] private HealthBar healthBar;
    private int startingHealth;
    private int currentHealth;
    private HealthEvent healthEvent;
    [HideInInspector] public bool isDamagable = true;
    
    private Coroutine immunityCoroutine;
    private bool isImmuneAfterHit;
    private float immunityTime;
    private SpriteRenderer spriteRenderer = null;
    private const float spriteFlashInterval = 0.2f;
    private WaitForSeconds waitForSecondsSpriteFlashInterval = new WaitForSeconds(spriteFlashInterval);


    private Player player;
    [HideInInspector] public Enemy enemy;

    private void Awake() 
    {
        // cache it
        healthEvent = GetComponent<HealthEvent>();
    }
    
    private void Start() 
    {   
        // trigger a health event for UI update.
        CallHealthEvent(0);
        player = GetComponent<Player>();
        enemy = GetComponent<Enemy>();

        if(player!=null)
        {
            if(player.playerDetails.isImmuneAfterHit)
            {
                isImmuneAfterHit = player.playerDetails.isImmuneAfterHit;
                immunityTime = player.playerDetails.hitImmunityTime;
                spriteRenderer = player.spriteRenderer;
            }
        }

        if(enemy != null && enemy.enemyDetails.isHealthBarDisplayed == true && healthBar != null)
        {
            healthBar.EnableHealthBar();
        }
        else
        {
            healthBar.DisableHealthBar();
        }


    }

    private void CallHealthEvent(int damageAmount)
    {
        healthEvent.CallHealthChangedEvent((float)currentHealth / (float)startingHealth, currentHealth, damageAmount);
    }

    public void TakeDamage(int damageAmount)
    {
        bool isRolling = false;
        
        if (player != null)
        {
            isRolling = player.playerControl.isPlayerRolling;
        }

        if (isDamagable && !isRolling)
        {
            currentHealth -= damageAmount;
            CallHealthEvent(damageAmount);
            PostHitImmunity();
        }

        if (healthBar != null)
        {
            healthBar.SetHealthBarValue((float)currentHealth / (float)startingHealth);
        }

    }

    private void PostHitImmunity()
    {
        if(gameObject.activeSelf == false)
            return;

        if(isImmuneAfterHit)
        {
            if(immunityCoroutine != null)
            {
                StopCoroutine(immunityCoroutine);
            }
            immunityCoroutine = StartCoroutine(PostImmunityRoutine(immunityTime, spriteRenderer));
        }
    }

    private IEnumerator PostImmunityRoutine(float immunityTime, SpriteRenderer spriteRenderer)
    {
        int iterations = Mathf.RoundToInt(immunityTime / spriteFlashInterval / 2f);

        isDamagable = false;

        while (iterations > 0)
        {
            spriteRenderer.color = Color.red;
            yield return waitForSecondsSpriteFlashInterval;
            spriteRenderer.color = Color.white;
            yield return waitForSecondsSpriteFlashInterval;
            iterations--;

            yield return null;
        }

        isDamagable = true;

    }



    /// <summary>
    /// Set starting health 
    /// </summary>
    public void SetStartingHealth(int startingHealth)
    {
        this.startingHealth = startingHealth;
        currentHealth = startingHealth;
    }

    /// <summary>
    /// Get the starting health
    /// </summary>
    public int GetStartingHealth()
    {
        return startingHealth;
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }

}
