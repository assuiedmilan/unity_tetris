using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class Board : MonoBehaviour
{
    [SerializeField] public TetrominoData[] tetrominos;
    [SerializeField] public Vector3Int spawnPosition;
    [SerializeField] public GameObject visualGrid;

    public Tilemap Tilemap { get; private set; }
    public Piece CurrentPiece { get; private set; }

    private RectInt _boundaries;

    private void Awake()
    {
        Tilemap = GetComponentInChildren<Tilemap>();
        CurrentPiece = GetComponentInChildren<Piece>();

        for (var i = 0; i < tetrominos.Length; i++)
        {
            tetrominos[i].Initialize();
        }

        var gridSize = visualGrid.GetComponent<SpriteRenderer>().size;
        var boardSize = new Vector2Int((int)gridSize.x, (int)gridSize.y);
        var boardLowerOrigin = new Vector2Int(
            -boardSize.x / 2,
            -boardSize.y / 2
        );
        _boundaries = new RectInt(boardLowerOrigin, boardSize);
    }

    private void Start()
    {
        SpawnPiece();
        /*SpawnDebugPiece();*/
    }

    public bool IsPositionInValid(Piece piece, Vector3Int newPieceCenterCoordinate)
    {
        // ReSharper disable once LoopCanBeConvertedToQuery: Performance
        // ReSharper disable once LoopCanBeConvertedToQuery
        foreach (var cell in piece.Cells)
        {
            var newOccupiedCell = cell + newPieceCenterCoordinate;

            if (IsCellPositionInValid(newOccupiedCell))
            {
                return true;
            }
        }

        return false;
    }

    private bool IsCellPositionInValid(Vector3Int cellToCheck)
    {
        return !_boundaries.Contains((Vector2Int)cellToCheck) || Tilemap.HasTile(cellToCheck);
    }

    public void SpawnPiece()
    {
        var shapeIndex = Random.Range(1, tetrominos.Length);
        CurrentPiece.Initialize(this, spawnPosition, tetrominos[shapeIndex]);
        
        if (IsPositionInValid(CurrentPiece, spawnPosition)) {
            GameOver();
        }
    }

    /*private void SpawnDebugPiece()
    {
        var shapeIndex = tetrominos[0];
        var debugPiece = gameObject.AddComponent<Piece>();
        debugPiece.Initialize(this, new Vector3Int(0,0,0), tetrominos[0]);
        DrawPiece(debugPiece);
    }*/

    public void ClearPiece(Piece piece)
    {
        UpdateTilemap(piece, null);
    }

    public void DrawPiece(Piece piece)
    {
        UpdateTilemap(piece, piece.TetrominoData.tile);
    }

    public void ClearLines()
    {
        var row = _boundaries.yMin;

        while (row < _boundaries.yMax)
        {
            if (IsLineFull(row))
            {
                LineClear(row);
                ShiftRowsDownFrom(row);
            }
            else
            {
                row++;
            }
        }
    }

    private void GameOver()
    {
        Tilemap.ClearAllTiles();
    }
    
    private void UpdateTilemap(Piece piece, TileBase tile)
    {
        foreach (var cell in piece.Cells)
        {
            var currentOccupiedCell = cell + piece.CenterCoordinates;
            Tilemap.SetTile(currentOccupiedCell, tile);
        }
    }

    private bool IsLineFull(int row)
    {
        for (var col = _boundaries.xMin; col < _boundaries.xMax; col++)
        {
            var position = new Vector3Int(col, row, 0);

            if (!Tilemap.HasTile(position))
            {
                return false;
            }
        }

        return true;
    }

    private void LineClear(int row)
    {
        for (var col = _boundaries.xMin; col < _boundaries.xMax; col++)
        {
            var position = new Vector3Int(col, row, 0);
            Tilemap.SetTile(position, null);
        }
    }

    private void ShiftRowsDownFrom(int row)
    {
        while (row < _boundaries.yMax)
        {
            for (var col = _boundaries.xMin; col < _boundaries.xMax; col++)
            {
                var position = new Vector3Int(col, row + 1, 0);
                var above = Tilemap.GetTile(position);

                position = new Vector3Int(col, row, 0);
                Tilemap.SetTile(position, above);
            }

            row++;
        }
    }
}
