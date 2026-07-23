using UnityEngine;
using UnityEngine.Tilemaps;

public class HoldSpace : MonoBehaviour
{
    public Tilemap Tilemap {get; private set;}
    public TetrominoData oldPiece{get; private set;}
    private TetrominoData newPiece;
    public bool isPieceHeld{get; private set;}
    public static HoldSpace holdSpace;
    [SerializeField] Vector3Int spawnPosition;


    private void Awake() {
        Tilemap = GetComponentInChildren<Tilemap>();    
        isPieceHeld = false;
    }

    public void HoldPiece(Piece piece)
    {
        Tilemap.ClearAllTiles();
        newPiece = piece.data;
        SetPiece(piece.data);                
    }

    public void UpdateOldPiece()
    {
        oldPiece = newPiece;
        isPieceHeld = true;
    }

    private void SetPiece(TetrominoData data)
    {
        for (int i = 0; i < data.cells.Length; i++)
        {
            //Offset the piece position by the actual position of each piece
            Vector3Int tilePosition = (Vector3Int)data.cells[i] + spawnPosition;
            Tilemap.SetTile(tilePosition, data.tile);
        }        
    }

}
