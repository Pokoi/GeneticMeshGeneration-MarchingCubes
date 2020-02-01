using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine;

public class DrawNavMesh : MonoBehaviour
{
    public Color         navMeshColor       = new Color32(255, 0, 0, 150);

    NavMeshTriangulation triangulation;
    Material             mat;
    
    bool                 drawNavMesh = false;  
    
    public void ToggleNavMeshDrawing()          => drawNavMesh = !drawNavMesh;  
     
    
    private void Awake()
    {
        mat = new Material(Shader.Find("Hidden/Internal-Colored"));
    }

    private void Start()
    {        
        mat.hideFlags = HideFlags.HideAndDontSave;       
    }

    private void OnPostRender()
    {
        triangulation = NavMesh.CalculateTriangulation();
        if(drawNavMesh)
        {

            if(triangulation.indices.Length != 0)
            { 
                GL.PushMatrix();
                mat.SetPass(0);

                GL.Begin(GL.TRIANGLES);

                for (int i = 0; i < triangulation.indices.Length; i += 3)
                {
                    var triangleIndex = i / 3;
                    var i1 = triangulation.indices[i];
                    var i2 = triangulation.indices[i + 1];
                    var i3 = triangulation.indices[i + 2];
                    var p1 = triangulation.vertices[i1];
                    var p2 = triangulation.vertices[i2];
                    var p3 = triangulation.vertices[i3];
                    var areaIndex = triangulation.areas[triangleIndex];

                    GL.Color(navMeshColor);
                    GL.Vertex(p1);
                    GL.Vertex(p2);
                    GL.Vertex(p3);
                }

                GL.End();

                GL.PopMatrix();
            }        
        }
    }
    
}
