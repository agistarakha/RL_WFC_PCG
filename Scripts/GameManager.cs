using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;


public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    private void Awake()
    {
        // If there is an instance, and it's not me, delete myself.

        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }
    [SerializeField] private FirstAgent agent;
    [SerializeField] private GameObject levelContainer;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject groundPrefab;
    [SerializeField] private GameObject wallPrefab;
    [SerializeField] private GameObject boxPrefab;
    [SerializeField] private GameObject goalPrefab;
    [SerializeField] private GameObject winOverlay;
    [SerializeField] private GameObject playerObj;

    public bool isGameOver = false;
    private int boxCount;
    private int winCount = 0;


    // Start is called before the first frame update
    void Start()
    {
        boxCount = LevelManager.Instance.boxCount;
        ImplementLevel();
        
    }

    

    private void ImplementLevel()
    {
        string levelString = LevelManager.Instance.level;

        //Level conversition to acceptable level
        levelString = levelString.Replace("\n", "").Replace("\r", "");
        char[] levelChar = levelString.ToCharArray();
        for(int i=0; i < levelChar.Length; i++)
        {
            switch (levelChar[i])
            {
                case '#':
                    SpawnTile(wallPrefab, i);
                    break;
                case '.':
                    SpawnTile(groundPrefab, i);
                    SpawnTile(goalPrefab, i);
                    break;
                case '&':
                    playerObj.transform.localPosition = new Vector3(i % 10, -(i / 10) + 9, 0);
                    playerObj.SetActive(true);
                    SpawnTile(groundPrefab, i);
                    //SpawnTile(playerPrefab, i);
                    break;
                case 'B':
                    SpawnTile(groundPrefab, i);
                    SpawnTile(boxPrefab, i);
                    break;
                case ' ':
                    SpawnTile(groundPrefab, i);
                    break;

            }
        }
    }


    private void SpawnTile(GameObject targetTile, int index)
    {
        Vector3 pos = new Vector3(index % 10, -(index / 10) + 9, 0);
        GameObject obj = Instantiate(targetTile, levelContainer.transform);
        obj.transform.localPosition = pos;

    }

    // Update is called once per frame
    public void ReloadLevel()
    {
        SceneManager.LoadScene("Sokoban");
    }

    public void MainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void NewLevel()
    {
        
    }


    public void SetWinCount(int value)
    {
        winCount += value;
    }

    public void WinCheck()
    {
        if (winCount == boxCount)
        {
            Debug.Log("WIIIIIN");
            isGameOver = true;
            winOverlay.SetActive(true);
        }
    }

}
