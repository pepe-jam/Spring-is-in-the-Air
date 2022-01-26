using System;
using System.Collections;
using System.Collections.Generic;
using Helper;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(SceneLoader))]
public class PauseMenu : MonoBehaviour
{
    public static PauseMenu Instance;
    public bool Open { get; private set; } = false;

    [SerializeField] private GameObject canvas;

    private SceneLoader sceneLoader;
    // Start is called before the first frame update
    void Start()
    {
        PauseMenu.Instance = this;  // Singleton Pattern, always overwrite references to PauseMenus from previous scenes
        sceneLoader = GetComponent<SceneLoader>();
        canvas.SetActive(Open);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleMenu();
        }
    }

    public void ToggleMenu()
    {
        Open = !Open;
        canvas.SetActive(Open);
    }

    public void LoadMainMenu()
    {
        sceneLoader.LoadScene(SceneNames.MainMenu.ToString());
    }

    /**
     * Reloads the current level and resets the player's position to the start of the level
     */
    public void ReloadLevel()
    {
        PlayerPrefs.DeleteKey("position_x");
        PlayerPrefs.DeleteKey("position_y");
        Respawn();
    }

    /**
     * Reloads the current level while keeping the player's last saved position in mind
     */
    public void Respawn()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
