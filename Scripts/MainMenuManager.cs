using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private Button startBtn;
    [SerializeField] private TMP_Dropdown boxCountDropDown;
    [SerializeField] private TMP_Dropdown levelSizeDropDown;
    [SerializeField] private GameObject[] trainingLabs;
    [SerializeField] private GameObject loadingOverlay;
    private int boxCount;
    // Start is called before the first frame update
    void Start()
    {
        
        startBtn.onClick.AddListener(LoadGame);
        
    }

    private void LoadGame()
    {
        loadingOverlay.SetActive(true);
        boxCount = System.Int32.Parse(boxCountDropDown.captionText.text);
        int levelSizeIndex = levelSizeDropDown.value;
        
        LevelManager.Instance.boxCount = boxCount;
        
        FirstAgent agent = trainingLabs[levelSizeIndex].GetComponentInChildren<FirstAgent>();
        agent.GenerateLevel(boxCount);
    }

    
}
