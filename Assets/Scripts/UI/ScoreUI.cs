using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class ScoreUI : MonoBehaviour
{
    private TextMeshProUGUI scoreTextTMP;
    private TextMeshProUGUI multiplierTextTMP;

    private void Awake() 
    {
        scoreTextTMP = transform.Find("ScoreText").GetComponent<TextMeshProUGUI>();
        multiplierTextTMP = transform.Find("MultiplierText").GetComponent<TextMeshProUGUI>();
    }

    private void OnEnable() 
    {
        StaticEventHandler.OnScoreChanged += StaticEventHandler_OnScoreChanged;        
    }

    private void OnDisable()
    {
        StaticEventHandler.OnScoreChanged -= StaticEventHandler_OnScoreChanged;
    }

    private void StaticEventHandler_OnScoreChanged(ScoreChangedArgs scoreChangedArgs)
    {
        UpdateScoreAndMultiplier(scoreChangedArgs.score, scoreChangedArgs.multiplier);
    }

    private void UpdateScoreAndMultiplier(long score, int multiplier)
    {
        scoreTextTMP.text = "SCORE: " + score.ToString("###,###0");
        multiplierTextTMP.text = "MULTIPLIER: x:" + multiplier;
    }
}
