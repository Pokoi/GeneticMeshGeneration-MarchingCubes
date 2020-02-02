/*
 * File: DrawNavMesh.cs
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
