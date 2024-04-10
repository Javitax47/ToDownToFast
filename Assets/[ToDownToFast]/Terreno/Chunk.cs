using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class Chunk : MonoBehaviour
{
    public int width = 100;
    public int height = 100;
    public int obstacleDensity = 100;
    public float scale = 35f;
    public float heightMultiplier = 0.5f;
    public float secondLayerScale = 20f; 
    public float secondLayerHeightMultiplier = 0.2f; 

    public GameObject prefab1;
    public GameObject prefab2;
    public GameObject prefab3;

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private MeshCollider meshCollider;
    private Mesh mesh;

    public int seed;
    public int chunkX;
    public int chunkZ;

    // Define los límites del chunk
    private Vector3 chunkMinBounds;
    private Vector3 chunkMaxBounds;

    void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        meshCollider = GetComponent<MeshCollider>();
        mesh = new Mesh();
        meshFilter.mesh = mesh;
        UpdatePlane();
        GeneratePrefabs();
    }

    void UpdatePlane()
    {
        Vector3[] vertices = new Vector3[width * height];
        int[] triangles = new int[(width) * (height) * 6];

        int vertexIndex = 0;
        int triangleIndex = 0;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float xCoord = ((float)x - (width - 1) + (chunkX * (width - 1))) / (width - 1) * scale;
                float yCoord = ((float)y - (height - 1) + (chunkZ * (height - 1))) / (height - 1) * scale;
                float heightValue = Mathf.PerlinNoise(xCoord + seed, yCoord + seed) * heightMultiplier;

                // Agregar la segunda capa de ruido
                float secondLayerXCoord = ((float)x - (width - 1) + (chunkX * (width - 1))) / (width - 1) * secondLayerScale;
                float secondLayerYCoord = ((float)y - (height - 1) + (chunkZ * (height - 1))) / (height - 1) * secondLayerScale;
                float secondLayerHeightValue = Mathf.PerlinNoise(secondLayerXCoord + seed, secondLayerYCoord + seed) * secondLayerHeightMultiplier;

                // Sumar el valor de la segunda capa de ruido al valor original
                heightValue += secondLayerHeightValue;

                vertices[vertexIndex] = new Vector3(x - width/2f, heightValue, y - height/2f);

                if (x < width - 1 && y < height - 1)
                {
                    int topLeft = vertexIndex;
                    int topRight = vertexIndex + 1;
                    int bottomLeft = vertexIndex + width;
                    int bottomRight = vertexIndex + width + 1;

                    triangles[triangleIndex] = topLeft;
                    triangles[triangleIndex + 1] = bottomLeft;
                    triangles[triangleIndex + 2] = topRight;
                    triangles[triangleIndex + 3] = topRight;
                    triangles[triangleIndex + 4] = bottomLeft;
                    triangles[triangleIndex + 5] = bottomRight;

                    triangleIndex += 6;
                }

                vertexIndex++;
            }
        }

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        meshCollider.sharedMesh = null;
        meshCollider.sharedMesh = mesh;
    }


    void GeneratePrefabs()
    {
        // Calcula los límites del chunk
        CalculateChunkBounds();

        int prefabCount = obstacleDensity;

        for (int i = 0; i < prefabCount; i++)
        {
            // Genera una posición aleatoria dentro de los límites del chunk
            Vector3 position = new Vector3(
                Random.Range(chunkMinBounds.x, chunkMaxBounds.x),
                Random.Range(chunkMinBounds.y, chunkMaxBounds.y),
                Random.Range(chunkMinBounds.z, chunkMaxBounds.z)
            );

            RaycastHit hit;
            if (Physics.Raycast(new Vector3(position.x, 1000, position.z), Vector3.down, out hit, 10000))
            {
                position.y = hit.point.y;

                // Verifica si el objeto golpeado tiene el tag "Obstaculo"
                if (hit.collider.CompareTag("Obstaculo"))
                {
                    // En lugar de destruir el objeto golpeado, simplemente omite la instancia del prefab
                    continue;
                }
            }

            // Random prefab selection
            GameObject prefabToSpawn = null;
            int randomPrefabIndex = Random.Range(0, 3);
            switch (randomPrefabIndex)
            {
                case 0:
                    prefabToSpawn = prefab1;
                    break;
                case 1:
                    prefabToSpawn = prefab2;
                    break;
                case 2:
                    prefabToSpawn = prefab3;
                    break;
            }

            if (prefabToSpawn != null)
            {
                Instantiate(prefabToSpawn, position, Quaternion.identity, transform);
            }
        }
    }



    // Calcula los límites del chunk
    private void CalculateChunkBounds()
    {
        chunkMinBounds = transform.position - new Vector3((width - 20) / 0.133f , 0, (height - 20) / 0.133f);
        chunkMaxBounds = transform.position + new Vector3((width - 20) / 0.133f, 0, (height - 20) / 0.133f);
    }

}