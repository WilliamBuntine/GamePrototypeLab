using UnityEngine;

public class AnimatedTexture : MonoBehaviour
{
    public int columns = 5;      // frames horizontally
    public int rows = 4;         // frames vertically
    public float framesPerSecond = 10f;

    private MeshRenderer rend;
    private int totalFrames;
    private float frameIndex;

    void Start()
    {
        rend = GetComponent<MeshRenderer>();
        totalFrames = columns * rows;
    }

    void Update()
    {
        frameIndex += framesPerSecond * Time.deltaTime;
        int index = (int)frameIndex % totalFrames;

        // Calculate UV offset
        float sizeX = 1f / columns;
        float sizeY = 1f / rows;
        int uIndex = index % columns;
        int vIndex = rows - 1 - (index / columns);

        rend.material.SetTextureScale("_MainTex", new Vector2(sizeX, sizeY));
        rend.material.SetTextureOffset("_MainTex", new Vector2(uIndex * sizeX, vIndex * sizeY));
    }
}
