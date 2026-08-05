using System;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(RectTransform))]
public class BouncingUIImages : MonoBehaviour
{
    [Serializable]
    private sealed class BouncingImageItem
    {
        [Tooltip("움직일 UI Image의 RectTransform입니다.")]
        public RectTransform image;

        [Min(0f)]
        public float speed = 180f;

        [Tooltip("처음 이동할 방향입니다. (0, 0)이면 자동으로 방향을 정합니다.")]
        public Vector2 startDirection = new Vector2(1f, 1f);

        [NonSerialized]
        public Vector2 velocity;
    }

    private struct Bounds2D
    {
        public float minX;
        public float maxX;
        public float minY;
        public float maxY;

        public Vector2 Center
        {
            get
            {
                return new Vector2(
                    (minX + maxX) * 0.5f,
                    (minY + maxY) * 0.5f);
            }
        }
    }

    [Header("이동 영역")]
    [SerializeField]
    [Tooltip("비워두면 이 스크립트가 붙은 RectTransform을 이동 영역으로 사용합니다.")]
    private RectTransform movementArea;

    [SerializeField, Min(0f)]
    [Tooltip("이미지가 화면 테두리에서 떨어질 여백입니다.")]
    private float edgePadding = 0f;

    [Header("움직일 이미지 목록 (6~7개 권장)")]
    [SerializeField]
    private BouncingImageItem[] movingImages =
    {
        new BouncingImageItem { speed = 180f, startDirection = new Vector2(1f, 1f) },
        new BouncingImageItem { speed = 165f, startDirection = new Vector2(-1f, 0.8f) },
        new BouncingImageItem { speed = 195f, startDirection = new Vector2(0.7f, -1f) },
        new BouncingImageItem { speed = 150f, startDirection = new Vector2(-0.8f, -1f) },
        new BouncingImageItem { speed = 175f, startDirection = new Vector2(1f, -0.6f) },
        new BouncingImageItem { speed = 185f, startDirection = new Vector2(-0.6f, 1f) },
        new BouncingImageItem { speed = 160f, startDirection = new Vector2(0.5f, 1f) }
    };

    [Header("충돌 설정")]
    [SerializeField, Range(0f, 1f)]
    [Tooltip("1이면 속도를 거의 잃지 않고 튕기고, 0이면 충돌 방향의 속도를 잃습니다.")]
    private float collisionBounciness = 1f;

    [SerializeField, Range(1, 8)]
    [Tooltip("값이 높을수록 빠른 이미지가 서로 통과할 가능성이 줄어듭니다.")]
    private int simulationSubsteps = 2;

    [SerializeField, Range(1, 8)]
    [Tooltip("겹친 이미지를 분리하는 반복 횟수입니다.")]
    private int collisionIterations = 3;

    [SerializeField]
    [Tooltip("메뉴에서 Time.timeScale이 0이어도 움직이게 합니다.")]
    private bool useUnscaledTime = true;

    private readonly Vector3[] worldCorners = new Vector3[4];

    private void Awake()
    {
        if (movementArea == null)
        {
            movementArea = GetComponent<RectTransform>();
        }
    }

    private void OnEnable()
    {
        InitializeVelocities();
    }

    private void Update()
    {
        if (movementArea == null || movingImages == null)
        {
            return;
        }

        float deltaTime = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        if (deltaTime <= 0f)
        {
            return;
        }

        int substeps = Mathf.Max(1, simulationSubsteps);
        float stepDeltaTime = deltaTime / substeps;

        for (int step = 0; step < substeps; step++)
        {
            MoveAllImages(stepDeltaTime);

            int iterations = Mathf.Max(1, collisionIterations);
            for (int iteration = 0; iteration < iterations; iteration++)
            {
                ResolveAllImageCollisions();
            }

            KeepAllImagesInsideArea();
        }
    }

    private void InitializeVelocities()
    {
        if (movingImages == null)
        {
            return;
        }

        for (int i = 0; i < movingImages.Length; i++)
        {
            BouncingImageItem item = movingImages[i];
            if (item == null)
            {
                continue;
            }

            float fallbackAngle = 37f + (137.5f * i);
            Vector2 fallbackDirection = new Vector2(
                Mathf.Cos(fallbackAngle * Mathf.Deg2Rad),
                Mathf.Sin(fallbackAngle * Mathf.Deg2Rad));

            Vector2 direction = GetValidDirection(item.startDirection, fallbackDirection);
            item.velocity = direction * Mathf.Max(0f, item.speed);
        }
    }

