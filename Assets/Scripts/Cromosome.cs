using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using ProceduralNoiseProject;

[System.Serializable]
public class Cromosome 
{
    // The size of voxel array.
    static int width    = 32;
    static int height   = 2;
    static int length   = 32;

    public float       [] voxels;
    float              [] mutations;

    float       mutationChance = 0.2f;
    List<Vector3> navMeshVertices = new List<Vector3>();
    float       longestPathDistance = 0.0f;

    public Cromosome()
    {
        voxels      = new float [width * height * length];
        mutations   = new float [voxels.Length];
        
        int mutationSeed = UnityEngine.Random.Range(0, int.MaxValue);
        
        INoise perlin        = new PerlinNoise(mutationSeed, 1.0f);
        FractalNoise fractal = new FractalNoise(perlin, 2, 2.0f);

        //Fill voxels with values. Im using perlin noise but any method to create voxels will work.
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        for (int z = 0; z < length; z++)
                        {
                            float fx = x / (width - 1.0f);
                            float fy = y / (height - 1.0f);
                            float fz = z / (length - 1.0f);

                            int idx = x + y * width + z * width * height;

                            mutations[idx] = fractal.Sample3D(fx, fy, fz);
                        }
                    }
                }
    }


    /// <summary>
	/// Recombine two cromosomes obtaining a new cromosome with random 
	/// </summary>
    public Cromosome Recombine(Cromosome other)
    {
        Cromosome child = new Cromosome();
        int chunkSize = voxels.Length / 8;

        for(int i = 0; i < voxels.Length; i += chunkSize)
        {
            
            float combinatorial = UnityEngine.Random.Range(0.0f, 1.0f);
            
            if (combinatorial <= mutationChance)
            {
                for (int j = 0; j < chunkSize; ++j)
                {
                    child.GetVoxels()[i + j]   = Mutation(i+j);
                }
            }
            else if (combinatorial <= mutationChance + ((1-mutationChance) / 2))
            {
                for (int j = 0; j < chunkSize; ++j)
                {
                    child.GetVoxels()[i + j]   = this.GetVoxels()[i + j];
                }
            }
            else 
            {
                for (int j = 0; j < chunkSize; ++j)
                {
                    child.GetVoxels()[i+j]   = other.GetVoxels()[i+j];
                }
            }
        }

        return child;
    }

    public float [] GetVoxels() => voxels;
    public int      GetWidth () => width;
    public int      GetHeight() => height;
    public int      GetLength() => length;
    public void     SetVoxels(float [] voxels) => this.voxels = voxels;
    public float    Mutation(int i) => mutations[i];
    public float    GetLongestPathDistance() => longestPathDistance;
    public void     AddVertex(Vector3 vertex) => navMeshVertices.Add(vertex);

    public void CalculateLongestDistance(NavMeshAgent agent)
    {
        for(int i = 0; i < navMeshVertices.Count - 1 ; ++i)
        {
            for(int j = i; j < navMeshVertices.Count; ++j)
            {
                NavMeshPath path = new NavMeshPath();

                if(NavMesh.CalculatePath(navMeshVertices[i], navMeshVertices[j], NavMesh.AllAreas, path))
                {
                    agent.transform.position = navMeshVertices[i];
                    agent.SetDestination(navMeshVertices[j]);
                    
                    if(agent.remainingDistance > longestPathDistance && agent.remainingDistance != Mathf.Infinity)
                    {
                        longestPathDistance = agent.remainingDistance;
                    }
                }
            }
        }
        
    }
    
}
