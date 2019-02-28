
//    MIT License
//    
//    Copyright (c) 2017 Dustin Whirle
//    
//    My Youtube stuff: https://www.youtube.com/playlist?list=PL-sp8pM7xzbVls1NovXqwgfBQiwhTA_Ya
//    
//    Permission is hereby granted, free of charge, to any person obtaining a copy
//    of this software and associated documentation files (the "Software"), to deal
//    in the Software without restriction, including without limitation the rights
//    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//    copies of the Software, and to permit persons to whom the Software is
//    furnished to do so, subject to the following conditions:
//    
//    The above copyright notice and this permission notice shall be included in all
//    copies or substantial portions of the Software.
//    
//    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//    SOFTWARE.

using UnityEngine;
using System.Collections.Generic;

public class MeshMaker
{
    // Mesh Values
    private List<Vector3> _vertices = new List<Vector3>();
    private List<Vector3> _normals = new List<Vector3>();
    private List<Vector2> _uvs = new List<Vector2>();
    private List<Vector4> _tangents = new List<Vector4>();
    private List<List<int>> _subIndices = new List<List<int>>();

    public void AddTriangle(
        Vector3[] vertices,
        Vector3[] normals,
        Vector2[] uvs,
        Vector4[] tangents,
        int submesh)
    {
        int vertCount = _vertices.Count;

        _vertices.Add(vertices[0]);
        _vertices.Add(vertices[1]);
        _vertices.Add(vertices[2]);

        _normals.Add(normals[0]);
        _normals.Add(normals[1]);
        _normals.Add(normals[2]);

        _uvs.Add(uvs[0]);
        _uvs.Add(uvs[1]);
        _uvs.Add(uvs[2]);

        _tangents.Add(tangents[0]);
        _tangents.Add(tangents[1]);
        _tangents.Add(tangents[2]);

        if (_subIndices.Count < submesh + 1)
        {
            for (int i = _subIndices.Count; i < submesh + 1; i++)
            {
                _subIndices.Add(new List<int>());
            }
        }

        _subIndices[submesh].Add(vertCount);
        _subIndices[submesh].Add(vertCount + 1);
        _subIndices[submesh].Add(vertCount + 2);
    }

    /// <summary>
    /// Creates and returns a new mesh.
    /// </summary>
    public Mesh GetMesh()
    {
        Mesh mesh = new Mesh
        {
            vertices = _vertices.ToArray(),
            normals = _normals.ToArray(),
            uv = _uvs.ToArray(),
            uv2 = _uvs.ToArray(),
        };

        if (_tangents.Count > 1)
        {
            mesh.SetTangents(_tangents);
        }
            
        mesh.subMeshCount = _subIndices.Count;

        for (int i = 0; i < _subIndices.Count; i++)
        {
            mesh.SetTriangles(_subIndices[i], i);
        }
           
        return mesh;
    }
}

