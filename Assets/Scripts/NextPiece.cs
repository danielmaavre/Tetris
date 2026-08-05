using UnityEngine;
using UnityEngine.Tilemaps;

public class NextPiece : MonoBehaviour
{
    public Tilemap Tilemap {get; private set;}
    public TetrominoData nextPiece{get; private set;}
    public TetrominoData previewPiece{get; private set;}
    public static NextPiece preview;
    [SerializeField] Vector3Int spawnPosition;


    private void Awake() {
        Tilemap = GetComponentInChildren<Tilemap>();
    }

    public void SetPiecePreview(TetrominoData piece)
    {
        Debug.Log("Starting SetPiecePreview");
        Tilemap.ClearAllTiles();

        Debug.Log("Setting Next Piece");
        SetPiece(piece);                
    }

    public void UpdatePreviewPiece(TetrominoData piece)
    {
        Debug.Log("Updating preview");
        previewPiece = piece;
    }

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
