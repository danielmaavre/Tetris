using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Enumerator that lists all the possible tetrominoes
/// </summary>
public enum Tetromino
{
    I,
    O,
    T,
    J,
    L,
    S,
    Z,
}

//Structure defined to control the information contained by a tetromino
[System.Serializable]
public struct TetrominoData
{
    public Tetromino tetromino;
    public Tile tile;

    //In order not to show up on the editor, we can assign get and set to transform cells from
    //a field into a property public ***Vector2Int[] cells{get; private set;}***
    public Vector2Int[] cells;

    //Adds the wallkicks for each piece
    public Vector2Int[,] wallKicks {get; private set;}

    //Assigns the static data to our cells
    public void Initialize()
    {
        cells = Data.Cells[tetromino];
        wallKicks = Data.WallKicks[tetromino];
    }
}