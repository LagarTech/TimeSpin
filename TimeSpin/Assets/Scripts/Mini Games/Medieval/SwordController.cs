using UnityEngine;
using System.Collections;
using UnityEngine.Audio;

public class SwordController : MonoBehaviour
{
    private const float SWORD_HOLD_TIME = 5f;  // Tiempo que la espada se mantiene en la base
    private bool _isHeldAtBase = false; // Se indica si la espada está en la base
    [SerializeField] private int _swordPoints = 0; // Puntos que se otorgan al llevar la espada a la base
    private Rigidbody _rb;

    [SerializeField]
    private AudioSource _reproductor;
    [SerializeField]
    private AudioClip _clipAudio;
    [SerializeField]
    private AudioMixer mezclador;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        PlayerMovement player = collision.gameObject.GetComponent<PlayerMovement>();
        // Si el jugador entra en contacto con una espada, este la toma consigo
        if (player != null && player.carriedSword == null && !_isHeldAtBase)
        {
            player.SetCarriedSword(gameObject);
            _rb.useGravity = false; // Se desactiva la gravedad
            transform.SetParent(player.transform);
            transform.localPosition = new Vector3(0, 1, 0);
            MusicManager.PonerMusica(_clipAudio, _reproductor, false);
        }
    }

    public void DeliverSword()
    {
        if (_isHeldAtBase) return;  // No hacer nada si ya está siendo retenida

        _isHeldAtBase = true; // Se indica que la espada ha sido depositada en la base

        // Mueve la espada a la base correcta
        transform.position = MedievalGameManager.Instance.bases[MedievalGameManager.Instance.nextBaseIndex].position;
        transform.parent = MedievalGameManager.Instance.bases[MedievalGameManager.Instance.nextBaseIndex]; // Fija la espada en la base

        // Inicia la corutina para mantener la espada en la base
        StartCoroutine(HoldSwordAtBase());

    }
    private IEnumerator HoldSwordAtBase()
    {
        // Espera 5 segundos mientras la espada está en la base
        yield return new WaitForSeconds(SWORD_HOLD_TIME);

        // Si da tiempo a que un personaje la recoja, otorga puntos al jugador y elimina la espada
        if (_isHeldAtBase)
        {
            MedievalGameManager.Instance.AddScore(_swordPoints); // Agrega puntos al jugador
            Destroy(gameObject);  // Elimina la espada del juego
        }
    }

}





