using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// This class controls the piece that's drawn inside the holding area
/// </summary>
public class HoldSpace : MonoBehaviour
{
    //Reference to the holding space tilemap
    public Tilemap Tilemap {get; private set;}

    //Reference to the last held piece inside the holding area
    public TetrominoData oldPiece{get; private set;}

    //Data of the piece that will replace the one currently held
    private TetrominoData newPiece;

    //Flag that indicates there's a piece inside the holding area
    public bool isPieceHeld{get; private set;}

    //Spawn position used to draw the piece inside the holding area
    [SerializeField] Vector3Int spawnPosition;

    //Awake function initializes the tilemap and the hold piece flag
    private void Awake() {
        Tilemap = GetComponentInChildren<Tilemap>();    
        isPieceHeld = false;
    }

    //This function clears the previously held piece and sets the new one
    public void HoldPiece(Piece piece)
    {
        Tilemap.ClearAllTiles();
        newPiece = piece.data;
        SetPiece(piece.data);                
    }

    //This function updates the newly held piece information and updates 
    //the hold piece flag
    public void UpdateOldPiece()
    {
        oldPiece = newPiece;
        isPieceHeld = true;
    }

    //This function draws the held piece inside the holding space
    private void SetPiece(TetrominoData data)
    {
        for (int i = 0; i < data.cells.Length; i++)
        {
            //Offset the piece position by the actual position of each piece
            Vector3Int tilePosition = (Vector3Int)data.cells[i] + spawnPosition;
            // Debug.Log($"Setting held piece at {tilePosition}");
            Tilemap.SetTile(tilePosition, data.tile);
        }        
    }

}
