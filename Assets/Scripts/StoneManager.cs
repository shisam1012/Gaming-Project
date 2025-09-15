using System.Collections.Generic;
using UnityEngine;

public class StoneManager : MonoBehaviour
{
    private static StoneManager _instance;
    public static StoneManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<StoneManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("StoneManager");
                    _instance = go.AddComponent<StoneManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }
    
    [Header("Stone Prefabs")]
    [SerializeField] private Stone[] stonePrefabs;
    
    [Header("Pooling Settings")]
    [SerializeField] private int initialPoolSize = 50;
    [SerializeField] private bool usePooling = true;
    
    [Header("Layer Settings")]
    [SerializeField] private string stoneLayerName = "Stone";
    private int stoneLayerIndex = -1;
    
    private Dictionary<Stone.StoneType, Queue<Stone>> stonePools = new Dictionary<Stone.StoneType, Queue<Stone>>();
    private List<Stone> activeStones = new List<Stone>();
    private Transform poolParent;
    
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        DontDestroyOnLoad(gameObject);
        
        stoneLayerIndex = LayerMask.NameToLayer(stoneLayerName);
        if (stoneLayerIndex == -1)
        {
            Debug.LogWarning($"[StoneManager] Layer '{stoneLayerName}' not found! Using Default layer. Please create a '{stoneLayerName}' layer in Project Settings > Tags and Layers.");
            stoneLayerIndex = 0;
        }
        else
        {
            Debug.Log($"[StoneManager] Using layer '{stoneLayerName}' (index: {stoneLayerIndex}) for all stones.");
        }
        
        InitializePooling();
    }
    
    private void InitializePooling()
    {
        if (!usePooling || stonePrefabs == null) return;
        
        GameObject poolParentObj = new GameObject("StonePool");
        poolParentObj.transform.SetParent(transform);
        poolParent = poolParentObj.transform;
        
        foreach (var stonePrefab in stonePrefabs)
        {
            if (stonePrefab != null)
            {
                stonePools[stonePrefab.Type] = new Queue<Stone>();
                
                for (int i = 0; i < initialPoolSize / stonePrefabs.Length; i++)
                {
                    Stone pooledStone = CreatePooledStone(stonePrefab);
                    stonePools[stonePrefab.Type].Enqueue(pooledStone);
                }
            }
        }
    }
    
    private Stone CreatePooledStone(Stone prefab)
    {
        Stone stone = Instantiate(prefab, poolParent);
        stone.gameObject.SetActive(false);
        
        SetStoneLayer(stone);
        
        return stone;
    }
    
    public Stone CreateStone(Stone.StoneType stoneType, Vector2 position, Transform parent = null)
    {
        Stone stone = null;
        
        if (stonePrefabs == null || stonePrefabs.Length == 0)
        {
            Debug.LogWarning("[StoneManager] No stone prefabs assigned! Cannot create stones.");
            return null;
        }
        
        if (usePooling && stonePools.ContainsKey(stoneType) && stonePools[stoneType].Count > 0)
        {
            stone = stonePools[stoneType].Dequeue();
            stone.transform.position = position;
            stone.transform.SetParent(parent);
            stone.gameObject.SetActive(true);
        }
        else
        {
            Stone prefab = GetStonePrefab(stoneType);
            if (prefab != null)
            {
                stone = Instantiate(prefab, position, Quaternion.identity, parent);
            }
        }
        
        if (stone != null)
        {
            activeStones.Add(stone);
            stone.Initialize();
            
            SetStoneLayer(stone);
        }
        
        return stone;
    }
    
    public Stone CreateRandomStone(Vector2 position, Transform parent = null)
    {
        if (stonePrefabs == null || stonePrefabs.Length == 0) return null;
        
        Stone.StoneType randomType = stonePrefabs[Random.Range(0, stonePrefabs.Length)].Type;
        return CreateStone(randomType, position, parent);
    }
    
    public void DestroyStone(Stone stone)
    {
        if (stone == null) return;
        
        activeStones.Remove(stone);
        
        if (usePooling && stonePools.ContainsKey(stone.Type))
        {
            stone.gameObject.SetActive(false);
            stone.transform.SetParent(poolParent);
            stone.ResetForPool();
            stonePools[stone.Type].Enqueue(stone);
        }
        else
        {
            Destroy(stone.gameObject);
        }
    }
    
    public void DestroyStone(GameObject stoneGameObject)
    {
        Stone stone = stoneGameObject.GetComponent<Stone>();
        if (stone != null)
        {
            DestroyStone(stone);
        }
        else
        {
            Destroy(stoneGameObject);
        }
    }
    
    public void DestroyAllActiveStones()
    {
        var stonesToDestroy = new List<Stone>(activeStones);
        foreach (var stone in stonesToDestroy)
        {
            DestroyStone(stone);
        }
        activeStones.Clear();
    }
    
    public Stone GetStonePrefab(Stone.StoneType stoneType)
    {
        if (stonePrefabs == null) return null;
        
        foreach (var prefab in stonePrefabs)
        {
            if (prefab != null && prefab.Type == stoneType)
            {
                return prefab;
            }
        }
        
        return null;
    }
    
    public Stone[] GetAllStonePrefabs()
    {
        return stonePrefabs;
    }
    
    public void SetStonePrefabs(Stone[] newPrefabs)
    {
        stonePrefabs = newPrefabs;
        
        if (usePooling)
        {
            ClearPools();
            InitializePooling();
        }
    }
    
    public void SetStoneLayerName(string layerName)
    {
        stoneLayerName = layerName;
        stoneLayerIndex = LayerMask.NameToLayer(stoneLayerName);
        if (stoneLayerIndex == -1)
        {
            Debug.LogWarning($"[StoneManager] Layer '{stoneLayerName}' not found! Using Default layer.");
            stoneLayerIndex = 0;
        }
        
        foreach (var stone in activeStones)
        {
            if (stone != null)
            {
                SetStoneLayer(stone);
            }
        }
    }
    
    private void ClearPools()
    {
        foreach (var pool in stonePools.Values)
        {
            while (pool.Count > 0)
            {
                Stone stone = pool.Dequeue();
                if (stone != null)
                {
                    Destroy(stone.gameObject);
                }
            }
        }
        stonePools.Clear();
    }
    
    public int GetActiveStoneCount()
    {
        return activeStones.Count;
    }
    
    private void SetStoneLayer(Stone stone)
    {
        if (stone != null && stone.gameObject != null)
        {
            stone.gameObject.layer = stoneLayerIndex;
            Debug.Log($"[StoneManager] Set stone {stone.name} to layer '{stoneLayerName}' (index: {stoneLayerIndex})");
        }
    }
    
    public int GetPooledStoneCount(Stone.StoneType stoneType)
    {
        if (stonePools.ContainsKey(stoneType))
        {
            return stonePools[stoneType].Count;
        }
        return 0;
    }
    
    public List<Stone> GetActiveStones()
    {
        return new List<Stone>(activeStones);
    }
    
    public List<Stone> GetActiveStonesByType(Stone.StoneType stoneType)
    {
        List<Stone> result = new List<Stone>();
        foreach (var stone in activeStones)
        {
            if (stone != null && stone.Type == stoneType)
            {
                result.Add(stone);
            }
        }
        return result;
    }
    
    public void LogPoolStatus()
    {
        Debug.Log($"StoneManager Status - Active: {activeStones.Count}");
        foreach (var kvp in stonePools)
        {
            Debug.Log($"Pool {kvp.Key}: {kvp.Value.Count} stones");
        }
    }
    
    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }
}