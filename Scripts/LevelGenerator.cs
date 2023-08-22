using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    [SerializeField] private FirstAgent agent;
    [SerializeField] private int boxCount;
    // Start is called before the first frame update
    void Start()
    {
        Invoke("GenerateLevel", 2f);

    }

    private void GenerateLevel()
    {
        agent.GenerateLevel(boxCount);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
