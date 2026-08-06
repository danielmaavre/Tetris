using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// This script controls the board functionality. Defines the start function and
/// any other function related to drawing and erasing tiles. Also executes a coroutine
/// that clears rows whenever one or more are full.
/// </summary>
public class Board : MonoBehaviour
{
    //Array that defines all the possible tetrominoes that can be played
    public TetrominoData[] tetrominoes;

    //Reference to the piece that's currently being controlled by the player
    public Piece ActivePiece {get; private set;}

    //Flag that represents the clearing lines status. It controls the update loop, locking it
    //until the board finishes clearing all the full rows
    public bool IsClearingLines {get; private set;}

    //Reference to the board tilemap, allows to draw all tiles
    public Tilemap Tilemap {get; private set;}

    //Variable that controls the clearing lines animation
    [SerializeField] private int blinkCount = 3;

    //Variable that controls the rate at which the clearing lines animation blinks
    [SerializeField] private float blinkInterval = 0.1f;

    //Spawn position for every new piece
    public Vector3Int defaultSpawnPosition = new(-1,8,0);

    //Board dimensions
    public Vector2Int boardSize = new(10,20);

    // Dictionary that saves all the tiles added to the clearing process
    private Dictionary<Vector3Int, TileBase> cachedTiles = new();

    // This function defines the board boundaries, used to prevent pieces from
    // being drawn outside the game board
    public RectInt Bounds
    {
        get
        {
            //Uses the previously defined board size to limit the boundaries
            Vector2Int position = new Vector2Int(-boardSize.x/2,-boardSize.y/2);
            return new RectInt(position, boardSize);
        }
    }

    //Board awake function, initializes the tilemap, the active piece script and 
    // caches every defined tetromino
    private void Awake()
    {
        Tilemap = GetComponentInChildren<Tilemap>();
        ActivePiece = GetComponentInChildren<Piece>();

        for (int i = 0; i < tetrominoes.Length; i++)
        {
            tetrominoes[i].Initialize();
        }
    }

    //Sart function. First action of the game, calls the spawn next function
    private void Start()
    {
        ActivePiece.SpawnNext();
        // SpawnPiece(defaultSpawnPosition, GenRandomPiece());
    }

    //Sets any given piece in the given coordinates inside the board. 
    public void SpawnPiece(Vector3Int spawnPosition, TetrominoData data)
    {
        // Debug.Log("Initialize Piece");

        //Initializes the piece information
        ActivePiece.Initialize(this, spawnPosition, data);

        //Checks if the position of the new piece is valid (grants the new piece won't overlap with any drawn tiles)
        //If the position is valid sets the piece on the board
        if(IsValidPosition(ActivePiece, spawnPosition))
        {
            // Debug.Log("Valid Position");            
            Set(ActivePiece);
        }
        // Otherwise triggers the game over function to end the game
        else
        {
            // Debug.Log($"Invalid spawn at {spawnPosition}, blocking tiles at: " +
            //     string.Join(", ", Array.ConvertAll(ActivePiece.cells, c => c + spawnPosition)));            
            GameOver();
            Debug.Log("Game Over");
        }
    }

    //This function returns a random tetromino from the list defined inside the awake function
    public TetrominoData GenRandomPiece()
    {
        TetrominoData piece = tetrominoes[UnityEngine.Random.Range(0,tetrominoes.Length)];
        return piece;
    }

    //The game over function clears the whole game board and resets the score and level counters
    private void GameOver()
    {
        Tilemap.ClearAllTiles();
        ScoreManager.scoreManager.ClearScore();
        LevelManager.levelManager.LevelReset();
    }

    //This function draws the piece into the board
    public void Set(Piece piece)
    {
        //It loops each individual cell of the tetromino 
        for (int i = 0; i < piece.cells.Length; i++)
        {
            //Offset the piece position by the actual position of each piece
            Vector3Int tilePosition = piece.cells[i] + piece.position;
            //Sets each cell in a tile
            Tilemap.SetTile(tilePosition, piece.data.tile);
        }
    }

