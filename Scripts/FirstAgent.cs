using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;



[System.Serializable]
public class MyData
{
    public string layout;
    public string method;
}

public class FirstAgent : Agent
{
    private Transform levelContainer;//Object that contains all tiles for blank level
    [SerializeField] private OverlapWFC overlapWFC;//Reference to wfc algo
    [SerializeField] private Text levelText;//Text UI for visualization
    [SerializeField] private string PORT = "5000";
    [SerializeField] private string nameSuffix = "";// Used for dynamic filename
    [SerializeField] private bool isImitationLearning = false;
    [SerializeField] private int demoFileIndex = 0;
    [SerializeField] private int maxDemoFileIndex = 10;
    [SerializeField] private int levelSize = 9;
    [SerializeField] private int maxBoxCount = 4;
    [SerializeField] private bool isEval = false;
    private int evalFileIndex = 0;
    private float[] array;//Level that has been converted to float array
    private string apiUrl;
    private List<float> availableTiles;
    private int boxCount;
    private float[] componentsCount;

    private string arrayLevels; //Converted level from array to string
    private string solverResults;
    private Dictionary<string, float> tilesValue;
    private int playerIndex;
    private List<int> boxesIndex;
    private List<int> goalsIndex;
    private List<int> allComponentsIndex; // Contains all index of box, player, and goal from demo level
    private bool isInference;
    public override void OnEpisodeBegin()
    {
        isInference = GetComponent<BehaviorParameters>().BehaviorType == BehaviorType.InferenceOnly;

        // Reset All process and Reinitialize variable
        StopAllCoroutines();
        apiUrl = "http://127.0.0.1:" + PORT + "/post";
        array = new float[100];  
        availableTiles = new List<float>();
        tilesValue = new Dictionary<string, float>()
        {
            {"ground(Clone)",0f},
            {"block(Clone)",1f }
        };
        if (isEval)
        {
            //Non Imitation Learning run
            boxCount = Random.Range(1, maxBoxCount + 1);
            AvailableTilesInit();
            overlapWFC.Generate();
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = 1f;
            }
            ////FillBoundary();
            evalFileIndex += 1;
            if (evalFileIndex > 120)
            {
                ////UnityEditor.EditorApplication.isPlaying = false;
            }
            
            StartCoroutine(GenerateBlankLayout());
        }
        if (!isInference)
        {



            if (isImitationLearning)
            {
                //Init variable for IL
                boxesIndex = new List<int>();
                goalsIndex = new List<int>();
                allComponentsIndex = new List<int>();
                Debug.Log(demoFileIndex);
                if (demoFileIndex > maxDemoFileIndex)
                {
                    //UnityEditor.EditorApplication.isPlaying = false;
                }
                string demoLevelPath = "D:\\Documents\\Python\\SokobanGamePreProcess\\dataset\\" + demoFileIndex.ToString() + ".txt";
                demoFileIndex += 1;
                ImitationLearningInit(demoLevelPath);
            }
            else
            {
                //Non Imitation Learning run
                boxCount = Random.Range(1, maxBoxCount+1);
                AvailableTilesInit();
                overlapWFC.Generate();
                for (int i = 0; i < array.Length; i++)
                {
                    array[i] = 1f;
                }
                ////FillBoundary();
                StartCoroutine(GenerateBlankLayout());
            }

        }
    }


    
    public void GenerateLevel(int boxCount)
    {
        
        isInference = GetComponent<BehaviorParameters>().BehaviorType == BehaviorType.InferenceOnly;

        // Reset All process and Reinitialize variable
        StopAllCoroutines();
        apiUrl = "http://127.0.0.1:" + PORT + "/post";
        array = new float[100];
        availableTiles = new List<float>();
        tilesValue = new Dictionary<string, float>()
        {
            {"ground(Clone)",0f},
            {"block(Clone)",1f }
        };

        this.boxCount = boxCount;
        AvailableTilesInit();
        overlapWFC.Generate();
        for (int i = 0; i < array.Length; i++)
        {
            array[i] = 1f;
        }
        //FillBoundary();
        StartCoroutine(GenerateBlankLayout());
        
    }

    private void ImitationLearningInit(string filePath)
    {
        //Read file containing sokoban level
        StreamReader reader = new StreamReader(filePath);
        string levelString = reader.ReadToEnd();
        reader.Close();

        //Level conversition to acceptable level
        levelString = levelString.Replace("\n", "").Replace("\r","");
        
        boxCount = levelString.Count(c => c == 'B');
        ////Debug.Log(boxCount);
        AvailableTilesInit();
        levelString = levelString.Replace(" ", "0").Replace("#", "1").Replace("&", "2").Replace("B", "3").Replace(".", "4");
        char[] levelChar = levelString.ToCharArray();

        
        
        
        int i = 0;
        foreach(char c in levelChar)
        {

            //Store player, box and goal index
            if (c == '3')
            {
                boxesIndex.Add(i);
                array[i] = 0f;

            }
            else if(c == '4')
            {
                goalsIndex.Add(i);
                array[i] = 0f;
            }
            else if(c == '2')
            {
                playerIndex = i;
                array[i] = 0f;
            }
            else
            {
                //Store level to float array level
                array[i] = c - '0';
            }

           
            i += 1;
        }

        //Store all index into one variable
        allComponentsIndex.Add(playerIndex);
        allComponentsIndex.AddRange(boxesIndex);
        allComponentsIndex.AddRange(goalsIndex);
        StartCoroutine(RequestDecisions());

    }


    //Generate level layout that only contains wall and ground tile
    private IEnumerator GenerateBlankLayout()
    {

        
         yield return new WaitForSeconds(0.5f);
        ////yield return new WaitForSeconds(1f);
        levelContainer = overlapWFC.transform.GetChild(0).GetChild(0);
        GetLevelLayout();
        StartCoroutine(RequestDecisions());
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        
        sensor.AddObservation(array);//Observe entire level layout
        sensor.AddObservation(boxCount);//Observe current requested box and goal count

    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        int selectedIndex = actions.DiscreteActions[0];
        if(selectedIndex < 0)
        {
            return;
        }
        float selectedTile = availableTiles[0];
        availableTiles.RemoveAt(0);
        float oldTile = array[selectedIndex];

        //Place tile to level
        array[selectedIndex] = selectedTile;
        if (oldTile != 0f && !isInference)
        {
            //If agent attempt to place tile beside empty ground
            SetReward(-1f);
            Debug.Log("Reward: -1");
            EndEpisode();

            
        }
        else if(availableTiles.Count() == 0 && GetCumulativeReward() == 0)
        {
            //If agent succesfully place all tile with correct amount of tiles
            SetReward(+0.5f);
            
            if (isImitationLearning)
            {
                AddReward(+0.5f);
                EndEpisode();
            }
            else
            {
                
                UpdateLevelUI();
            }
            
            ////EndEpisode();
        }    


    }


    //Give an advice to help agent make decision
    private int HeuristicRequestDecision()
    {
        if(allComponentsIndex.Count() == 0)
        {
            return -1;
        }
        int value = allComponentsIndex[0];
        allComponentsIndex.RemoveAt(0);
        return value;
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<int> discreteActions = actionsOut.DiscreteActions;   
        discreteActions[0] = HeuristicRequestDecision();
    }

    //Initialize tile that should be placed based on box and goal count
    private void AvailableTilesInit()
    {

        availableTiles.Add(2f);
        float[] boxAndGoalFloat = new float[] {3f,4f};
        foreach (float value in boxAndGoalFloat)
        {
            for (int i = 0; i < boxCount; i++)
            {
                availableTiles.Add(value);
                
            }
        }
    }

    //Update level UI and file
    private void UpdateLevelUI()
    {
        arrayLevels = "";
        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                arrayLevels += "" + array[i * 10 + j].ToString() + " ";
            }

            arrayLevels += "\n";
        }

        arrayLevels = arrayLevels.Replace(" ", "").Replace("0", " ").Replace("1", "#").Replace("2", "&").Replace("3", "B").Replace("4", ".").Replace("5", "X");
        ////levelText.text = arrayLevels;
        
        if (!isInference)
        {
            WriteToFile(arrayLevels, "Assets/Levels/Output" + nameSuffix + ".txt");
            StartCoroutine(PlayabilityCheck());
        }
        else
        {
            if (isEval)
            {
                WriteToFile(arrayLevels, "Assets/Eval/" + evalFileIndex.ToString() + ".txt");
                Debug.Log("Level Created");
                EndEpisode();
            }
            else
            {
                LevelManager.Instance.level = arrayLevels;
                SceneManager.LoadScene("Sokoban");
            }
            
        }
        
    }


    //Request multiple decision basen on box and goals count
    private IEnumerator RequestDecisions()
    {
        for(int i=0; i<boxCount*2+1; i++)
        {
            
            RequestDecision();
            yield return new WaitForSeconds(0.5f);
            ////yield return new WaitForSeconds(1f);
            
        }

    }

    //Store blank level as an array
    private void GetLevelLayout()
    {
        Transform[] tilesTransform = levelContainer.GetComponentsInChildren<Transform>();
        
        for (int i = 0; i < levelContainer.childCount; i++)
        {
            Transform tile = levelContainer.GetChild(i);
            float x = tile.localPosition.x + 1;
            float y = 7 - tile.localPosition.y + 1;
            int index = Mathf.FloorToInt(x) + (Mathf.FloorToInt(y) * 10);
            

            if (x < 9 && y < 9)
            {
                array[index] = tilesValue[tile.gameObject.name];
            }
        }
    }

    //Create level boundary using wall tile
    private void FillBoundary()
    {
        int boundarySize = 1; // Increase the boundary size

        for (int y = 0; y < 10; y++)
        {
            for (int x = 0; x < 10; x++)
            {
                if (x < boundarySize || x >= 10 - boundarySize || y < boundarySize || y >= 10 - boundarySize)
                {
                    int flattenedIndex = y * 10 + x;
                    array[flattenedIndex] = 1f;
                }
            }
        }
    }




    //Request to a server that will check the playability of current generated level
    IEnumerator PlayabilityCheck()
    {
        // Create an instance of your data class and populate it
        MyData postData = new MyData
        {
            layout = "Output"+nameSuffix+".txt",
            method = "astar"
        };

        // Convert the data to JSON format
        string requestBody = JsonUtility.ToJson(postData);
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(requestBody);

        using (UnityWebRequest webRequest = UnityWebRequest.Post(apiUrl, ""))
        {
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");

            // Send the request asynchronously
            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error: " + webRequest.error);
                EndEpisode();
                yield break; // Exit the coroutine if there's an error
            }

            // Request successful, process the response
            string response = webRequest.downloadHandler.text;
            

            if (response.Contains("true"))
            {
                //If level is playable
                Debug.Log("+0.5");
                WriteToFile(arrayLevels, "Assets/Levels/playable"+nameSuffix+".txt");
                AddReward(+0.5f);
            }
            else
            {
                //If level is not playable
                if (response.Contains("step"))
                {
                    //If maximum step is reached
                    AddReward(-0.5f);
                }
                else
                {
                    //If level is definitely not playable
                    Debug.Log("-1");
                    AddReward(-1);
                }
                
            }

            EndEpisode();
        }
    }






    private void WriteToFile(string text, string filePath)
    {
        // Open a file stream with write access
        using (StreamWriter writer = new StreamWriter(filePath, false))
        {
            writer.Write(text);
        }

        //Debug.Log("Text written to file: " + filePath);
    }

}
