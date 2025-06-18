using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class PartialHollowCylinderMesh : MonoBehaviour
{
    [Range(3, 100)] public int segments = 20;
    [Range(0f, 360f)] public float OpeningAngle = 90f;
    public float innerRadius = 0.5f;
    public float outerRadius = 1f;
    public float height = 1f;

    void OnValidate() => GenerateMesh();

    public void GenerateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.name = "PartialHollowCylinder";

        int arcPoints = segments + 1;
        float radAngle = Mathf.Deg2Rad * OpeningAngle;

        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<int> triangles = new List<int>();

        for (int i = 0; i < arcPoints; i++)
        {
            float t = (float)i / segments;
            float theta = t * radAngle;
            float cos = Mathf.Cos(theta);
            float sin = Mathf.Sin(theta);

            // ✅ PAS DE décalage : les points sont à leur vraie position dans le cercle
            Vector3 innerBottom = new Vector3(cos * innerRadius, 0, sin * innerRadius);
            Vector3 innerTop = new Vector3(cos * innerRadius, height, sin * innerRadius);
            Vector3 outerBottom = new Vector3(cos * outerRadius, 0, sin * outerRadius);
            Vector3 outerTop = new Vector3(cos * outerRadius, height, sin * outerRadius);

            vertices.Add(innerBottom); // 0
            vertices.Add(innerTop);    // 1
            vertices.Add(outerBottom); // 2
            vertices.Add(outerTop);    // 3

            normals.Add(Vector3.down);  // juste pour base, on recalculera après
            normals.Add(Vector3.up);
            normals.Add(Vector3.down);
            normals.Add(Vector3.up);
        }

        // Index helpers
        int iBottomIn(int i) => i * 4;
        int iTopIn(int i) => i * 4 + 1;
        int iBottomOut(int i) => i * 4 + 2;
        int iTopOut(int i) => i * 4 + 3;

        for (int i = 0; i < segments; i++)
        {
            // Outer wall
            triangles.Add(iBottomOut(i));
            triangles.Add(iTopOut(i));
            triangles.Add(iBottomOut(i + 1));
            triangles.Add(iBottomOut(i + 1));
            triangles.Add(iTopOut(i));
            triangles.Add(iTopOut(i + 1));

            // Inner wall
            triangles.Add(iBottomIn(i + 1));
            triangles.Add(iTopIn(i));
            triangles.Add(iBottomIn(i));
            triangles.Add(iTopIn(i + 1));
            triangles.Add(iTopIn(i));
            triangles.Add(iBottomIn(i + 1));

            // Top face (ring)
            triangles.Add(iTopIn(i));
            triangles.Add(iTopIn(i + 1));
            triangles.Add(iTopOut(i));
            triangles.Add(iTopOut(i));
            triangles.Add(iTopIn(i + 1));
            triangles.Add(iTopOut(i + 1));

            // Bottom face (ring)
            triangles.Add(iBottomOut(i));
            triangles.Add(iBottomIn(i + 1));
            triangles.Add(iBottomIn(i));
            triangles.Add(iBottomOut(i + 1));
            triangles.Add(iBottomIn(i + 1));
            triangles.Add(iBottomOut(i));
        }

        // Side start face
        triangles.Add(iBottomIn(0));
        triangles.Add(iTopIn(0));
        triangles.Add(iBottomOut(0));
        triangles.Add(iBottomOut(0));
        triangles.Add(iTopIn(0));
        triangles.Add(iTopOut(0));

        // Side end face
        triangles.Add(iBottomOut(segments));
        triangles.Add(iTopIn(segments));
        triangles.Add(iBottomIn(segments));
        triangles.Add(iTopOut(segments));
        triangles.Add(iTopIn(segments));
        triangles.Add(iBottomOut(segments));

        mesh.SetVertices(vertices);
        mesh.SetNormals(normals);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals();

        MeshFilter mf = GetComponent<MeshFilter>();
        MeshCollider mc = GetComponent<MeshCollider>();

        mf.mesh = mesh;
        mc.sharedMesh = mesh; // ✅ Le collider suit ton mesh

    }
}