    //This function erases the piece from the board
    public void Clear(Piece piece)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            //Offset a null on the previous position of a piece
            Vector3Int tilePosition = piece.cells[i] + piece.position;
            Tilemap.SetTile(tilePosition, null);
        }
    }    

    //This function returns a flag that indicates if the position of a given piece is allowed or not
    public bool IsValidPosition(Piece piece, Vector3Int position)
    {
        //Loops over every cell of the piece
        for (int i = 0; i < piece.cells.Length; i++)
        {
            //Gets the board boundaries
            RectInt bounds = Bounds;

            //Gets the position of the current looped cell
            Vector3Int tilePosition = piece.cells[i] + position;

            //Checks if the cell's position is inside the boards bounds
            if (!bounds.Contains((Vector2Int)tilePosition))
            {
                //If not, it's not a valid position
                return false;
            }
            
            //Checks if there's a cell already drawn into the tile
            if (Tilemap.HasTile(tilePosition))
            {
                //If there is a cell, it's not a valid position
                return false;
            }
        }

        //If all the piece's cells pass the checks, it is a valid position
        return true;
    } 

    //Function in charge of checking if there are full rows that must be cleared.
    //It receives an input action as a callback parameter that will be executed if there aren't rows to clear
    public void ClearLines(Action onComplete)
    {
        //Gets the board boundaries
        RectInt boardBounds = Bounds;

        //Defines a list that will contain every full row
        List<int> fullRows = new();
        
        //Iterates over every row of the board (y axis) limited by its boundaries checking if the row is full
        for (int row = boardBounds.yMin; row < boardBounds.yMax; row++)
        {
            if (IsLineFull(row))

                //If the row is full, it's added to the list
                fullRows.Add(row);            
        }

        //Updates the player score. The amount of points depends on the amount of rows cleared
        ScoreManager.scoreManager.AddScore(LevelManager.levelManager.currentLevel, fullRows.Count);

        //Checks if the current score is enough to level up
        LevelManager.levelManager.LevelUp(fullRows.Count);        

        //Checks if any full row was found
        if (fullRows.Count > 0)
        {
            //Starts coroutine with the clear lines function to erase all full rows and execute the clearing animation
            //Passes down the onComplete callback to be executed at the end of the clearing animation
            StartCoroutine(ClearLinesRoutine(fullRows, boardBounds, onComplete));

        } else
        {
            //If there are no rows to clear, executes the input action. 
            // '?' is used to check if the onComplete action is not null before its execution
            onComplete?.Invoke();
        }
    }  

    //Function defined to execute a coroutine for the clear lines animation, recieves the list of full rows, 
    // the board boundaries and the action to execute at the end of the coroutine
    private IEnumerator ClearLinesRoutine(List<int> fullRows, RectInt boardBounds, Action onComplete)
    {
        //Sets the clearing lines status to avoid the update function from spawning new pieces and prevent 
        // the player from performing actions
        IsClearingLines = true;

        //Saves in memory the position of the rows to be cleared
        CacheRowTiles(fullRows,boardBounds);

        //Executes the blinking animation the amount of times defined by blinkCount
        for (int i = 0; i < blinkCount; i++)
        {
            //Sets rows invisible and waits for the interval
            SetRowsVisible(fullRows, false,boardBounds);
            yield return new WaitForSeconds(blinkInterval);

            //Sets rows visible and waits for the interval
            SetRowsVisible(fullRows, true,boardBounds);
            yield return new WaitForSeconds(blinkInterval);            
        }

        //Rearranges the list so the rows are removed descending
        fullRows.Sort();
        fullRows.Reverse();

        //Clears every full row in the board
        foreach (int row in fullRows)
        {
            LineClear(row);
        }

        //Disables the clearing lines status so the game can continue
        IsClearingLines = false;

        //Executes the callback action
        onComplete?.Invoke();
    }

    //Function defined to store in a list the information of every full row that will be cleared
    private void CacheRowTiles(List<int> rows, RectInt boardBounds)
    {
        //Clears any previously saved list of rows to clear
        cachedTiles.Clear();
        
        //Loops over each row to clear
        foreach (int row in rows)
        {
            //Loops over evey tile position in the x axis of the board and saves its content
            for (int col = boardBounds.xMin; col < boardBounds.xMax; col++)
            {
                Vector3Int pos = new(col, row, 0);
                cachedTiles[pos] = Tilemap.GetTile(pos);
            }
        }
    }

    //Function defined to the visibility of the tiles inside the rows to clear
    private void SetRowsVisible(List<int> rows, bool isVisible, RectInt boardBounds)
    {
        //Iterates over each full row
        foreach (int row in rows)
        {
            //Loops over every tile on the x axis of the row and toggles its visibility accordinc to isVisible
            for (int col = boardBounds.xMin; col < boardBounds.xMax; col++)
            {
                Vector3Int pos = new(col, row, 0);
                Tilemap.SetTile(pos, isVisible ? cachedTiles[pos] : null);
            }
        }
    }

    //Function in charge of checkig if the row is full
    private bool IsLineFull(int row)
    {
        //Gets the board boundaries
        RectInt bounds = Bounds;

        //Loops over every tiles of the row and checks if it has content
        for (int col = bounds.xMin; col < bounds.xMax; col++)
        {
            Vector3Int position = new Vector3Int(col, row, 0);

            if (!Tilemap.HasTile(position))
            {
                //If any of the row's tiles is empty, declares the row as not full
                return false;
            }
        }

        //If it gets to this point, the row is full
        return true;
    }

    //Function in charge of deleting a whole row from the board
    private void LineClear(int row)
    {
        //Defines the board boundaries
        RectInt bounds = Bounds;

        //Loops over each tile of the input row
        for (int col = bounds.xMin; col < bounds.xMax; col++)
        {
            //Sets empty the tile position
            Vector3Int position = new Vector3Int(col, row, 0);
            Tilemap.SetTile(position, null);
        }

        //Loops over every other row
        while(row < bounds.yMax)
        {
            //Shifts down all the rows tiles
            for (int col = bounds.xMin; col < bounds.xMax; col++)
            {
                Vector3Int position = new Vector3Int(col, row + 1, 0);
                TileBase above = Tilemap.GetTile(position);

                position = new Vector3Int(col, row, 0);
                Tilemap.SetTile(position,above);
            }

            row++;
        }
    }
}
