using UnityEngine;

namespace Meshes
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
    public class Triangle : MonoBehaviour
    {
        
        private void OnEnable()
        {
            var mesh = new Mesh
            {
                name = "Triangle",
                vertices = new[]
                {
                    Vector3.zero, // 0
                    Quaternion.Euler(-45, 0, 0) * Vector3.up, // 1
                    Quaternion.Euler(45, 0, 0) * Vector3.up, // 2
                    Vector3.right, // 3
                    Quaternion.Euler(-45, 0, 0) * Vector3.up + Vector3.right, // 4
                    Quaternion.Euler(45, 0, 0) * Vector3.up + Vector3.right // 5
                },
                triangles = new[] { 0, 2, 1, 3, 4, 5, 0, 3, 2, 2, 3, 5, 0, 1, 3, 1, 4, 3, 1, 2, 4, 2, 5, 4 }
            };

            GetComponent<MeshFilter>().sharedMesh = mesh;
            GetComponent<MeshCollider>().sharedMesh = mesh;
        }
    }
}
