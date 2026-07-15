using System;
using NUnit.Framework.Internal;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

public class Piece : MonoBehaviour
{
    public Board board {get; private set;}
    public TetrominoData data {get; private set;}
    public Vector3Int position {get; private set;}
    public Vector3Int[] cells {get; private set;}
    public int rotationIndex {get; private set;}

    [SerializeField] private float moveDelay = 0.15f;
    private float moveTimer;

    public void Initialize(Board board, Vector3Int position, TetrominoData data)
    {
        this.board = board;
        this.position = position;
        this.data = data;
        this.rotationIndex = 0;

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
        //Piece rotation Q: Left E: Right
        if (Keyboard.current.qKey.wasPressedThisFrame)
        {
            this.board.Clear(this);
            Rotate(-1);                
        } 
        else if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            this.board.Clear(this);
            Rotate(1);
        }        

        moveTimer += Time.deltaTime;
        if (moveTimer >= moveDelay)
        {
            moveTimer = 0f;
            this.board.Clear(this);

            //Horizontal movement a: Left d: Right
            if (Keyboard.current.aKey.isPressed)
            {
                Move(Vector2Int.left);
            }
            else if (Keyboard.current.dKey.isPressed)
            {
                Move(Vector2Int.right);
            }

            //Vertical movement
            if (Keyboard.current.sKey.isPressed)
            {
                Move(Vector2Int.down);
            }   

            //Hard drop
            if (Keyboard.current.spaceKey.isPressed)
            {
                HardDrop();
            }    
            this.board.Set(this);
        }        
    }

    //Applies the rotation matrix to each piece and rotates its position by 90 degrees
    private void Rotate(int direction)
    {
        int originalRotation = this.rotationIndex;
        this.rotationIndex = Wrap(this.rotationIndex + direction, 4);
        // Debug.Log(this.rotationIndex);

        ApplyRotationMatrix(direction);

        if (!TestWallKicks(this.rotationIndex, direction))
        {
            this.rotationIndex = originalRotation;
            ApplyRotationMatrix(-direction);
        }
    }

    private void ApplyRotationMatrix(int direction)
    {
        for (int i = 0; i < this.cells.Length; i++)
        {
            Vector3 cell = this.cells[i];

            int x, y;

            switch (this.data.tetromino)
            {
                case Tetromino.I:
                case Tetromino.O:
                    cell.x -= 0.5f;
                    cell.y -= 0.5f;
                    x = Mathf.CeilToInt((cell.x * Data.RotationMatrix[0] * direction) + (cell.y * Data.RotationMatrix[1] * direction));
                    y = Mathf.CeilToInt((cell.x * Data.RotationMatrix[2] * direction) + (cell.y * Data.RotationMatrix[3] * direction)); 
                    break;
                default:

                    x = Mathf.RoundToInt((cell.x * Data.RotationMatrix[0] * direction) + (cell.y * Data.RotationMatrix[1] * direction));
                    y = Mathf.RoundToInt((cell.x * Data.RotationMatrix[2] * direction) + (cell.y * Data.RotationMatrix[3] * direction));                
                    break;
            }

            this.cells[i] = new Vector3Int(x,y,0);
        }        
    }

    private bool TestWallKicks(int rotationIndex, int rotationDirection)
    {
        int wallkickIdx = GetWallKickIdx(rotationIndex, rotationDirection);

        for (int i = 0; i < this.data.wallKicks.GetLength(1); i++)
        {
            Vector2Int translation = this.data.wallKicks[wallkickIdx,i];

            if (Move(translation))
            {
                return true;
            }
        }
        return false;
    }

    private int GetWallKickIdx(int rotationIndex, int rotationDirection)
    {
        int wallkickIdx = rotationIndex * 2;

        if (rotationDirection < 0)
        {
            wallkickIdx--;
        }
        return Wrap(wallkickIdx, this.data.wallKicks.GetLength(0));
    }

    public int Wrap(int value, int validPositions)
    {
        return ((value % validPositions) + validPositions) % validPositions;
    }

    //Hard drops a piece
    private void HardDrop()
    {
        while (Move(Vector2Int.down))
        {
            continue;
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
