// FILE: Stone.cs
using UnityEngine;

/// <summary>
/// Stone is now a VIEW-ONLY component that only handles visuals and data representation.
/// All game logic has been moved to InteractionController and other managers.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class Stone : MonoBehaviour
{
    public enum StoneType
    {
        Type1,
        Type2,
        Type3,
        Type4
    }

    [Header("Stone Data")]
    [SerializeField] private StoneType type;
    public StoneType Type => type;

    [Header("Position")]
    [HideInInspector] public int column;
    [HideInInspector] public int row;

    [Header("Visual Components")]
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D boxCollider;
    private Color originalColor;

    [Header("Animation")]
    [SerializeField] private float highlightMultiplier = 1.5f;
    [SerializeField] private float animationSpeed = 5f;
    

    private bool isHighlighted = false;
    private Color targetColor;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();
        
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
            targetColor = originalColor;
        }

        SetupCollider();
    }

    private void Start()
    {

        column = Mathf.RoundToInt(transform.position.x);
        row = Mathf.RoundToInt(transform.position.y);


        var position = transform.position;
        if (Mathf.Abs(position.z) > 0.0001f)
        {
            transform.position = new Vector3(position.x, position.y, 0f);
        }
    }

    private void Update()
    {

        if (spriteRenderer != null && spriteRenderer.color != targetColor)
        {
            spriteRenderer.color = Color.Lerp(spriteRenderer.color, targetColor, Time.deltaTime * animationSpeed);
        }
    }

    private void SetupCollider()
    {
        if (spriteRenderer != null && boxCollider != null)
        {
            boxCollider.isTrigger = false;
            

            if (boxCollider.size.sqrMagnitude < 0.0001f)
            {
                boxCollider.size = spriteRenderer.bounds.size;
                boxCollider.offset = Vector2.zero;
            }
        }
    }


    public void Initialize()
    {
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
            targetColor = originalColor;
            isHighlighted = false;
        }
        
        SetupCollider();
    }


    public void ResetForPool()
    {
        ResetHighlight();
        column = 0;
        row = 0;
        isHighlighted = false;
    }


    public void Highlight()
    {
        if (spriteRenderer == null) return;
        
        isHighlighted = true;
        targetColor = originalColor * highlightMultiplier;
    }

    public void ResetHighlight()
    {
        if (spriteRenderer == null) return;
        
        isHighlighted = false;
        targetColor = originalColor;
        // Immediately apply the color change instead of waiting for lerp
        spriteRenderer.color = originalColor;
    }

    public void SetHighlightColor(Color color)
    {
        if (spriteRenderer == null) return;
        
        targetColor = color;
    }

    public void SetOriginalColor(Color color)
    {
        originalColor = color;
        if (!isHighlighted)
        {
            targetColor = originalColor;
        }
    }

    public void SetGridPosition(int newColumn, int newRow)
    {
        column = newColumn;
        row = newRow;
    }

    public void SetWorldPosition(Vector2 worldPosition)
    {
        transform.position = new Vector3(worldPosition.x, worldPosition.y, 0f);
        column = Mathf.RoundToInt(worldPosition.x);
        row = Mathf.RoundToInt(worldPosition.y);
    }

    public void SetGridAndWorldPosition(int newColumn, int newRow)
    {
        column = newColumn;
        row = newRow;
        transform.position = new Vector3(newColumn, newRow, 0f);
    }


    public void SetType(StoneType newType)
    {
        type = newType;
    }


    public Vector2Int GridPosition => new Vector2Int(column, row);
    public Vector2 WorldPosition => new Vector2(transform.position.x, transform.position.y);
    public bool IsHighlighted => isHighlighted;
    public Color OriginalColor => originalColor;
    public Color CurrentColor => spriteRenderer != null ? spriteRenderer.color : Color.white;


    public SpriteRenderer SpriteRenderer => spriteRenderer;
    public BoxCollider2D BoxCollider => boxCollider;


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, Vector3.one * 0.9f);
        

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(new Vector3(column, row, 0), 0.1f);
    }


    private void Reset()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();
        SetupCollider();
    }
}
