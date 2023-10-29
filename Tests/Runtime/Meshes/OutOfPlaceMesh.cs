using UnityEngine;

namespace TestHelper.Monkey.Meshes
{
    /// <summary>
    /// Create a mesh that not contains a Transform position of the GameObject that the component attached to.
    /// This mesh is able to hit raycast. Without no position annotations, monkey operators cannot operate the mesh
    /// because a point where the monkey operators operate on is not in the mesh
    /// </summary>
    [RequireComponent(typeof(MeshCollider))]
    [RequireComponent(typeof(MeshFilter))]
    [AddComponentMenu("")] // Hide from "Add Component" picker
    public class OutOfPlaceMesh : MonoBehaviour
    {
        private static readonly Vector3[] s_vertices =
        {
            new Vector3(0, 0.5f, 0), new Vector3(-0.5f, 1f, 0), new Vector3(0.5f, 1f, 0),
        };

        private static readonly int[] s_triangles = { 0, 1, 2 };

        private Mesh _mesh;
        private MeshFilter _meshFilter;
        private MeshCollider _meshCollider;

        private void Awake()
        {
            _meshFilter = gameObject.GetComponent<MeshFilter>();
            _meshFilter.sharedMesh = Mesh;
            _meshCollider = gameObject.GetComponent<MeshCollider>();
            _meshCollider.sharedMesh = Mesh;
        }

        private Mesh Mesh
        {
            get
            {
                if (_mesh != null)
                {
                    return _mesh;
                }

                var mesh = new Mesh();
                mesh.name = nameof(OutOfPlaceMesh);
                mesh.SetVertices(s_vertices);
                mesh.SetTriangles(s_triangles, 0);
                mesh.RecalculateBounds();
                mesh.RecalculateNormals();
                _mesh = mesh;

                return _mesh;
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            var tr = transform;
            Gizmos.DrawWireMesh(Mesh, tr.position, tr.rotation, tr.localScale);
        }
    }
}
