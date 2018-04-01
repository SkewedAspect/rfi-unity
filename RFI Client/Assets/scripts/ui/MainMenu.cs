// ----------------------------------------------------------------------------------------------------------------------
// Main Menu controller
// ----------------------------------------------------------------------------------------------------------------------

using UnityEngine;
using UnityEngine.SceneManagement;

// ----------------------------------------------------------------------------------------------------------------------

public class MainMenu : MonoBehaviour
{
    public void StartSinglePlayer()
    {
        SceneManager.LoadScene("asteroidsScene", LoadSceneMode.Single);
    } // end StartSinglePlayer

    public void Exit()
    {
        Application.Quit();
    } // end Exit
} // end MainMenu

// ----------------------------------------------------------------------------------------------------------------------
