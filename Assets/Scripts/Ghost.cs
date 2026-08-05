using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// This class controls the behaviour of the ghost piece used as a reference 
/// inside the board for the player to see where the current piece will land
/// </summary>
public class Ghost : MonoBehaviour
{
    //Defines the tile (color) used to draw the ghost piece
    [SerializeField] private Tile tile;

    //Reference to the game board
    [SerializeField] private Board board;

    //Reference to the current piece
    [SerializeField] private Piece trackingPiece;

    //Reference to the ghost piece that can be called from any other script
    public static Ghost ghostPiece;

    //Tilemap defined exclusively to draw the ghost piece
    public Tilemap tilemap{get; private set;}

    //Position of the ghost piece
    public Vector3Int position {get; private set;}

    //Cells that compose the ghost piece
    public Vector3Int[] cells {get; private set;}

    //Awake function. Initializes the ghost piece information. 
    //Uses a singleton pattern to control the ghost piece instance
    private void Awake()
    {
        tilemap = GetComponentInChildren<Tilemap>();
        cells = new Vector3Int[4];

        if(ghostPiece == null){
            ghostPiece = this;
        }
        else{
            Destroy(gameObject);
        }
    }

    //Late update function to update the ghost piece after any action 
    // performed on the main piece
    private void LateUpdate()
    {
        //The ghost piece is also locked if the board is clearing lines
        if (board.IsClearingLines) return;

        Clear();
        Copy();
        Drop();
        Set();
    }

    //Erases the ghost piece from the board
    public void Clear()
    {
        for (int i = 0; i < cells.Length; i++)
        {
            Vector3Int tilePosition = cells[i] + position;
            tilemap.SetTile(tilePosition, null);
        }           
    }

    //Copies the current piece's cells to draw the ghost piece
    private void Copy()
    {
        for (int i = 0; i < cells.Length; i++)
        {
            cells[i] = trackingPiece.cells[i];
        }
    }

    //Drops the ghost piece to the lowest possible position based on the main board tiles
    private void Drop()
    {
        //Gets the main piece's position
        Vector3Int position = trackingPiece.position;

        int current = position.y;
        int bottom = -board.boardSize.y/2 - 1;

        //Clears the current piece from the board
        board.Clear(trackingPiece);

        //Checks every row to find out if the ghost piece can be drawn in that position
        for (int row = current; row >= bottom; row--)
        {
            position.y = row;

            if(board.IsValidPosition(trackingPiece, position))
            {
                this.position = position;
            }
            else
            {
                break;
            }
        }

        //Draws again the current piece into the game board
        board.Set(trackingPiece);
    }

    //Sets the ghost piece in its tilemap
    private void Set()
    {
        for (int i = 0; i < cells.Length; i++)
        {
            Vector3Int tilePosition = cells[i] + position;
            tilemap.SetTile(tilePosition, tile);
        }        
    }
}
