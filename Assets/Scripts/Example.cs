using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;
using TMPro;
using System.Linq;
using NavMeshBuilder = UnityEngine.AI.NavMeshBuilder;
using ProceduralNoiseProject;

    //[ExecuteInEditMode]
    public class Example : MonoBehaviour
    {
        public enum EvaluationTypes { pathDistance, navMeshVertices };
        public EvaluationTypes evaluation;

        public Material m_material;
        const int    numberOfMeshes      = 10;
        float        visualizationTime   = 0.5f;
        public int          survivors           = 2;
        public int          [] seeds            = new int       [numberOfMeshes];
        Cromosome           [] cromosomes       = new Cromosome [numberOfMeshes];
        public Transform    [] pivots           = new Transform [numberOfMeshes];
        List<GameObject>    meshes              = new List<GameObject>();
        MarchingCubes       marching            = new MarchingCubes();
        public NavMeshAgent agent;
    
        public TMP_Dropdown    timesDropwdown;
        public TextMeshProUGUI GenerationText;
        int                    generationIndex        = 0;        
        Cromosome           [] bestCromosomesInOrder  = new Cromosome [numberOfMeshes];
        bool                   running;
        
        void Start()
        {

            //Surface is the value that represents the surface of mesh
            //For example the perlin noise has a range of -1 to 1 so the mid point is where we want the surface to cut through.
            //The target value does not have to be the mid point it can be any value with in the range.
            marching.Surface = 0.0f;
            GenerationText.text = "Generation: " + generationIndex;
            
            FirstGeneration();
        }

        public void FirstGeneration()
        {
            for(int i = 0; i < numberOfMeshes; ++i)
            {
                INoise perlin        = new PerlinNoise(seeds[i], 1.0f);
                FractalNoise fractal = new FractalNoise(perlin, 2, 2.0f);

                Cromosome cromosome = new Cromosome();

                int      width       = cromosome.GetWidth();
                int      height      = cromosome.GetHeight();
                int      length      = cromosome.GetLength();
                float [] voxels      = new float [width * height * length];

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

                            voxels[idx] = fractal.Sample3D(fx, fy, fz);
                        }
                    }
                }

                cromosome.SetVoxels(voxels);
                pivots[i].name = "--";
                cromosomes[i] = cromosome;
                CreateMesh(cromosome, i);
            }

            Invoke("Evaluate", visualizationTime);
        }

        public void Evaluate()
        {
            var navMesh = NavMesh.CalculateTriangulation();
            Vector3[] vertices = navMesh.vertices;
            
            // Check what navmesh belongs to each cromosome
            for(int i = 0; i < vertices.Length; ++i)
            {
                float lowerDistance = float.MaxValue;
                int owner           = 0;
                
                for(int j = 0; j < pivots.Length; ++j)
                {
                    float distance = Vector3.Distance(vertices[i], pivots[j].position);
                    if(distance < lowerDistance)
                    {
                        lowerDistance = distance;
                        owner = j;
                    }
                }

                cromosomes [owner].AddVertex(vertices[i]);
            } 

            List<Cromosome> OrderedCromosomes = new List<Cromosome>();

            for(int i = 0; i < cromosomes.Length; ++i)
            {
                cromosomes[i].CalculateLongestDistance(agent);
                OrderedCromosomes.Add(cromosomes[i]);

                switch (evaluation)
                {
                    case EvaluationTypes.pathDistance:
                        
                        pivots[i].name = cromosomes[i].GetLongestPathDistance().ToString();
                        break;
                    case EvaluationTypes.navMeshVertices:
                        pivots[i].name = cromosomes[i].GetNavMeshVerticesCount().ToString();
                        break;
                }
            }

            switch (evaluation)
            {
                case EvaluationTypes.pathDistance:
                    OrderedCromosomes = OrderedCromosomes.OrderByDescending(go => go.GetLongestPathDistance()).ToList<Cromosome>();
                    break;
                case EvaluationTypes.navMeshVertices:
                    OrderedCromosomes = OrderedCromosomes.OrderByDescending(go => go.GetNavMeshVerticesCount()).ToList<Cromosome>();
                    break;
            }
        
            bestCromosomesInOrder = OrderedCromosomes.ToArray();
            
            NextGeneration();
        }
        
        public void NextGeneration()
        {
            if (running)
            { 
                ++generationIndex;
                GenerationText.text = "Generation: " + generationIndex;

                // Clear the meshes
                foreach(GameObject go in meshes)
                {
                    Destroy(go);
                }

                meshes.Clear();

                // Create the childs cromosomes
                for(int i = 0; i < cromosomes.Length; ++i)
                {
                    pivots[i].name = "--";
                
                    // Choose the parents randomnly between the survivors
                    int index1;
                    int index2;
                
                    index1 = UnityEngine.Random.Range(0, survivors);
                
                    do
                    {
                        index2 = UnityEngine.Random.Range(0, survivors);
                    } while (index2 == index1);

                    cromosomes[i] = bestCromosomesInOrder[index1].Recombine(bestCromosomesInOrder[index2]);
                
                    CreateMesh(cromosomes[i], i);
                }
            
                if (generationIndex == 50 || generationIndex == 100 || generationIndex == 200) Pause();

                Invoke("Evaluate", visualizationTime);
            }            
        }

        public void CreateMesh(Cromosome cromosome, int index)
        {
            List<Vector3> verts = new List<Vector3>();
            List<int> indices   = new List<int>();

            float [] voxels      = cromosome.GetVoxels();
            int      width       = cromosome.GetWidth();
            int      height      = cromosome.GetHeight();
            int      length      = cromosome.GetLength();

            //The mesh produced is not optimal. There is one vert for each index.
            //Would need to weld vertices for better quality mesh.
            marching.Generate(voxels, width, height, length, verts, indices);
            
            Mesh mesh = new Mesh();
            mesh.SetVertices(verts);
            mesh.SetTriangles(indices, 0);
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();

            GameObject go = new GameObject("Mesh");
            go.AddComponent<MeshFilter>();
            go.AddComponent<MeshRenderer>();
            go.GetComponent<Renderer>().material = m_material;
            go.GetComponent<MeshFilter>().mesh = mesh;
            go.transform.SetParent(pivots[index]);
            go.isStatic = true;
            go.transform.localPosition = new Vector3(-width * 0.5f, 0, -length * 0.5f );
            go.AddComponent<NavMeshSourceTag>();

            meshes.Add(go);
        }


        public void ChangeVisualizationTime() => visualizationTime = 0.5f + (0.5f * timesDropwdown.value);        
        public void Pause() => running = false;
        public void Continue()
        {
            running = true;
            NextGeneration();
        }
    }
