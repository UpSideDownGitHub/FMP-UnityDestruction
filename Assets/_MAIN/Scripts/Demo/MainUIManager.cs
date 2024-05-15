using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

namespace UnityFracture.Demo
{
    public class MainUIManager : MonoBehaviour
    {
        [Header("UI")]
        public GameObject mainUI;
        public TMP_Text titleText;
        public string[] titles;

        [Header("Scenes")]
        public int currentScene = 0;
        public string[] scenes;

        public void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        public void Start()
        {
            titleText.text = titles[currentScene];
        }

        public void Update()
        {
            // toggle the MainUI
            if(Input.GetKeyDown(KeyCode.Escape))
            {
                mainUI.SetActive(!mainUI.activeInHierarchy);
            }

            // if the scene is to be changed then change the scene
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                currentScene = currentScene - 1 < 0 ? scenes.Length - 1 : currentScene - 1;
                SceneManager.LoadSceneAsync(scenes[currentScene]);
                titleText.text = titles[currentScene];
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                currentScene = currentScene + 1 >= scenes.Length ? 0 : currentScene + 1;
                SceneManager.LoadSceneAsync(scenes[currentScene]);
                titleText.text = titles[currentScene];
            }

        }
    }
}
