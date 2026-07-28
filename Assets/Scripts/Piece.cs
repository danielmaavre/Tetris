using System;
using NUnit.Framework.Internal;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

public class Piece : MonoBehaviour
{  
    public Board board;
    public TetrominoData data {get; private set;}
    public Vector3Int position {get; private set;}
    public Vector3Int[] cells {get; private set;}
    public int rotationIndex {get; private set;}

    [SerializeField] private float moveDelay = 0.15f;
    [SerializeField] private float lockDelay = 0.5f;
    [SerializeField] private HoldSpace holdSpace;
    private float stepTime;
    private float lockTime;
    private float moveTime;    
    private bool usedHold;

    public void Initialize(Board board, Vector3Int position, TetrominoData data)
    {
        this.data = data;
        this.board = board;
        this.position = position;
        
        rotationIndex = 0;
        lockTime = 0f;

        if (cells == null)
        {
            cells = new Vector3Int[data.cells.Length];
        }

        for (int i = 0; i < data.cells.Length; i++)
        {
            cells[i] = (Vector3Int)data.cells[i];
        }
    }

    private void Update()
    {
        //Locks the movement of the piece during the line clearing animation
        if (board.IsClearingLines) return;
        
        board.Clear(this);

        //Allows to adjust the piece before it locks in place
        lockTime += Time.deltaTime;

        //Hold piece
        if (Keyboard.current.hKey.wasPressedThisFrame && !usedHold)
        {          

            holdSpace.HoldPiece(this);  //Saves and holds the new piece
            board.Clear(this);
            Vector3Int newSpawnPos = new(-1,position.y,0); //Provisional fix, center x and keep y. TODO: Find an alternative to also keep x
            board.SpawnPiece(newSpawnPos,holdSpace.isPieceHeld ? holdSpace.oldPiece : null); //Replaces the new piece by the previously held
            holdSpace.UpdateOldPiece();       
            usedHold = true;            
        }

        //Piece rotation Q: Left E: Right
        if (Keyboard.current.qKey.wasPressedThisFrame){
            Rotate(-1);                
        } else if (Keyboard.current.eKey.wasPressedThisFrame){
            Rotate(1);
        }     

        //Hard drop
        if (Keyboard.current.spaceKey.wasPressedThisFrame){
            HardDrop();
            if (board.IsClearingLines) return;
        }              
        
        moveTime += Time.deltaTime;
        //Allows horizontal movement A: Left, D: Right. Times
        //every movement to control movement speed
        if (moveTime >= moveDelay)
        {
            HandleMoveInputs();
            moveTime = 0f;
        }

        stepTime += Time.deltaTime;
        //Advance the piece to the next row
        if(stepTime >= LevelManager.levelManager.StepDelay){
            Step();
            stepTime = 0;
            if (board.IsClearingLines) return;
        }

        board.Set(this);        
    }

    private void HandleMoveInputs()
    {
        //Horizontal movement a: Left d: Right
        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed){
            Move(Vector2Int.left);
        }else if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed){
            Move(Vector2Int.right);
        }

        //Vertical movement
        if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed){
            if (Move(Vector2Int.down)){
                //Updates stepTime to prevent double movement
                stepTime = Time.deltaTime;
            }
        }
    }

    private void Step()
    {
        // stepTime = Time.time + LevelManager.levelManager.StepDelay;

        //Steps down a row
        Move(Vector2Int.down);

        //If the piece is inactive locks in place
        if (lockTime >= lockDelay){
            Lock();
        }
    }

    private void Lock()
    {
        board.Clear(this);
        board.Set(this);

        if(Ghost.ghostPiece != null)
        {
            Ghost.ghostPiece.Clear();
        }

        board.ClearLines(SpawnNext);
        usedHold = false;
    }

    private void SpawnNext()
    {
        board.SpawnPiece(board.defaultSpawnPosition);
    }

    //Applies the rotation matrix to each piece and rotates its position by 90 degrees
    private void Rotate(int direction)
    {
        int originalRotation = rotationIndex;
        rotationIndex = Wrap(rotationIndex + direction, 4);
        // Debug.Log(this.rotationIndex);

        ApplyRotationMatrix(direction);

        if (!TestWallKicks(rotationIndex, direction))
        {
            rotationIndex = originalRotation;
            ApplyRotationMatrix(-direction);
        }
    }

    private void ApplyRotationMatrix(int direction)
    {
        for (int i = 0; i < cells.Length; i++)
        {
            Vector3 cell = cells[i];

            int x, y;

            switch (data.tetromino)
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

            cells[i] = new Vector3Int(x,y,0);
        }        
    }

    private bool TestWallKicks(int rotationIndex, int rotationDirection)
    {
        int wallkickIdx = GetWallKickIdx(rotationIndex, rotationDirection);

        for (int i = 0; i < data.wallKicks.GetLength(1); i++)
        {
            Vector2Int translation = data.wallKicks[wallkickIdx,i];

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
        return Wrap(wallkickIdx, data.wallKicks.GetLength(0));
    }

    public int Wrap(int value, int validPositions)
    {
        return ((value % validPositions) + validPositions) % validPositions;
    }

    //Hard drops a piece
    private void HardDrop()
    {
        //Drops the piece to the bottom of the screen
        while (Move(Vector2Int.down)){
            continue;
        }

        //Locks the piece in place when it reaches the bottom
        Lock();
    }

    private bool Move(Vector2Int translation)
    {
        Vector3Int newPosition = position;
        newPosition.x += translation.x;
        newPosition.y += translation.y;

        bool isValid = board.IsValidPosition(this, newPosition);

        if (isValid)
        {
            position = newPosition;
            moveTime = Time.time + moveDelay;
            lockTime = 0f;
        }

        return isValid;
    }


}
