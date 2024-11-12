using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartingManager : MonoBehaviour
{
    [SerializeField] private List<Button> _gamesButtons;
    private void Start()
    {
        BlockPlayedGamesButtons();
    }

    public void StartGame(int gameId)
    {
        // En función del minijuego escogido se pasa a una escena u otra
        string nextScene = "";
        switch(gameId)
        {
            case 0: nextScene = "Prehistory"; break;
            case 1: nextScene = "Egypt"; break;
            case 2: nextScene = "Medieval"; break;
            case 3: nextScene = "Maya"; break;
            case 4: nextScene = "Future"; break;
        }
        // Se marca el minijuego como ya jugado en el gestor entre escenas
        GameSceneManager.instance.RegisterGameSelection(gameId);
        LoadingScreenManager.instance.SceneToGameTransition(nextScene);
    }

    private void BlockPlayedGamesButtons()
    {
        bool[] gamesPlayed = GameSceneManager.instance.GetPlayedGames();
        for (int i = 0; i < gamesPlayed.Length; i++)
        {
            if (gamesPlayed[i])
            {
                BlockButton(_gamesButtons[i]);
            }
        }
    }

    private void BlockButton(Button button)
    {
        button.interactable = false;
        // Cambia el color para indicar que el botón está bloqueado
        ColorBlock colores = button.colors;
        colores.normalColor = Color.gray; // Cambia a un color gris
        button.colors = colores;
    }

}
