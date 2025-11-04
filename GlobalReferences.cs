using Unity.VisualScripting;
using UnityEngine;

[DefaultExecutionOrder(-1000)]
public class GlobalReferences : MonoBehaviour
{
    public static GlobalReferences Instance { get; set; }

    public GameObject bulletImpactEffectPrefab;

    public void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

}


