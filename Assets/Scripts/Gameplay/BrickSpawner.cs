using UnityEngine;

public class BrickSpawner : MonoBehaviour
{
    public float minInterval = 3f;
    public float maxInterval = 7f;
    public float yRangeHalf = 2.5f;
    public float brickSize = 0.2f;
    public float brickSpeed = 2f;
    public int minHealth = 1;
    public int maxHealth = 2;
    public float firstSpawnDelay = 5f;

    [Tooltip("How much bigger the microbe looks than its physical footprint. 1 = fills the cells exactly; 1.5 = bulges 50% past the colliders (like the ball). Collisions are unaffected.")]
    public float visualScale = 1.5f;

    [Tooltip("Chance a spawn is a single cube that slowly drifts downward (bouncing inside the paddles) instead of a normal side-moving formation.")]
    [Range(0f, 1f)]
    public float fallingCubeChance = 0.35f;

    [Tooltip("Falling cubes drift down at this fraction of their horizontal speed.")]
    public float fallSpeedRatio = 0.25f;

    private Sprite _whiteSquare;
    // Microbe textures, loaded from Resources/Microbes/{Round,RodH,RodV,Ball}.
    // Round/Rod are sliced across a whole formation; Ball is a smooth coccus
    // (no flagella) used whole, one per cell, for cluster-style formations.
    private Texture2D[] _round;
    private Texture2D[] _rodH;
    private Texture2D[] _rodV;
    private Texture2D[] _ball;
    private float _nextSpawnTime;

    // How a group of cells is skinned.
    private enum SkinMode
    {
        Rod,    // a single rod microbe sliced across the cells (orientation by aspect)
        Round,  // a single round microbe sliced across the cells
        Ball,   // each cell gets its own whole smooth ball
    }

    private struct Part
    {
        public SkinMode mode;
        public Vector2Int[] cells;
    }

    private static Vector2Int V(int x, int y)
    {
        return new Vector2Int(x, y);
    }

    private static Part Mk(SkinMode mode, params Vector2Int[] cells)
    {
        return new Part { mode = mode, cells = cells };
    }

    // Each formation is a recipe: one or more parts, each with its own skin mode.
    // Some recipes randomise themselves at spawn time (e.g. the plus).
    private static readonly System.Func<Part[]>[] Formations =
    {
        // Square 2x2 -> one round microbe sliced across it.
        () => new[] { Mk(SkinMode.Round, V(0, 0), V(1, 0), V(0, 1), V(1, 1)) },
        // Horizontal line -> horizontal rod.
        () => new[] { Mk(SkinMode.Rod, V(0, 0), V(1, 0), V(2, 0)) },
        // Vertical line -> vertical rod.
        () => new[] { Mk(SkinMode.Rod, V(0, 0), V(0, 1), V(0, 2)) },
        // L -> vertical rod (the column) + a ball on the foot.
        () => new[] { Mk(SkinMode.Rod, V(0, 0), V(0, 1), V(0, 2)), Mk(SkinMode.Ball, V(1, 0)) },
        // T -> horizontal rod (the bar) + a ball on the stem.
        () => new[] { Mk(SkinMode.Rod, V(0, 1), V(1, 1), V(2, 1)), Mk(SkinMode.Ball, V(1, 0)) },
        // Plus -> randomly a single round microbe, or a clump of balls.
        () => PlusRecipe(),
    };

    private static Part[] PlusRecipe()
    {
        Vector2Int[] cells = { V(1, 0), V(0, 1), V(1, 1), V(2, 1), V(1, 2) };
        if (Random.value < 0.5f)
        {
            return new[] { Mk(SkinMode.Round, cells) };
        }

        Part[] parts = new Part[cells.Length];
        for (int i = 0; i < cells.Length; i++)
        {
            parts[i] = Mk(SkinMode.Ball, cells[i]);
        }
        return parts;
    }

    void Start()
    {
        CreateSprite();
        LoadMicrobes();
        _nextSpawnTime = Time.time + firstSpawnDelay;
    }

