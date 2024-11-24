using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectionController : MonoBehaviour
{

    public static CharacterSelectionController instance;

    public Image characterPreview;  // La imagen donde se mostrará el personaje
    private int currentCharacterIndex = 0;  // Índice del personaje actual
    private const int NUM_CHARACTERS = 12;

    public Vector3 lobbyCameraPosition;
    public Vector3 lobbyCameraRotation;

    private const int _startingCharacterPosition = 40;
    private const int CHARACTERS_DISTANCE = 5;
    private const float CAMERA_OFFSET = 2.75F;


    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    void Start()
    {
        // Mostrar el primer personaje al inicio
        UpdateCharacterPreview();
    }

    // Método para avanzar al siguiente personaje
    public void NextCharacter()
    {
        currentCharacterIndex = (currentCharacterIndex + 1) % NUM_CHARACTERS;
        UpdateCharacterPreview();
    }

    // Método para retroceder al personaje anterior
    public void PreviousCharacter()
    {
        currentCharacterIndex--;
        if (currentCharacterIndex < 0)
        {
            currentCharacterIndex = NUM_CHARACTERS - 1;
        }
        UpdateCharacterPreview();
    }

    // Método que actualiza la imagen del personaje
    // Para ello, se necesita mover la posición de la cámara
    public void UpdateCharacterPreview()
    {
        if (GameSceneManager.instance.gameStarted) return;
        // Se calcula la posición que tendrá la cámara utilizando la fórmula
        float cameraPosition = _startingCharacterPosition + currentCharacterIndex * CHARACTERS_DISTANCE + CAMERA_OFFSET;
        // Se actualiza la posición de la cámara
        Camera.main.transform.position = new Vector3(cameraPosition, Camera.main.transform.position.y, Camera.main.transform.position.z);
        SelectionController.instance.ModifyCharacter(currentCharacterIndex);
    }

    // Método que coloca la cámara en el museo después de escoger personaje
    public void SetMuseumCamera()
    {
        // Se adaptan la posición y la rotación
        Camera.main.transform.position = lobbyCameraPosition;
        Camera.main.transform.rotation = Quaternion.Euler(lobbyCameraRotation);
    }
}
