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
    [SerializeField] private int baseLevelupPoints = 800;

    //Parameter that defines the step delay, it will decrease as the player levels up
    public float StepDelay{get; private set;}

    //Parameter that holds the current level
    public int currentLevel{get; private set;}

    //Inscance of the level manager
    public static LevelManager levelManager;
    
    //Awake function, starts the level information
    private void Awake()
    {
        currentLevel = 1;
        StepDelay = 1;

        UpdateCurrentLevel(currentLevel);

        if(levelManager == null){
            levelManager = this;
        }
        else{
            Destroy(gameObject);
        }        
    }

    //Checks if the current score is enough to level up
    public void LevelUp(int currentScore)
    {
        //The current condition to level up is for the player to 
        //have a total score equal to the current level times the base points
        if (currentScore >= (baseLevelupPoints*currentLevel))
        {
            currentLevel++;
            UpdateCurrentLevel(currentLevel);
            Debug.Log($"Current level {currentLevel}");
            UpdateStepDelay();            
        }                 
    }

    //Rests the current level every time the game ends
    public void LevelReset()
    {
        currentLevel = 1;
        UpdateCurrentLevel(currentLevel);
        UpdateStepDelay();
    }

    //Updates the step delay every time the level changes
    private void UpdateStepDelay()
    {
        StepDelay = Mathf.Pow((float)(1 - ((currentLevel - 1) * 0.007)),currentLevel - 1);
        Debug.Log($"Step delay {StepDelay}");
    }

    //Updates the current level every time the player levels up
    private void UpdateCurrentLevel(int level)
    {
        currentLevelTMP.text = level.ToString();
    }
}
