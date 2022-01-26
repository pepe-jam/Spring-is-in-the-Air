using System.Collections;
using System.Collections.Generic;
using Helper;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject continueButton;
    private SceneLoader sceneLoader;
    public void Start()
    {
        if (!PlayerPrefs.HasKey("level"))
        {
            continueButton.SetActive(false);
        }

    }

    public void LoadSaveGame()
    {
        SceneManager.LoadScene(PlayerPrefs.GetString("level"));
    }

    public void NewGame()
    {
        PlayerPrefs.DeleteKey("level");
        PlayerPrefs.DeleteKey("position_x");
        PlayerPrefs.DeleteKey("position_y");
        SceneManager.LoadScene(SceneNames.SewerLevel.ToString());
    }
}
