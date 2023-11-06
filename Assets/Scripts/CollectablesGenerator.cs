using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;
using Random = UnityEngine.Random;

public class CollectablesGenerator : MonoBehaviour
{
    [SerializeField] private GeneratorSettings generatorSettings;
    [SerializeField] private Marker marker;
    [SerializeField] private SpriteRenderer bgRenderer;

    private Vector2Int _currentTileIndex;
    private Dictionary<CollectableSettings, ObjectPool<Collectable>> _poolsDict = new ();
    private float _currentRefreshCooldown;
    private readonly Dictionary<Vector2Int, List<Collectable>> _activeObjectsDict = new();
    private int _totalGenerationWeight;
    private Dictionary<Vector2Int, CollectableSettings> _generationTable;
    private readonly SpriteRenderer[] _bgRenderers = new SpriteRenderer[9];
    private bool _initialGenerationPassed;

    public void Initialize()
    {
        _bgRenderers[0] = bgRenderer;
        for (int i = 1; i < _bgRenderers.Length; i++)
        {
            _bgRenderers[i] = Instantiate(bgRenderer, transform);
        }
        
        foreach (var collectableSettings in generatorSettings.collectableSettingsList)
        {
            _poolsDict[collectableSettings] = new(
                () =>
                {
                    var collectable = Instantiate(collectableSettings.prefab);
                    collectable.SettingsRef = collectableSettings;
                    return collectable;
                }, 
                OnTakeFromPool, 
                OnReturnedToPool, 
                OnDestroyPoolObject);
        }

        _currentTileIndex = CalcMarkerTileIndex();
        RefreshActiveTiles();

        _initialGenerationPassed = true;
    }

    private void Update()
    {
        _currentRefreshCooldown -= Time.deltaTime;
        if (_currentRefreshCooldown > 0f)
        {
            return;
        }
        
        Vector2Int markerTileIndex = CalcMarkerTileIndex();
        if (_currentTileIndex.x == markerTileIndex.x && _currentTileIndex.y == markerTileIndex.y)
        {
            return;
        }

        _currentTileIndex = markerTileIndex;
        RefreshActiveTiles();
    }

    private void UpdateBackgroundPos()
    {
        int bgRendererIndex = 0;
        for (int i = -1; i < 2; i++)
        {
            for (int j = -1; j < 2; j++)
            {
                var tileIndex = _currentTileIndex + new Vector2Int(i, j);
                _bgRenderers[bgRendererIndex].transform.position = TileCenterByIndex(tileIndex);
                bgRendererIndex++;
            }
        }
    }

    private Vector2Int CalcMarkerTileIndex()
    {
        Vector2 markerPos = marker.transform.position;

        int tileX = 0;
        if (markerPos.x < -generatorSettings.GenerationTileSize.x / 2f ||
            markerPos.x > generatorSettings.GenerationTileSize.x / 2f)
        {
            tileX = Mathf.FloorToInt( (generatorSettings.GenerationTileSize.x / 2f + markerPos.x) / generatorSettings.GenerationTileSize.x);
        }

        int tileY = 0;
        if (markerPos.y <= -generatorSettings.GenerationTileSize.y / 3f ||
            markerPos.y >= 2f/3f * generatorSettings.GenerationTileSize.x)
        {
            tileY = Mathf.FloorToInt((generatorSettings.GenerationTileSize.y / 3f + markerPos.y) / generatorSettings.GenerationTileSize.y);
        }

        return new Vector2Int(tileX, tileY);
    }

    private void RefreshActiveTiles()
    {
        _currentRefreshCooldown = generatorSettings.RefreshCooldown;
        Debug.Log($"Refreshing tiles, current: {_currentTileIndex.x}, {_currentTileIndex.y}");
        
        List<Vector2Int> activeTiles = new ();
        for (int x = _currentTileIndex.x - 1; x < _currentTileIndex.x + 2; x++)
        {
            for (int y = _currentTileIndex.y - 1; y < _currentTileIndex.y + 2; y++)
            {
                var tileIndex = new Vector2Int(x, y);
                activeTiles.Add(tileIndex);
                if (!_activeObjectsDict.ContainsKey(tileIndex))
                {
                    FillTile(tileIndex);
                }
            }
        }
        
        
        
        List<Vector2Int> tilesToRelease = new ();
        foreach (var tileIndex in _activeObjectsDict.Keys)
        {
            if (!activeTiles.Contains(tileIndex))
            {
                tilesToRelease.Add(tileIndex);
            }
        }
        
        foreach (var tileIndex in tilesToRelease)
        {
            ReleaseTile(tileIndex);
        }

        UpdateBackgroundPos();
    }
    
    private Vector2 TileLeftBottomByIndex(Vector2Int tileIndex)
    {
        Vector2 tileLeftBottom = new Vector2(
            -generatorSettings.GenerationTileSize.x / 2f + tileIndex.x * generatorSettings.GenerationTileSize.x,
            -generatorSettings.GenerationTileSize.y / 3f + tileIndex.y * generatorSettings.GenerationTileSize.y);

        return tileLeftBottom;
    }
    
