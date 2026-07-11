using UnityEngine;
using UnityEngine.Tilemaps;

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

[System.Serializable]
public struct TetrominoData
{
    public Tetromino tetromino;
    public Tile tile;

    //In order not to show up on the editor, we can assign get and set to transform cells from
    //a field into a property public ***Vector2Int[] cells{get; private set;}***
    public Vector2Int[] cells;

    //Assigns the static data to our cells
    public void Initialize()
    {
        this.cells = Data.Cells[this.tetromino];
    }
}