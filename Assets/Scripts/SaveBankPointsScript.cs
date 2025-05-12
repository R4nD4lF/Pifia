using UnityEngine;

public class SaveBankPointsScript : MonoBehaviour
{

    public ScoreScript scoreScript;

    void Start()
    {
        scoreScript = FindAnyObjectByType<ScoreScript>();
    }
    
    public void SaveScore(){
        scoreScript._totalScore += scoreScript._bankScore;
        scoreScript._totalScoreTMP.text = "Total Score" + scoreScript._totalScore.ToString();
    }
}
