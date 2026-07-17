using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI currentLevelTMP;
    [SerializeField] private int baseLevelupPoints = 800;

    public float StepDelay{get; private set;}
    public int currentLevel{get; private set;}

    public static LevelManager levelManager;
    

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

    public void LevelUp(int currentScore)
    {
        if (currentScore >= (baseLevelupPoints*currentLevel))
        {
            currentLevel++;
            UpdateCurrentLevel(currentLevel);
            Debug.Log($"Current level {currentLevel}");
            UpdateStepDelay();            
        }                 
    }

    public void LevelReset()
    {
        currentLevel = 1;
        UpdateCurrentLevel(currentLevel);
        UpdateStepDelay();
    }

    private void UpdateStepDelay()
    {
        StepDelay = Mathf.Pow((float)(1 - ((currentLevel - 1) * 0.007)),currentLevel - 1);
        Debug.Log($"Step delay {StepDelay}");
    }

    private void UpdateCurrentLevel(int level)
    {
        currentLevelTMP.text = level.ToString();
    }
}