    void Update()
    {
        if (Time.time >= _nextSpawnTime)
        {
            Spawn();
            ScheduleNext();
        }
    }

    void CreateSprite()
    {
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        _whiteSquare = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
    }

    void LoadMicrobes()
    {
        _round = Resources.LoadAll<Texture2D>("Microbes/Round");
        _rodH = Resources.LoadAll<Texture2D>("Microbes/RodH");
        _rodV = Resources.LoadAll<Texture2D>("Microbes/RodV");
        _ball = Resources.LoadAll<Texture2D>("Microbes/Ball");
    }

    void ScheduleNext()
    {
        _nextSpawnTime = Time.time + Random.Range(minInterval, maxInterval);
    }

    void Spawn()
    {
        if (Random.value < fallingCubeChance)
        {
            // A single cube (one round microbe) that slowly drifts downward.
            SpawnCluster(new[] { Mk(SkinMode.Round, V(0, 0)) }, true);
        }
        else
        {
            SpawnCluster(Formations[Random.Range(0, Formations.Length)](), false);
        }
    }

    void SpawnCluster(Part[] parts, bool falling)
    {
        bool goingRight = Random.value > 0.5f;

        int minCol = int.MaxValue;
        int maxCol = int.MinValue;
        int minRow = int.MaxValue;
        int maxRow = int.MinValue;
        foreach (Part part in parts)
        {
            foreach (Vector2Int cell in part.cells)
            {
                minCol = Mathf.Min(minCol, cell.x);
                maxCol = Mathf.Max(maxCol, cell.x);
                minRow = Mathf.Min(minRow, cell.y);
                maxRow = Mathf.Max(maxRow, cell.y);
            }
        }

        float shapeHalfWidth = (maxCol - minCol + 1) * brickSize / 2f;
        float shapeHalfHeight = (maxRow - minRow + 1) * brickSize / 2f;

        float leftInner, rightInner;
        var wallLeft = GameObject.Find("WallLeft");
        var wallRight = GameObject.Find("WallRight");
        if (wallLeft != null && wallRight != null)
        {
            leftInner = wallLeft.transform.position.x + wallLeft.transform.localScale.x / 2f + shapeHalfWidth;
            rightInner = wallRight.transform.position.x - wallRight.transform.localScale.x / 2f - shapeHalfWidth;
        }
        else
        {
            float camHalfW = Camera.main.orthographicSize * Camera.main.aspect;
            leftInner = -camHalfW + shapeHalfWidth;
            rightInner = camHalfW - shapeHalfWidth;
        }

        float startX = goingRight ? leftInner : rightInner;
        float yMax = Mathf.Max(0f, yRangeHalf - shapeHalfHeight);
        float y = Random.Range(-yMax, yMax);

        GameObject clusterGO = new GameObject("BrickCluster");
        clusterGO.transform.position = new Vector3(startX, y, 0f);

        // Falling cubes get a random speed between half and full, so they drift
        // at varying rates; normal formations keep the base speed.
        float speedMul = falling ? Random.Range(0.5f, 1f) : 1f;
        var cluster = clusterGO.AddComponent<BrickCluster>();
        cluster.speed = brickSpeed * speedMul;
        cluster.direction = goingRight ? Vector2.right : Vector2.left;
        if (falling)
        {
            cluster.fallSpeed = cluster.speed * fallSpeedRatio;
        }

        // The whole structure shares a single health roll so it spawns a uniform
        // colour; individual pieces then redden as they are hit.
        int clusterHealth = Random.Range(minHealth, maxHealth + 1);
        float centerCol = (minCol + maxCol) / 2f;
        float centerRow = (minRow + maxRow) / 2f;

        foreach (Part part in parts)
        {
            RenderPart(clusterGO.transform, part, centerCol, centerRow, clusterHealth);
        }
    }

