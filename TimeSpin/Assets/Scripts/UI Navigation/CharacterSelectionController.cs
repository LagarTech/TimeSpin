using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectionController : MonoBehaviour
{

    public static CharacterSelectionController instance;

    public Image characterPreview;  // La imagen donde se mostrar� el personaje
    private int currentCharacterIndex = 0;  // �ndice del personaje actual
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

    // M�todo para avanzar al siguiente personaje
    public void NextCharacter()
    {
        currentCharacterIndex = (currentCharacterIndex + 1) % NUM_CHARACTERS;
        UpdateCharacterPreview();
    }

    // M�todo para retroceder al personaje anterior
    public void PreviousCharacter()
    {
        currentCharacterIndex--;
        if (currentCharacterIndex < 0)
        {
            currentCharacterIndex = NUM_CHARACTERS - 1;
        }
        UpdateCharacterPreview();
    }

    // M�todo que actualiza la imagen del personaje
    // Para ello, se necesita mover la posici�n de la c�mara
    public void UpdateCharacterPreview()
    {
        if (GameSceneManager.instance.gameStarted) return;
        // Se calcula la posici�n que tendr� la c�mara utilizando la f�rmula
        float cameraPosition = _startingCharacterPosition + currentCharacterIndex * CHARACTERS_DISTANCE + CAMERA_OFFSET;
        // Se actualiza la posici�n de la c�mara
        Camera.main.transform.position = new Vector3(cameraPosition, Camera.main.transform.position.y, Camera.main.transform.position.z);
        SelectionController.instance.ModifyCharacter(currentCharacterIndex);
    }

    // M�todo que coloca la c�mara en el museo despu�s de escoger personaje
    public void SetMuseumCamera()
    {
        // Se adaptan la posici�n y la rotaci�n
        Camera.main.transform.position = lobbyCameraPosition;
        Camera.main.transform.rotation = Quaternion.Euler(lobbyCameraRotation);
    }
}
