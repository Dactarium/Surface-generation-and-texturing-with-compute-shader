# Surface-generation-and-texturing-with-compute-shader
Unity mesh generation script also adds custom texture generation with compute shader

<details open>
  <summary><b>Example usage</b></summary>
<br>
  
  <details>
  <summary>SimpleExample.cs</summary>
  <br>

  ``` csharp
    public class SimpleExample : MonoBehaviour
    {
        public ComputeShader computeShader;
		public Material material;

        public int width = 10; 
        public int height = 10;
        public Vector2 scale = new Vector2(1, 1);
		
        GameObject surface;

        void Awake()
        {
            // (width * scale.x) x (height * scale.y) surface
            // verticies count = width * height
            surface = SurfaceGenerator.generate(computeShader, material, width, height, scale);
       }

    }
  ```
  </details>

  <details>
  <summary>TexturedExample.cs</summary>
  <br>

  ``` csharp
    public class TexturedExample : MonoBehaviour
    {
        public ComputeShader computeShader;
        public Texture2D texture;
		public Material material;

        public int width;
        public int height;
		
        GameObject surface;

        void Awake()
        {
            // Color per tile (if there is no texture it works for per pixel)
            Color[] colors = new Color[SurfaceGenerator.calculateColorsLength(width, height, Vector2.one)];

            // Setting random colors for visualisation
            for(int i = 0; i < colors.Length; i++){
                colors[i] = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f)); 
            }

            // (width) x (height) textured surface
            // verticies count = width * height
            surface = SurfaceGenerator.generate(computeShader, material, width, height, texture, colors);
       }

    }
  ```
  </details>
  
  <details>
  <summary>YExample.cs</summary>
  <br>

  ``` csharp
    public class YExample : MonoBehaviour
    {
        public ComputeShader computeShader;
		public Material material;

        public int width;
        public int height;


        GameObject surface;

        void Awake()
        {
          // Color per tile (if there is no texture it works for per pixel)
          Color[] colors = new Color[SurfaceGenerator.calculateColorsLength(width, height)];

          // Setting random colors for visualisation
          for(int i = 0; i < colors.Length; i++){
              colors[i] = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
          }

          // has to be width + 1 and height + 1 
          float[,] y = new float[width + 1, height + 1];

          // Setting random length between 0 and 1 for visualisation
          // You can use noise function there
          for(int x = 0; x < width + 1; x++){
              for(int z = 0; z < height + 1; z++){
                  y[x, z] = Random.Range(0f, 1f);
              }
          }

          // (width) x (height) colored surface 
          // verticies count = width * height
          surface = SurfaceGenerator.generate(computeShader, material, width, height, y, colors);
       }

    }
  ```
  </details>
</details>
