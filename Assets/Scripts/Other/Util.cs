
using UnityEngine;


static class Util
{
    static public float Map(float v, float a0, float a1, float b0, float b1) => b0 + (b1 - b0) * (v - a0) / (a1 - a0);

    static public float ConstrainMap(float v, float a0, float a1, float b0, float b1) => Mathf.Clamp(b0 + (b1 - b0) * (v - a0) / (a1 - a0), b0, b1);


    public static void SetLayer(Transform t, int layer)
    {
        // Recursive transform layer update
        t.gameObject.layer = layer;
        foreach (Transform c in t) SetLayer(c, layer);
    }
    
    public static float SignedVolumeOfTriangle(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        // Get volume of a triangle
        float v321 = p3.x * p2.y * p1.z;
        float v231 = p2.x * p3.y * p1.z;
        float v312 = p3.x * p1.y * p2.z;
        float v132 = p1.x * p3.y * p2.z;
        float v213 = p2.x * p1.y * p3.z;
        float v123 = p1.x * p2.y * p3.z;
        return (1.0f / 6.0f) * (-v321 + v231 + v312 - v132 - v213 + v123);
    }

    public static float VolumeOfMesh(Mesh mesh)
    {
        // Return volume of a mesh
        float volume = 0;
        for (int i = 0; i < mesh.triangles.Length; i += 3)
        {
            Vector3 p1 = mesh.vertices[mesh.triangles[i + 0]];
            Vector3 p2 = mesh.vertices[mesh.triangles[i + 1]];
            Vector3 p3 = mesh.vertices[mesh.triangles[i + 2]];
            volume += SignedVolumeOfTriangle(p1, p2, p3);
        }
        return Mathf.Abs(volume);
    }

    
    public static class Easing
    {
        public static float EaseOutSine(float x) => Mathf.Sin((x * Mathf.PI) / 2);
        public static float EaseInSine(float x) => 1 - Mathf.Cos((x * Mathf.PI) / 2);
    }
}
