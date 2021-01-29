using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelPick : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }
    string[] scenePaths;
    private void Awake()
    {
        //    string lvlPath = Path.Combine(Application.dataPath, "Scenes", "Levels");
        var myLoadedAssetBundle = AssetBundle.LoadFromFile("Assets/AssetBundles/levels");
        scenePaths = myLoadedAssetBundle.GetAllScenePaths();
    //    var files = Directory.GetFiles(lvlPath);
        foreach(var p in scenePaths)
        {
#if UNITY_EDITOR
            if (p.EndsWith("meta"))
            {
                continue;
            }
#endif
            list.options.Add(new Dropdown.OptionData(p.Split(new string[] { "Levels" },System.StringSplitOptions.RemoveEmptyEntries)[1].Substring(1)));
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void StartHasBeenCalled()
    {
        var val = list.value;
        if (val != 0)
        {
            SceneManager.LoadSceneAsync(1);
            SceneManager.LoadSceneAsync(scenePaths[val-1], LoadSceneMode.Additive);
        }
    }
    public void Repicked()
    {
        Debug.Log(list.value);
    }
    public Dropdown list;
}
