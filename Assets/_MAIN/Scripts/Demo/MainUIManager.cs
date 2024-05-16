using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

namespace ReubenMiller.Fracture.Demo
{
    public class MainUIManager : MonoBehaviour
    {
        public static MainUIManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject); // Destroy this instance if another exists
            }
            else
            {
                Instance = this;
                DontDestroyOnLoad(gameObject); // Persist this object across scenes
            }
        }
        [Header("UI")]
        public GameObject mainUI;
        public TMP_Text titleText;

        [Header("Fragment Counts")]
        public TMP_Dropdown fragmentCountDropdown;
        public int[] fragmentCounts;
        public RayCastActivation rayCastActivation;

        [Header("Scenes")]
        public int currentScene = 0;
        public string[] scenes;

        public void Start()
        {
            titleText.text = scenes[currentScene];
            fragmentCountDropdown.onValueChanged.AddListener((val) => DropDownChanged(val));
        }

        public void DropDownChanged(int val)
        {
            rayCastActivation.fractureCount = fragmentCounts[val];
        }

        public void Update()
        {
            // toggle the MainUI
            if(Input.GetKeyDown(KeyCode.Q))
            {
                mainUI.SetActive(!mainUI.activeInHierarchy);
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                SceneManager.LoadScene(scenes[currentScene]);
            }

            // if the scene is to be changed then change the scene
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                currentScene = currentScene - 1 < 0 ? scenes.Length - 1 : currentScene - 1;
                SceneManager.LoadSceneAsync(scenes[currentScene]);
                titleText.text = scenes[currentScene];
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                currentScene = currentScene + 1 >= scenes.Length ? 0 : currentScene + 1;
                SceneManager.LoadSceneAsync(scenes[currentScene]);
                titleText.text = scenes[currentScene];
            }

        }
    }
}
