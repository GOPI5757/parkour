using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HighScoreObj : MonoBehaviour
{
    public TMP_Text playerNameText;
    public TMP_Text highScoreText;

    public void UpdateScore(string name, int score)
    {
        playerNameText.text = name;
        highScoreText.text = score.ToString();
    }
}
