using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectionController : MonoBehaviour
{
    public Image characterPreview;  // La imagen donde se mostrará el personaje
    public Sprite[] characterSprites;  // Array de sprites de los personajes
    private int currentCharacterIndex = 0;  // Índice del personaje actual

    void Start()
    {
        // Mostrar el primer personaje al inicio
        UpdateCharacterPreview();
    }

    // Método para avanzar al siguiente personaje
    public void NextCharacter()
    {
        currentCharacterIndex = (currentCharacterIndex + 1) % characterSprites.Length;
        UpdateCharacterPreview();
    }

    // Método para retroceder al personaje anterior
    public void PreviousCharacter()
    {
        currentCharacterIndex--;
        if (currentCharacterIndex < 0)
        {
            currentCharacterIndex = characterSprites.Length - 1;
        }
        UpdateCharacterPreview();
    }

    // Método que actualiza la imagen del personaje
    void UpdateCharacterPreview()
    {
        SelectionController.instance.ModifyCharacter(currentCharacterIndex);
        characterPreview.sprite = characterSprites[currentCharacterIndex];
    }
}
