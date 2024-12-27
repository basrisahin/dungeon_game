using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[DisallowMultipleComponent]
public class HealthUI : MonoBehaviour
{
    private List<GameObject> healthHeartsList = new List<GameObject>();

    private void OnEnable()
    {
        GameManager.Instance.GetPlayer().healthEvent.OnHealthChanged += HealthEvent_OnHealthChange;
    }

    private void OnDisable()
    {
        GameManager.Instance.GetPlayer().healthEvent.OnHealthChanged -= HealthEvent_OnHealthChange;
    }

    private void HealthEvent_OnHealthChange(HealthEvent healthEvent, HealthEventArgs healthEventArgs)
    {
        SetHealthBar(healthEventArgs);
    }

    private void SetHealthBar(HealthEventArgs healthEventArgs)
    {
        ClearHealthBar();
        int healthHearts = Mathf.CeilToInt(healthEventArgs.healthPercent * 5);

        for(int i = 0; i < healthHearts; i++)
        {
            GameObject heart = Instantiate(GameResources.Instance.heartPrefab, transform);
            heart.GetComponent<RectTransform>().anchoredPosition = new Vector2(Settings.uiHearthSpacing * i, 0f);
            healthHeartsList.Add(heart);
        }


    }

    private void ClearHealthBar()
    {
        foreach(GameObject heartIcon in healthHeartsList)
        {
            Destroy(heartIcon);
        }

        healthHeartsList.Clear();
    }

}
