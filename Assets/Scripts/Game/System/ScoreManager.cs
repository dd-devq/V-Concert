using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class ScoreManager : SingletonMono<ScoreManager>
{
    [SerializeField] private TextMeshProUGUI scoreText = null;
    [SerializeField] private TextMeshProUGUI comboCountText = null;

    private float comboScore = 0;
    private float totalScore = 0;
    private int comboCount = 0;

    void Start()
    {
        comboScore = 0;
    }

    public void OnResponseNoteHit(Component sender, object data)
    {
        if (data is HitType hitData)
        {
            if (hitData == HitType.Perfect)
            {
                comboCount += 1;
                int multiplier = GetMultiplier();
                float score = Define.PerfectScore * multiplier;
                totalScore += score;
            }
            else if (hitData == HitType.Miss)
            {
                comboCount = 0;
                float score = Define.BaseScore;
                totalScore += score;
                Debug.LogError("comboCount: " + comboCount);
            }
            scoreText.text = totalScore.ToString();
            comboCountText.text = comboCount.ToString() + " HIT";
        }
    }

    private int GetMultiplier()
    {
        if (comboCount >= 0 && comboCount <= 4)
        {
            Debug.LogError(string.Format("{0} => {1}", comboCount, 1));
            return 1;
        }
        else if (comboCount >= 5 && comboCount <= 9)
        {
            Debug.LogError(string.Format("{0} => {1}", comboCount, 2));
            return 2;
        }
        else if (comboCount >= 10 && comboCount <= 19)
        {
            Debug.LogError(string.Format("{0} => {1}", comboCount, 3));
            return 3;
        }
        else
        {
            Debug.LogError(string.Format("{0} => {1}", comboCount, 5));
            return 5;
        }
    }
}