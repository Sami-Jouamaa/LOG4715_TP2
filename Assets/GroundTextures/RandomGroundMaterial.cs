using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class RandomGroundMaterial : MonoBehaviour
{
    [SerializeField] Material[] variations;

    void Start()
    {
        if (variations.Length == 0)
            return;

        GetComponent<MeshRenderer>().sharedMaterial =
            variations[Random.Range(0, variations.Length)];
    }
}