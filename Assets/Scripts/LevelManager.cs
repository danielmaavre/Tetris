using TMPro;
using UnityEngine;

/// <summary>
/// This class controls the game leveling system
/// </summary>
public class LevelManager : MonoBehaviour
{
    //Reference to the current level counter
    [SerializeField] private TextMeshProUGUI currentLevelTMP;

    //Variable that defines the base amount of points required to level up
    [SerializeField] private int baseRowsToLevelUp = 10;

    //Parameter that defines the step delay, it will decrease as the player levels up
    public float StepDelay{get; private set;}

    //Parameter that holds the current level
    public int currentLevel{get; private set;}

    //Inscance of the level manager
    public static LevelManager levelManager;

    //Cleared rows counter
    private int totalRowsCleared = 0;

    //Rows required to level up
    private int requiredRows;
    
    //Awake function, starts the level information
    private void Awake()
    {
        currentLevel = 1;
        StepDelay = 1;

        SetCurrentLevelGUI(currentLevel);
        CalcRequiredRows();

        if(levelManager == null){
            levelManager = this;
        }
        else{
            Destroy(gameObject);
        }        
    }

    //Checks if the current score is enough to level up
    public void LevelUp(int rowsCleared)
    {
        //Adds the amount of rows cleared to the total
        UpdateRowsCleared(rowsCleared);

        //Validates if the level up condition was met
        if (totalRowsCleared >= requiredRows)
        {
            //Adds 1 to the current level
            currentLevel++; 

            //In case of leveling up, resets the amount of rows cleared
            UpdateRowsCleared(rowsCleared, true);

            //Updates the level up requirement
            CalcRequiredRows(); 

            //Updates the level on screen
            SetCurrentLevelGUI(currentLevel);

            //Updates the piece falling speed
            UpdateStepDelay();                          
        }              
    }

    //Rests the current level every time the game ends
    public void LevelReset()
    {
        currentLevel = 1;
        SetCurrentLevelGUI(currentLevel);
        UpdateStepDelay();
    }

    //Updates the step delay every time the level changes
    private void UpdateStepDelay()
    {
        StepDelay = Mathf.Pow((float)(1 - ((currentLevel - 1) * 0.007)),currentLevel - 1);
        Debug.Log($"Step delay {StepDelay}");
    }

    //Updates the current level every time the player levels up
    private void SetCurrentLevelGUI(int level)
    {
        currentLevelTMP.text = level.ToString();
    }

    private void CalcRequiredRows()
    {
        requiredRows = baseRowsToLevelUp * currentLevel;
    }

    private void UpdateRowsCleared(int rows, bool leveledUp = false)
    {
        if (leveledUp)
        {
            totalRowsCleared -= requiredRows;
        }
        else
        {
            totalRowsCleared += rows;
        }
    }
}