    void RenderPart(Transform cluster, Part part, float centerCol, float centerRow, int health)
    {
        // The part's own bounding box: rods/round are sliced across this grid.
        int pMinCol = int.MaxValue;
        int pMaxCol = int.MinValue;
        int pMinRow = int.MaxValue;
        int pMaxRow = int.MinValue;
        foreach (Vector2Int cell in part.cells)
        {
            pMinCol = Mathf.Min(pMinCol, cell.x);
            pMaxCol = Mathf.Max(pMaxCol, cell.x);
            pMinRow = Mathf.Min(pMinRow, cell.y);
            pMaxRow = Mathf.Max(pMaxRow, cell.y);
        }
        int pCols = pMaxCol - pMinCol + 1;
        int pRows = pMaxRow - pMinRow + 1;

        Texture2D sliced = null;
        if (part.mode == SkinMode.Round)
        {
            sliced = RandFrom(_round);
        }
        else if (part.mode == SkinMode.Rod)
        {
            sliced = pCols >= pRows ? RandFrom(_rodH) : RandFrom(_rodV);
        }

        foreach (Vector2Int cell in part.cells)
        {
            Vector3 cellPos = new Vector3((cell.x - centerCol) * brickSize, (cell.y - centerRow) * brickSize, 0f);

            Sprite sprite;
            int w;
            int h;
            if (part.mode == SkinMode.Ball)
            {
                Texture2D ball = RandFrom(_ball);
                if (ball != null)
                {
                    w = ball.width;
                    h = ball.height;
                    sprite = Sprite.Create(ball, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f);
                }
                else
                {
                    w = 1;
                    h = 1;
                    sprite = _whiteSquare;
                }
            }
            else if (sliced != null)
            {
                // Slice this cell's piece out of the microbe. Texture space has
                // y=0 at the bottom, so the row index counts up from the bottom.
                int cx = cell.x - pMinCol;
                int cy = cell.y - pMinRow;
                int x0 = Mathf.RoundToInt((float)cx * sliced.width / pCols);
                int x1 = Mathf.RoundToInt((float)(cx + 1) * sliced.width / pCols);
                int y0 = Mathf.RoundToInt((float)cy * sliced.height / pRows);
                int y1 = Mathf.RoundToInt((float)(cy + 1) * sliced.height / pRows);
                w = Mathf.Max(1, x1 - x0);
                h = Mathf.Max(1, y1 - y0);
                sprite = Sprite.Create(sliced, new Rect(x0, y0, w, h), new Vector2(0.5f, 0.5f), 100f);
            }
            else
            {
                w = 1;
                h = 1;
                sprite = _whiteSquare;
            }

            CreateBrick(cluster, cellPos, sprite, w, h, health);
        }
    }

    // Builds one brick: a brickSize collider at the grid cell, with the visual
    // sprite on a child that is scaled up by visualScale and pushed out from the
    // cluster centre, so adjacent slices still tile while the microbe overflows
    // the collider grid. The physical footprint stays one brickSize per cell.
    void CreateBrick(Transform cluster, Vector3 cellPos, Sprite sprite, int spriteW, int spriteH, int health)
    {
        GameObject go = new GameObject("Brick");
        go.transform.SetParent(cluster);
        go.transform.localPosition = cellPos;

        var col = go.AddComponent<BoxCollider2D>();
        col.size = new Vector2(brickSize, brickSize);

        var rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.freezeRotation = true;

        GameObject skin = new GameObject("Skin");
        skin.transform.SetParent(go.transform);
        skin.transform.localPosition = cellPos * (visualScale - 1f);
        skin.transform.localScale = new Vector3(visualScale * brickSize * 100f / spriteW, visualScale * brickSize * 100f / spriteH, 1f);

        var sr = skin.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;

        var brick = go.AddComponent<Brick>();
        brick.health = health;
        brick.maxHealth = maxHealth;
        brick.selfMove = false;
    }

    Texture2D RandFrom(Texture2D[] set)
    {
        if (set == null || set.Length == 0)
        {
            return null;
        }
        return set[Random.Range(0, set.Length)];
    }
}