    private void MoveAllImages(float deltaTime)
    {
        for (int i = 0; i < movingImages.Length; i++)
        {
            BouncingImageItem item = movingImages[i];
            if (!IsUsable(item))
            {
                continue;
            }

            MoveInAreaSpace(item.image, item.velocity * deltaTime);
            ConstrainToMovementArea(item, true);
        }
    }

    private void ResolveAllImageCollisions()
    {
        for (int i = 0; i < movingImages.Length - 1; i++)
        {
            BouncingImageItem first = movingImages[i];
            if (!IsUsable(first))
            {
                continue;
            }

            for (int j = i + 1; j < movingImages.Length; j++)
            {
                BouncingImageItem second = movingImages[j];
                if (!IsUsable(second) || first.image == second.image)
                {
                    continue;
                }

                ResolveImageCollision(first, second, i, j);
            }
        }
    }

    private void ResolveImageCollision(
        BouncingImageItem first,
        BouncingImageItem second,
        int firstIndex,
        int secondIndex)
    {
        Bounds2D firstBounds = GetBoundsInMovementArea(first.image);
        Bounds2D secondBounds = GetBoundsInMovementArea(second.image);

        Vector2 firstCenter = firstBounds.Center;
        Vector2 secondCenter = secondBounds.Center;
        float firstHalfWidth = (firstBounds.maxX - firstBounds.minX) * 0.5f;
        float secondHalfWidth = (secondBounds.maxX - secondBounds.minX) * 0.5f;
        float firstHalfHeight = (firstBounds.maxY - firstBounds.minY) * 0.5f;
        float secondHalfHeight = (secondBounds.maxY - secondBounds.minY) * 0.5f;

        float overlapX = firstHalfWidth + secondHalfWidth
                         - Mathf.Abs(secondCenter.x - firstCenter.x);
        float overlapY = firstHalfHeight + secondHalfHeight
                         - Mathf.Abs(secondCenter.y - firstCenter.y);

        if (overlapX <= 0f || overlapY <= 0f)
        {
            return;
        }

        Vector2 collisionNormal;
        float penetration;

        if (overlapX < overlapY)
        {
            float sign = GetSeparationSign(
                secondCenter.x - firstCenter.x,
                second.velocity.x - first.velocity.x,
                firstIndex,
                secondIndex);
            collisionNormal = new Vector2(sign, 0f);
            penetration = overlapX;
        }
        else
        {
            float sign = GetSeparationSign(
                secondCenter.y - firstCenter.y,
                second.velocity.y - first.velocity.y,
                firstIndex,
                secondIndex);
            collisionNormal = new Vector2(0f, sign);
            penetration = overlapY;
        }

        // 겹침을 먼저 풀어줘야 같은 위치에서 계속 방향이 뒤집히는 떨림이 생기지 않습니다.
        Vector2 separation = collisionNormal * (penetration + 0.01f) * 0.5f;
        MoveInAreaSpace(first.image, -separation);
        MoveInAreaSpace(second.image, separation);

        Vector2 relativeVelocity = second.velocity - first.velocity;
        float velocityAlongNormal = Vector2.Dot(relativeVelocity, collisionNormal);

        // 이미 서로 멀어지는 중이라면 위치만 분리하고 속도는 다시 뒤집지 않습니다.
        if (velocityAlongNormal >= 0f)
        {
            return;
        }

        float impulseMagnitude = -(1f + collisionBounciness)
                                 * velocityAlongNormal * 0.5f;
        Vector2 impulse = collisionNormal * impulseMagnitude;

        first.velocity -= impulse;
        second.velocity += impulse;
    }

    private void KeepAllImagesInsideArea()
    {
        for (int i = 0; i < movingImages.Length; i++)
        {
            BouncingImageItem item = movingImages[i];
            if (IsUsable(item))
            {
                ConstrainToMovementArea(item, true);
            }
        }
    }

