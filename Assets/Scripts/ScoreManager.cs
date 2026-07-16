using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class ScoreManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreDisplay;
    [SerializeField] private int scoreSingle = 40;
    [SerializeField] private int scoreDouble = 100;
    [SerializeField] private int scoreTriple = 300;
    [SerializeField] private int scoreTetris = 1200;
    public static ScoreManager scoreManager;
    private int score = 0;

    private void Awake()
    {
        if(scoreManager == null){
            scoreManager = this;
        }
        else{
            Destroy(gameObject);
        }
    }    

    private void Start()
    {
        UpdateScoreUI();
    }

    public void AddScore(int level, int rowsCleared = 0)
    {
        int pointsEarned;
        
        if (rowsCleared > 0)
        {
            pointsEarned = GetPoints(rowsCleared);
        }
        else
        {
            pointsEarned = 1;
        }
                
        score += pointsEarned * (level + 1);
        // Debug.Log($"Level {level}, Rows cleared {rowsCleared}, Points earned {pointsEarned}, Score {score}");

        UpdateScoreUI();
        LevelManager.levelManager.LevelUp(score);
    }

    public void ClearScore()
    {
        score = 0;
        UpdateScoreUI();
    }    

    private void UpdateScoreUI()
    {
        scoreDisplay.text = score.ToString();
    }

    private int GetPoints(int rowsCleared)
    {
        int points = 0;
        points = rowsCleared switch
        {
            1 => scoreSingle,
            2 => scoreDouble,
            3 => scoreTriple,
            4 => scoreTetris,
            _ => rowsCleared * scoreTetris,
        };
        return points;        
    }
}
