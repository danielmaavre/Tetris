using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class Piece : MonoBehaviour
{
    public Board board {get; private set;}
    public TetrominoData data {get; private set;}
    public Vector3Int position {get; private set;}
    public Vector3Int[] cells {get; private set;}

    [SerializeField] private float moveDelay = 0.15f;
    private float moveTimer;

    public void Initialize(Board board, Vector3Int position, TetrominoData data)
    {
        this.board = board;
        this.position = position;
        this.data = data;

        if (this.cells == null)
        {
            this.cells = new Vector3Int[data.cells.Length];
        }

        for (int i = 0; i < data.cells.Length; i++)
        {
            this.cells[i] = (Vector3Int)data.cells[i];
        }
    }

    private void Update()
    {
        moveTimer += Time.deltaTime;

        if (moveTimer >= moveDelay)
        {
            moveTimer = 0f;
            this.board.Clear(this);

            if (Keyboard.current.aKey.isPressed)
            {
                Move(Vector2Int.left);
            }else if (Keyboard.current.dKey.isPressed)
            {
                Move(Vector2Int.right);
            }

            if (Keyboard.current.sKey.isPressed)
            {
                Move(Vector2Int.down);
            }            

            this.board.Set(this);
        }        
    }

    private bool Move(Vector2Int translation)
    {
        Vector3Int newPosition = this.position;
        newPosition.x += translation.x;
        newPosition.y += translation.y;

        bool isValid = this.board.IsValidPosition(this, newPosition);

        if (isValid)
        {
            this.position = newPosition;
        }

        return isValid;
    }


}
