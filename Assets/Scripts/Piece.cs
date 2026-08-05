using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// This class controls the behaviour of the piece currently controlled by the player
/// </summary>
public class Piece : MonoBehaviour
{  
    //Reference to the game board
    public Board board;

    //Reference to the tetromino being played
    public TetrominoData data {get; private set;}

    //Position of the current piece
    public Vector3Int position {get; private set;}

    //Cells that define the current tetromino
    public Vector3Int[] cells {get; private set;}

    //Index that defines the rotation direction of the piece
    public int rotationIndex {get; private set;}

    //Parameter that controls the speed at which a piece can move horizontally
    [SerializeField] private float moveDelay = 0.15f;

    //Time delay that the game will wait until a piece is locked
    [SerializeField] private float lockDelay = 0.5f;

    //Reference to the holdin area where the held piece will be drawn
    [SerializeField] private HoldSpace holdSpace;

    //Reference to the area where the next piece preview will be drawn
    [SerializeField] private NextPiece nextPiece;

    //Counters used to control if the move and lock delays are done
    private float stepTime;
    private float lockTime;
    private float moveTime;    

    //Flag that allows the player to use the hold action once per piece
    private bool usedHold;

    //Function used to initialize the current piece on the board
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

    //Update function, controls the game flow
    private void Update()
    {
        //Locks the movement of the piece during the line clearing animation
        if (board.IsClearingLines) return;
        
        //Clears the piece's tiles to update its position
        board.Clear(this);

        //Allows to adjust the piece before it locks in place
        lockTime += Time.deltaTime;

        //Hold piece action is controlled by the W key and can only be performed once 
        // before the current piece locks into place
        if (Keyboard.current.hKey.wasPressedThisFrame && !usedHold)
        { 
            HoldPiece();
        }

        //Piece rotation Q: Left E: Right
        if (Keyboard.current.qKey.wasPressedThisFrame){
            Rotate(-1);                
        } else if (Keyboard.current.eKey.wasPressedThisFrame){
            Rotate(1);
        }     

        //Hard drop controlled by the space bar
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

    //Function that controls the hold piece action
    private void HoldPiece()
    {
        //Defines the spawning position for the piece based on the current piece position
        //only uses the current piece's y position to prevent bugs regarding the board boundaries 
        // (can be improved to use the piece's x position)
        Vector3Int newSpawnPos = new(-1,position.y,0); 

        //Saves and holds the new piece
        holdSpace.HoldPiece(this); 

        //Clears from the board the piece that will be held 
        board.Clear(this);    

        //Updates the hold flag to prevent the action to be executed again
        usedHold = true;
        
        //Checks if there is already a piece in the holding space
        if (holdSpace.isPieceHeld)
        {    
            //If so, draws the held piece in the new position
            board.SpawnPiece(newSpawnPos,holdSpace.oldPiece); 
        }
        else
        {
            //If there's not a held piece, it behaves as if spawning a new piece
            SpawnNext();
        }    

        //Updates the held piece's information so it can be swapped the next time
        holdSpace.UpdateOldPiece(); 
    }

    //Function that controls the piece's horizontal and vertical movement
    private void HandleMoveInputs()
    {
        //Horizontal movement a: Left d: Right
        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed){
            Move(Vector2Int.left);
        }else if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed){
            Move(Vector2Int.right);
        }

        //Vertical movement s key
        if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed){
            if (Move(Vector2Int.down)){
                //Updates stepTime to prevent double movement
                stepTime = Time.deltaTime;
            }
        }
    }

    //Function that makes the piece step down one tile very time it's called
    private void Step()
    {     
        //Steps down a row
        Move(Vector2Int.down);

        //If the piece is inactive locks in place
        if (lockTime >= lockDelay){
            Lock();
        }
    }

    //Function in charge of locking the piece into place if the piece is still for longer than the lockDelay parameter
    private void Lock()
    {
        //Clears and sets the same piece to draw it still into place
        board.Clear(this);
        board.Set(this);

        //If there's an active ghost piece, clears it
        if(Ghost.ghostPiece != null)
        {
            Ghost.ghostPiece.Clear();
        }

        //Calls the clear lines function to check if there's any full line to erase, 
        //sends the SpawnNext function as a callback to spawn a new piece after checking for rows to clear
        board.ClearLines(SpawnNext);

        //After locking the piece into place allows the player to use the hold action again
        usedHold = false;
    }

    //Function in charge of determining which piece will spawn next
    public void SpawnNext()
    {
        //Generates a random tetromino to display as the next piece in the preview area
        TetrominoData newPiece = board.GenRandomPiece();

        //Sets the new piece inside the preview area
        nextPiece.SetPiecePreview(newPiece);

        //Checks if there is a piece held in the preview area
        if (nextPiece != null && (nextPiece.previewPiece.cells?.Length ?? 0) > 0)
        {
            //If so, spawns the preview piece into the game board
            board.SpawnPiece(board.defaultSpawnPosition,nextPiece.previewPiece);            
        }
        else
        {
            //Otherwise (its the first iteration) spawns a random piece into the board
            board.SpawnPiece(board.defaultSpawnPosition,board.GenRandomPiece());
        }
        
        //Saves the information of the piece preview so it can be drawn during the next iteration
        nextPiece.UpdatePreviewPiece(newPiece);
    }

    //Applies the rotation matrix to each piece and rotates its position by 90 degrees
    private void Rotate(int direction)
    {
        //Gets the peice's original rotation index
        int originalRotation = rotationIndex;

        //Calculates the new rotation index by adding the direction to the current rotation index
        rotationIndex = Wrap(rotationIndex + direction, 4);
        // Debug.Log(this.rotationIndex);

        //Applies the rotation matrix to the piece
        ApplyRotationMatrix(direction);

        //Checks the piece's wall kicks
        if (!TestWallKicks(rotationIndex, direction))
        {
            rotationIndex = originalRotation;
            ApplyRotationMatrix(-direction);
        }
    }

    //Function that applies the rotation matrix to the current piece
    private void ApplyRotationMatrix(int direction)
    {
        //Loops over each cell of the current piece
        for (int i = 0; i < cells.Length; i++)
        {
            Vector3 cell = cells[i];

            int x, y;

            //Checks the rotation matrix to be applied depending if its the I/O tetromino or any other
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

    //Applies the wall kick rule to the piece to check if the rotation is possible
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

    //Gets the wall kicks that will apply to the tetromino and checks if the new rotation is possible
    private int GetWallKickIdx(int rotationIndex, int rotationDirection)
    {
        int wallkickIdx = rotationIndex * 2;

        if (rotationDirection < 0)
        {
            wallkickIdx--;
        }
        return Wrap(wallkickIdx, data.wallKicks.GetLength(0));
    }

    //Function created to control any possible rotation index for a piece, it should rotate within 4 possible positions
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

    //Checks if the horizontal movement is possible.
    // If not, the piece won't translate
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
