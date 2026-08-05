using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// This class controls the visualization of the next piece to be played
/// </summary>
public class NextPiece : MonoBehaviour
{
    //Reference to the piece preview tilemap
    public Tilemap Tilemap {get; private set;}

    //Information of the next piece
    public TetrominoData previewPiece{get; private set;}

    //Spawn position of the piec inside the piece preview tilemap
    [SerializeField] Vector3Int spawnPosition;


    //Awake function, initializes the tilemap
    private void Awake() {
        Tilemap = GetComponentInChildren<Tilemap>();
    }

    //Sets the next piece inside the preview area
    public void SetPiecePreview(TetrominoData piece)
    {
        Tilemap.ClearAllTiles();
        SetPiece(piece);                
    }

    //Updates the next piece data so it can be spawned after locking the current piece
    public void UpdatePreviewPiece(TetrominoData piece)
    {
        previewPiece = piece;
    }

    //Draws the piec preview in its tilemap
    private void SetPiece(TetrominoData data)
    {
        for (int i = 0; i < data.cells.Length; i++)
        {
            //Offset the piece position by the actual position of each piece
            Vector3Int tilePosition = (Vector3Int)data.cells[i] + spawnPosition;
            Debug.Log($"Setting piece preview at {tilePosition}");
            Tilemap.SetTile(tilePosition, data.tile);
        }        
    }
}
