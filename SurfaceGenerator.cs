using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurfaceGenerator
{   
    public static GameObject generate(ComputeShader computeShader, Material material, int width, int height, Color[] colors){
        return generate(computeShader, material, width, height, null, null, null, colors);
    }

    public static GameObject generate(ComputeShader computeShader, Material material, int width, int height, Texture2D tileTexture){
        return generate(computeShader, material, width, height, null, null, tileTexture, null);
    }

    public static GameObject generate(ComputeShader computeShader, Material material, int width, int height, Texture2D tileTexture, Color[] colors){
        return generate(computeShader, material, width, height, null, null, tileTexture, colors);
    }

    public static GameObject generate(ComputeShader computeShader, Material material, int width, int height, Vector2 scale){
        return generate(computeShader, material, width, height, null, scale, null, null);
    }

    public static GameObject generate(ComputeShader computeShader, Material material, int width, int height, Vector2 scale, Color[] colors){
        return generate(computeShader, material, width, height, null, scale, null, colors);
    }

    public static GameObject generate(ComputeShader computeShader, Material material, int width, int height, Vector2 scale, Texture2D tileTexture){
        return generate(computeShader, material, width, height, null, scale, tileTexture, null);
    }

    public static GameObject generate(ComputeShader computeShader, Material material, int width, int height, Vector2 scale, Texture2D tileTexture, Color[] colors){
        return generate(computeShader, material, width, height, null, scale,  tileTexture,  colors);
    }

    public static GameObject generate(ComputeShader computeShader, Material material, int width, int height, float[,] y, Color[] colors){
        return generate(computeShader, material, width, height, y, null, null, colors);
    }

    public static GameObject generate(ComputeShader computeShader, Material material, int width, int height, float[,] y, Texture2D tileTexture){
        return generate(computeShader, material, width, height, y, null, tileTexture, null);
    }

    public static GameObject generate(ComputeShader computeShader, Material material, int width, int height, float[,] y, Texture2D tileTexture, Color[] colors){ 
        return generate(computeShader, material, width, height, y, null, tileTexture, colors);
    }

    public static GameObject generate(ComputeShader computeShader, Material material, int width, int height, float[,] y ,Vector2 scale, Color[] colors){ 
        return generate(computeShader, material, width, height, y, scale, null, colors);
    }

    public static GameObject generate(ComputeShader computeShader, Material material, int width, int height, float[,] y = null, Vector2? scale = null, Texture2D tileTexture = null, Color[] colors = null){
        
        
        if(!scale.HasValue) scale = Vector2.one;
        int[] textureSize = {(int)(width * scale.Value.x), (int)(height * scale.Value.y)};
        
        for(int i = 0; i < textureSize.Length; i++){
            if(textureSize[i] < 1) textureSize[i] = 1;
        }

        GameObject surface = new GameObject();

        // Components
        MeshFilter meshFilter = surface.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = surface.AddComponent<MeshRenderer>();
        
        Mesh mesh = generateMesh(width, height, y, scale);
        meshFilter.sharedMesh = mesh;

        Material textureMaterial = new Material(material);
        textureMaterial.mainTexture = generateTexture(computeShader, textureSize[0], textureSize[1], tileTexture, colors);
        meshRenderer.sharedMaterial = textureMaterial;

        return surface;
    }

    public static Mesh generateMesh(int width, int height, float[,] y = null, Vector2? scale = null){
        Mesh generatedMesh = new Mesh();
        
        if(!scale.HasValue) scale = Vector2.one;

        // Setup Vertices And UV

        Vector3[] vertices = new Vector3[(width + 1) * (height + 1)];

        for(int z = 0; z < height + 1; z++){
            for(int x = 0; x < width + 1; x++){
                vertices[z * (width + 1) + x] = new Vector3((x - (width / 2f)) * scale.Value.x, (y != null) ? y[x, z]: 0, (z - (height / 2f)) * scale.Value.y);
            }
        }

        generatedMesh.vertices = vertices;

        // Setup Triangles

        int[] triangles = new int[width * height * 6];

        for(int z = 0, i = 0, t = 0; z < height; z++, i++){
            for(int x = 0; x < width; x++, i++, t+=6){
                triangles[t] = i;
                triangles[t + 1] = triangles[t + 4] = width + i + 1;
                triangles[t + 2] = triangles[t + 3] = i + 1;
                triangles[t + 5] = width + i + 2;
            }
        }
       
        generatedMesh.triangles = triangles;
        generatedMesh.RecalculateNormals();

        // Setup UV

        Vector2[] uv = new Vector2[vertices.Length];

        for(int i = 0; i < uv.Length; i++){
            uv[i] = new Vector2((vertices[i].x / scale.Value.x + width / 2f) / width, (vertices[i].z / scale.Value.y + height / 2f) / height);
        }

        generatedMesh.uv = uv;

        return generatedMesh;
    }

    public static RenderTexture generateTexture(ComputeShader computeShader, int width, int height, Texture2D tileTexture, Color[] colors){
        int[] resolution = {1, 1};
        if(tileTexture == null){
            tileTexture = new Texture2D(1, 1);
        }

        resolution[0] = tileTexture.width;
        resolution[1] = tileTexture.height;

        if(colors == null){
            colors = new Color[width * height];
            
            for(int i = 0; i < colors.Length; i++)
                colors[i] = Color.white;
        } 

        RenderTexture renderTexture = new RenderTexture(width * resolution[0], height * resolution[1], 16);
        renderTexture.enableRandomWrite = true;
        renderTexture.filterMode = FilterMode.Point;
        renderTexture.Create();

        computeShader.SetTexture(0, "Result", renderTexture);

        computeShader.SetTexture(0, "TileTexture", tileTexture);

        computeShader.SetInt("TextureWidth", tileTexture.width);
        computeShader.SetInt("TextureHeight", tileTexture.height);

        ComputeBuffer computeBuffer = new ComputeBuffer(colors.Length, sizeof(float) * 4);
        computeBuffer.SetData(colors);

        computeShader.SetBuffer(0, "Colors", computeBuffer);
        computeShader.SetInt("Width", width - 1);

        int threadGroupX = (width * resolution[0]) / 8;
        int threadGroupY = (height * resolution[1]) / 8;
        threadGroupX += (threadGroupX == 0)? 1:0;
        threadGroupY += (threadGroupY == 0)? 1:0;

        computeShader.Dispatch(0, threadGroupX, threadGroupY, 1);

        computeBuffer.Release();

        return renderTexture;
    }

    public static int calculateColorsLength(int width, int height, Vector2? scale = null){
        if(!scale.HasValue) scale = Vector2.one;
        return (int)((width + 1) * (height + 1) * scale.Value.x * scale.Value.y);
    }

    public static Color[] createColorArray(int width, int height, Vector2? scale = null, Color? color = null){
        Color[] colors = new Color[calculateColorsLength(width, height, scale)];

        Color _color = (color.HasValue)? color.Value: Color.gray;
        
        for(int i = 0; i < colors.Length; i++){
            colors[i] = _color;
        }

        return colors;
    }

}