    private void ConstrainToMovementArea(BouncingImageItem item, bool reflectVelocity)
    {
        Rect areaRect = movementArea.rect;
        Bounds2D bounds = GetBoundsInMovementArea(item.image);

        float areaMinX = areaRect.xMin + edgePadding;
        float areaMaxX = areaRect.xMax - edgePadding;
        float areaMinY = areaRect.yMin + edgePadding;
        float areaMaxY = areaRect.yMax - edgePadding;

        float shiftX = 0f;
        float shiftY = 0f;
        float availableWidth = Mathf.Max(0f, areaMaxX - areaMinX);
        float availableHeight = Mathf.Max(0f, areaMaxY - areaMinY);
        float imageWidth = bounds.maxX - bounds.minX;
        float imageHeight = bounds.maxY - bounds.minY;

        if (imageWidth > availableWidth)
        {
            shiftX = ((areaMinX + areaMaxX) * 0.5f) - bounds.Center.x;
            item.velocity.x = 0f;
        }
        else if (bounds.minX < areaMinX)
        {
            shiftX = areaMinX - bounds.minX;
            if (reflectVelocity)
            {
                item.velocity.x = Mathf.Abs(item.velocity.x);
            }
        }
        else if (bounds.maxX > areaMaxX)
        {
            shiftX = areaMaxX - bounds.maxX;
            if (reflectVelocity)
            {
                item.velocity.x = -Mathf.Abs(item.velocity.x);
            }
        }

        if (imageHeight > availableHeight)
        {
            shiftY = ((areaMinY + areaMaxY) * 0.5f) - bounds.Center.y;
            item.velocity.y = 0f;
        }
        else if (bounds.minY < areaMinY)
        {
            shiftY = areaMinY - bounds.minY;
            if (reflectVelocity)
            {
                item.velocity.y = Mathf.Abs(item.velocity.y);
            }
        }
        else if (bounds.maxY > areaMaxY)
        {
            shiftY = areaMaxY - bounds.maxY;
            if (reflectVelocity)
            {
                item.velocity.y = -Mathf.Abs(item.velocity.y);
            }
        }

        if (!Mathf.Approximately(shiftX, 0f) || !Mathf.Approximately(shiftY, 0f))
        {
            MoveInAreaSpace(item.image, new Vector2(shiftX, shiftY));
        }
    }

    private Bounds2D GetBoundsInMovementArea(RectTransform image)
    {
        image.GetWorldCorners(worldCorners);

        Vector3 firstCorner = movementArea.InverseTransformPoint(worldCorners[0]);
        Bounds2D bounds = new Bounds2D
        {
            minX = firstCorner.x,
            maxX = firstCorner.x,
            minY = firstCorner.y,
            maxY = firstCorner.y
        };

        for (int i = 1; i < worldCorners.Length; i++)
        {
            Vector3 corner = movementArea.InverseTransformPoint(worldCorners[i]);
            bounds.minX = Mathf.Min(bounds.minX, corner.x);
            bounds.maxX = Mathf.Max(bounds.maxX, corner.x);
            bounds.minY = Mathf.Min(bounds.minY, corner.y);
            bounds.maxY = Mathf.Max(bounds.maxY, corner.y);
        }

        return bounds;
    }

    private void MoveInAreaSpace(RectTransform image, Vector2 movement)
    {
        Vector3 areaLocalPosition = movementArea.InverseTransformPoint(image.position);
        areaLocalPosition.x += movement.x;
        areaLocalPosition.y += movement.y;
        image.position = movementArea.TransformPoint(areaLocalPosition);
    }

    private static bool IsUsable(BouncingImageItem item)
    {
        return item != null
               && item.image != null
               && item.image.gameObject.activeInHierarchy;
    }

    private static Vector2 GetValidDirection(Vector2 direction, Vector2 fallback)
    {
        if (direction.sqrMagnitude < 0.0001f)
        {
            direction = fallback;
        }

        return direction.normalized;
    }

    private static float GetSeparationSign(
        float centerDifference,
        float velocityDifference,
        int firstIndex,
        int secondIndex)
    {
        if (Mathf.Abs(centerDifference) > 0.001f)
        {
            return Mathf.Sign(centerDifference);
        }

        if (Mathf.Abs(velocityDifference) > 0.001f)
        {
            return Mathf.Sign(velocityDifference);
        }

        return firstIndex <= secondIndex ? 1f : -1f;
    }

    private void OnValidate()
    {
        edgePadding = Mathf.Max(0f, edgePadding);
        simulationSubsteps = Mathf.Max(1, simulationSubsteps);
        collisionIterations = Mathf.Max(1, collisionIterations);

        if (movingImages == null)
        {
            return;
        }

        for (int i = 0; i < movingImages.Length; i++)
        {
            if (movingImages[i] != null)
            {
                movingImages[i].speed = Mathf.Max(0f, movingImages[i].speed);
            }
        }
    }
}