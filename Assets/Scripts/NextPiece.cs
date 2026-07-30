using UnityEngine;
using UnityEngine.Tilemaps;

public class NextPiece : MonoBehaviour
{
    public Tilemap Tilemap {get; private set;}
    public TetrominoData oldPiece{get; private set;}
    private TetrominoData nextPiece;
    public static NextPiece preview;
    [SerializeField] Vector3Int spawnPosition;


    private void Awake() {
        Tilemap = GetComponentInChildren<Tilemap>();
    }

    public void SetPiecePreview(TetrominoData piece)
    {
        Tilemap.ClearAllTiles();
        nextPiece = piece;
        SetPiece(piece);                
    }

    public void UpdateOldPiece()
    {
        oldPiece = nextPiece;
    }

    private void SetPiece(TetrominoData data)
    {
        for (int i = 0; i < data.cells.Length; i++)
        {
            //Offset the piece position by the actual position of each piece
            Vector3Int tilePosition = (Vector3Int)data.cells[i] + spawnPosition;
            Debug.Log($"Setting held piece at {tilePosition}");
            Tilemap.SetTile(tilePosition, data.tile);
        }        
    }
}
