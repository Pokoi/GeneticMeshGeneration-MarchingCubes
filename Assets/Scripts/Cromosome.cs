/*
 * File: Cromosome.cs
 * File Created: Sunday, 2nd February 2020 7:06:15 pm
 * ––––––––––––––––––––––––
 * Author: Jesus Fermin, 'Pokoi', Villar  (hello@pokoidev.com)
 * ––––––––––––––––––––––––
 * MIT License
 * 
 * Copyright (c) 2020 Pokoidev
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of
 * this software and associated documentation files (the "Software"), to deal in
 * the Software without restriction, including without limitation the rights to
 * use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
 * of the Software, and to permit persons to whom the Software is furnished to do
 * so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using ProceduralNoiseProject;


public class Cromosome 
{
    // The size of voxel array.
    static int width    = 32;
    static int height   = 2;
    static int length   = 32;

    
    public float       []   voxels;
    float              []   mutations;

    float                   mutationChance  = 0.20f;
    List<Vector3>           navMeshVertices = new List<Vector3>();
    float                   longestPathDistance = 0.0f;
    NavMeshAgent            agent; 


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
        int chunkSize = voxels.Length / 4;

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
    public int      GetNavMeshVerticesCount()   => navMeshVertices.Count;
    public void     SetVoxels(float [] voxels)  => this.voxels = voxels;
    public float    Mutation(int i)             => mutations[i];
    public float    GetLongestPathDistance()    => longestPathDistance;
    public void     AddVertex(Vector3 vertex)   => navMeshVertices.Add(vertex);

    public void     CalculateLongestDistance(NavMeshAgent agent)
    {
        this.agent = agent;

        for(int i = 0; i < navMeshVertices.Count - 1 ; ++i)
        {
            for(int j = i; j < navMeshVertices.Count; ++j)
            {
                NavMeshPath path = new NavMeshPath();

                agent.transform.position = navMeshVertices[i];                
                
                if(agent.CalculatePath(navMeshVertices[j], path))
                {
                    agent.SetPath(path);

                    float distance = 0.0f;
                    Vector3[] corners = agent.path.corners;

                    for (int k = 0; k < corners.Length -1; ++k)
                    {
                        distance += Mathf.Abs((corners[k] - corners[k + 1]).magnitude);
                    }

                    if (distance > longestPathDistance) longestPathDistance = distance;
                }
            }
        }
    }

    

        

   
    
}