    private Vector2 TileCenterByIndex(Vector2Int tileIndex)
    {
        Vector2 tileLeftBottom = TileLeftBottomByIndex(tileIndex);

        return tileLeftBottom + generatorSettings.GenerationTileSize / 2f;
    }

    private void FillTile(Vector2Int tileIndex)
    {
        var collectables = new List<Collectable>();
        float gridCellSizeX = generatorSettings.GenerationTileSize.x / generatorSettings.TileGridWidth;
        float gridCellSizeY = generatorSettings.GenerationTileSize.y / generatorSettings.TileGridHeight;
        

        bool[,] lockedPoints = new bool[generatorSettings.TileGridWidth, generatorSettings.TileGridHeight];
        int lockedPointLengthX = lockedPoints.GetLength(0);
        int lockedPointLengthY = lockedPoints.GetLength(1);
        for (int y = 0; y < generatorSettings.TileGridHeight; y++)
        {
            for (int x = 0; x < generatorSettings.TileGridWidth; x++)
            {
                if (lockedPoints[x, y])
                {
                    continue;
                }
                
                var collectableSettings = SelectCollectableToGenerate();
                if (collectableSettings == null)
                {
                    lockedPoints[x, y] = true;
                    continue;
                }

                if (!_initialGenerationPassed && tileIndex == _currentTileIndex && collectableSettings.prefab.ScoreType == ScoreType.Negative)
                {
                    lockedPoints[x, y] = true;
                    continue;
                }

                bool noSpace = false;
                for (int i = x; i < x + collectableSettings.size; i++)
                {
                    for (int j = y; j < y + collectableSettings.size; j++)
                    {
                        if (i >= lockedPointLengthX || j >= lockedPointLengthY || lockedPoints[i, j])
                        {
                            noSpace = true;
                        }
                    }
                }

                if (noSpace)
                {
                    lockedPoints[x, y] = true;
                    continue;
                }
                
                for (int i = x; i < x + collectableSettings.size; i++)
                {
                    for (int j = y; j < y + collectableSettings.size; j++)
                    {
                        lockedPoints[i, j] = true;
                    }
                }
                
                var collectable = _poolsDict[collectableSettings].Get();
                collectables.Add(collectable);
                    
                var gridCellCenter = new Vector2(
                    gridCellSizeX * x + gridCellSizeX * collectableSettings.size / 2f,
                    gridCellSizeY * y + gridCellSizeY * collectableSettings.size / 2f);
                collectable.transform.position = 
                    TileLeftBottomByIndex(tileIndex) 
                    + gridCellCenter
                    + Random.Range(-collectableSettings.gridTileActiveSpaceCoefficient, collectableSettings.gridTileActiveSpaceCoefficient) 
                    * new Vector2(gridCellSizeX, gridCellSizeY);

                collectable.name = $"{collectable.name}_{x}_{y}";
            }
        }

        _activeObjectsDict[tileIndex] = collectables;
    }

    private CollectableSettings SelectCollectableToGenerate()
    {
        if (_totalGenerationWeight == 0)
        {
            _totalGenerationWeight = generatorSettings.collectableSettingsList.Sum(s => s.generationWeight) +
                                     generatorSettings.emptyWeight;
        }

        if (_generationTable == null)
        {
            _generationTable = new Dictionary<Vector2Int, CollectableSettings>();
            int highestRoll = 0;
            foreach (var collectableSettings in generatorSettings.collectableSettingsList)
            {
                var range = new Vector2Int(highestRoll, highestRoll + collectableSettings.generationWeight);
                _generationTable[range] = collectableSettings;
                highestRoll += collectableSettings.generationWeight;
            }

            _generationTable[new Vector2Int(highestRoll, highestRoll + generatorSettings.emptyWeight)] = null;
        }

        var roll = Random.Range(0, _totalGenerationWeight);
        var key = _generationTable.Keys.First(k => roll >= k.x && roll < k.y);
        return _generationTable[key];
    }

    private void ReleaseTile(Vector2Int tileIndex)
    {
        var collectables = _activeObjectsDict[tileIndex];
        foreach (var collectable in collectables)
        {
            _poolsDict[collectable.SettingsRef].Release(collectable);
        }

        _activeObjectsDict.Remove(tileIndex);
    }

    private void OnReturnedToPool(Collectable collectable)
    {
        collectable.gameObject.SetActive(false);
        collectable.OnReturnedToPool();
    }
    
    private void OnTakeFromPool(Collectable collectable)
    {
        collectable.gameObject.SetActive(true);
        collectable.OnTakeFromPool();
    }
    
    private void OnDestroyPoolObject(Collectable collectable)
    {
        Destroy(collectable.gameObject);
    }
    
    [Serializable]
    private class GeneratorSettings
    {
        public Vector2 GenerationTileSize = new (20f, 20f);
        public int TileGridWidth = 5;
        public int TileGridHeight = 4;
        public float RefreshCooldown = 10f;
        public int emptyWeight = 20;
        public List<CollectableSettings> collectableSettingsList;
    }

    [Serializable]
    public class CollectableSettings
    {
        public Collectable prefab;
        public int size = 1;
        public int generationWeight = 1;
        [Range(0f, 1f)] public float gridTileActiveSpaceCoefficient = 0.4f;
    }
}
