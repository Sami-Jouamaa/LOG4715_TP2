
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ParallaxBackground : MonoBehaviour
{
    [SerializeField] Camera targetCamera;
    [SerializeField, Range(0f, 1f)] float parallaxFactor = 0.2f;

    SpriteRenderer[] tiles = new SpriteRenderer[3];

    Vector3 startCameraPosition;
    Vector3 startBackgroundPosition;

    float tileWidth;

    void Start()
    {
        startCameraPosition = targetCamera.transform.position;
        startBackgroundPosition = transform.position;

        FitBackground();

        tiles[0] = GetComponent<SpriteRenderer>();

        for (int i = 1; i < 3; i++)
        {
            GameObject tile = new GameObject("Background Tile");
            tile.transform.SetPositionAndRotation(transform.position, transform.rotation);
            tile.transform.localScale = transform.localScale;

            SpriteRenderer renderer = tile.AddComponent<SpriteRenderer>();
            SpriteRenderer original = tiles[0];

            renderer.sprite = original.sprite;
            renderer.color = original.color;
            renderer.sortingLayerID = original.sortingLayerID;
            renderer.sortingOrder = original.sortingOrder;
            renderer.flipX = original.flipX;
            renderer.flipY = original.flipY;
            renderer.sharedMaterial = original.sharedMaterial;

            tiles[i] = renderer;
        }

        tileWidth = tiles[0].bounds.size.z;
    }

    void LateUpdate()
    {
        Vector3 movement = targetCamera.transform.position - startCameraPosition;

        float backgroundZ = startBackgroundPosition.z + movement.z * parallaxFactor;

        // Recycle the tile group as the camera moves.
        backgroundZ += Mathf.Round(
            (targetCamera.transform.position.z - backgroundZ) / tileWidth
        ) * tileWidth;

        float backgroundY = startBackgroundPosition.y + movement.y * parallaxFactor;

        for (int i = 0; i < tiles.Length; i++)
        {
            Vector3 position = startBackgroundPosition;
            position.y = backgroundY;
            position.z = backgroundZ + (i - 1) * tileWidth;

            tiles[i].transform.position = position;
        }
    }

    void FitBackground()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();

        float distance = Mathf.Abs(transform.position.x - targetCamera.transform.position.x);
        float height = 2f * distance * Mathf.Tan(targetCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
        float width = height * targetCamera.aspect;

        float spriteWidth = sr.sprite.bounds.size.x;
        float spriteHeight = sr.sprite.bounds.size.y;

        float scale = Mathf.Max(width / spriteWidth, height / spriteHeight);

        transform.localScale = new Vector3(scale, scale, 1f);
    }
}
