using TMPro;
using UnityEngine;

/// <summary>
/// This class controls the scoring system
/// </summary>
public class ScoreManager : MonoBehaviour
{
    //Reference to the score counter
    [SerializeField] private TextMeshProUGUI scoreDisplay;

    //References to the amount of points earned according to the amount 
    // of rows cleared each time
    [SerializeField] private int scoreSingle = 40;
    [SerializeField] private int scoreDouble = 100;
    [SerializeField] private int scoreTriple = 300;
    [SerializeField] private int scoreTetris = 1200;

    //Instance of the score manager
    public static ScoreManager scoreManager;

    //Score counter
    private int score = 0;

    //Awake function, initializes the score instance
    private void Awake()
    {
        if(scoreManager == null){
            scoreManager = this;
        }
        else{
            Destroy(gameObject);
        }
    }    

    //Start function used to initialize the score
    private void Start()
    {
        UpdateScoreUI();
    }

    //Adds the amount of points earned to the score counter
    public void AddScore(int level, int rowsCleared = 0)
    {
        int pointsEarned;
        
        //If one or more rows were cleared, calculates the base amount of points earned
        if (rowsCleared > 0)
        {
            pointsEarned = GetPoints(rowsCleared);
        }
        //If no rows were cleared, gives the player 1 point
        else
        {
            pointsEarned = 1;
        }
                
        //Adds to the score the amount of points earned multiplied by the current level
        score += pointsEarned * (level + 1);

        //Updates the score inside the UI
        UpdateScoreUI();

        //Checks if the current score is enough to level up
        LevelManager.levelManager.LevelUp(score);
    }

    //Resets the score after a game over
    public void ClearScore()
    {
        score = 0;
        UpdateScoreUI();
    }    

    //Updates the score value inside the UI
    private void UpdateScoreUI()
    {
        scoreDisplay.text = score.ToString();
    }

    //Defines the points earned based on the amount of rows cleared
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
