using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Tilemaps;


public class Board : MonoBehaviour
{
    public TetrominoData[] tetrominoes;
    public Piece ActivePiece {get; private set;}
    public bool IsClearingLines {get; private set;}
    public Tilemap Tilemap {get; private set;}

    [SerializeField] private int blinkCount = 3;
    [SerializeField] private float blinkInterval = 0.1f;

    public Vector3Int defaultSpawnPosition = new(-1,8,0);
    public Vector2Int boardSize = new(10,20);
    private Dictionary<Vector3Int, TileBase> cachedTiles = new();

    public RectInt Bounds
    {
        get
        {
            Vector2Int position = new Vector2Int(-boardSize.x/2,-boardSize.y/2);
            return new RectInt(position, boardSize);
        }
    }

    private void Awake()
    {
        Tilemap = GetComponentInChildren<Tilemap>();
        ActivePiece = GetComponentInChildren<Piece>();

        for (int i = 0; i < tetrominoes.Length; i++)
        {
            tetrominoes[i].Initialize();
        }
    }

    private void Start()
    {
        SpawnPiece(defaultSpawnPosition);
    }

    public void SpawnPiece(Vector3Int spawnPosition, TetrominoData? heldPiece = null)
    {
        TetrominoData data = heldPiece ?? tetrominoes[UnityEngine.Random.Range(0,tetrominoes.Length)];

        ActivePiece.Initialize(this, spawnPosition, data);

        if(IsValidPosition(ActivePiece, spawnPosition))
        {
            Set(ActivePiece);
        }
        else
        {
            GameOver();
            Debug.Log("Game Over");
        }
    }

    private void GameOver()
    {
        Tilemap.ClearAllTiles();
        ScoreManager.scoreManager.ClearScore();
        LevelManager.levelManager.LevelReset();
    }

    public void Set(Piece piece)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            //Offset the piece position by the actual position of each piece
            Vector3Int tilePosition = piece.cells[i] + piece.position;
            Tilemap.SetTile(tilePosition, piece.data.tile);
        }
    }

    public void Clear(Piece piece)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            //Offset a null on the previous position of a piece
            Vector3Int tilePosition = piece.cells[i] + piece.position;
            Tilemap.SetTile(tilePosition, null);
        }
    }    

    public bool IsValidPosition(Piece piece, Vector3Int position)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            RectInt bounds = Bounds;

            Vector3Int tilePosition = piece.cells[i] + position;

            if (!bounds.Contains((Vector2Int)tilePosition))
            {
                return false;
            }
            
            if (Tilemap.HasTile(tilePosition))
            {
                return false;
            }
        }

        return true;
    } 

    public void ClearLines(Action oncomplete)
    {
        RectInt boardBounds = Bounds;
        List<int> fullRows = new();
        
        for (int row = boardBounds.yMin; row < boardBounds.yMax; row++)
        {
            if (IsLineFull(row))
                fullRows.Add(row);            
        }

        if (fullRows.Count > 0)
        {
            //Clears the lines with blink animation
            StartCoroutine(ClearLinesRoutine(fullRows, boardBounds, oncomplete));

            //Updates the player score
            ScoreManager.scoreManager.AddScore(LevelManager.levelManager.currentLevel, fullRows.Count);
        } else
        {
            oncomplete?.Invoke();
        }
    }  

    private IEnumerator ClearLinesRoutine(List<int> fullRows, RectInt boardBounds, Action onComplete)
    {
        IsClearingLines = true;

        //Saves in memory the position of the rows
        CacheRowTiles(fullRows,boardBounds);

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
        foreach (int row in fullRows)
        {
            LineClear(row);
        }

        IsClearingLines = false;

        onComplete?.Invoke();
    }

    private void CacheRowTiles(List<int> rows, RectInt boardBounds)
    {
        cachedTiles.Clear();
        
        foreach (int row in rows)
        {
            for (int col = boardBounds.xMin; col < boardBounds.xMax; col++)
            {
                Vector3Int pos = new(col, row, 0);
                cachedTiles[pos] = Tilemap.GetTile(pos);
            }
        }
    }

    private void SetRowsVisible(List<int> rows, bool isVisible, RectInt boardBounds)
    {
        foreach (int row in rows)
        {
            for (int col = boardBounds.xMin; col < boardBounds.xMax; col++)
            {
                Vector3Int pos = new(col, row, 0);
                Tilemap.SetTile(pos, isVisible ? cachedTiles[pos] : null);
            }
        }
    }

    private bool IsLineFull(int row)
    {
        RectInt bounds = Bounds;

        for (int col = bounds.xMin; col < bounds.xMax; col++)
        {
            Vector3Int position = new Vector3Int(col, row, 0);

            if (!Tilemap.HasTile(position))
            {
                return false;
            }
        }

        return true;
    }

    private void LineClear(int row)
    {
        RectInt bounds = Bounds;

        for (int col = bounds.xMin; col < bounds.xMax; col++)
        {
            Vector3Int position = new Vector3Int(col, row, 0);
            Tilemap.SetTile(position, null);
        }

        while(row < bounds.yMax)
        {
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
