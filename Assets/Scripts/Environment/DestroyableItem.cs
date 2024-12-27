using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyableItem : MonoBehaviour
{
    [SerializeField] private int startingHealthAmount = 1;
    [SerializeField] private SoundEffectSO destroySoundEffect;
    private Animator animator;
    private BoxCollider2D boxCollider2D;
    private HealthEvent healthEvent;
    private Health health;
    private ReceiveContactDamage receiveContactDamage;
    
    private void Awake() 
    {
        animator = GetComponent<Animator>();
        boxCollider2D = GetComponent<BoxCollider2D>();
        healthEvent = GetComponent<HealthEvent>();
        health = GetComponent<Health>();
        receiveContactDamage = GetComponent<ReceiveContactDamage>();
    }

    private void OnEnable() 
    {
        healthEvent.OnHealthChanged += HealthEvent_OnHealthChanged;
    }

    private void OnDisable() 
    {
        healthEvent.OnHealthChanged -= HealthEvent_OnHealthChanged;
    }

    private void HealthEvent_OnHealthChanged(HealthEvent healthEvent, HealthEventArgs healthEventArgs)
    {
        if (healthEventArgs.healthAmount <= 0f)
        {
            StartCoroutine(PlayAnimation());
        }
    }

    private IEnumerator PlayAnimation()
    {
        Destroy(boxCollider2D);
        if(destroySoundEffect != null)
        {
            SoundEffectManager.Instance.PlaySoundEffect(destroySoundEffect);
        }
        animator.SetBool(Settings.destroy, true);

        while (!animator.GetCurrentAnimatorStateInfo(0).IsName(Settings.stateDestroyed))
        {   // animasyonun tamamlanmasini beklerken dongunun her frame'de sadece 1 kez calismasini saglar.
            yield return null;
        }

        Destroy(animator);
        Destroy(receiveContactDamage);
        Destroy(health);
        Destroy(healthEvent);
        Destroy(this);
    }
}


