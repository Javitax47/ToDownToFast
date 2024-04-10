using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ControladorJugador : MonoBehaviour
{
    public Vector3 gravity;

    public float rotationSpeed = 100.0f;
    public float maxForwardSpeed = 10.0f;
    public float acceleration = 5.0f;
    public float rotationReductionFactor = 1;
    public float rotationThreshold = 45.0f;
    public float speedThreshold = 8.0f;

    public ParticleSystem sistemaParticulas;

    private Rigidbody rb;
    private bool isGrounded = false;
    private float forwardSpeed = 10.0f;
    private float timeInAir = 0.0f;
    private bool hasBeenInAir = false;
    private Vector3 lastPosition;


    private Quaternion groundRotation;

    void Start()
    {
        Physics.gravity = gravity;
        rb = GetComponent<Rigidbody>();

        // Inicializar la última posición del jugador
        lastPosition = transform.position;

        // Realizar el raycast al principio para posicionar al jugador en el suelo
        PerformRaycast();
    }

    void Update()
    {
        if (!isGrounded)
        {
            timeInAir += Time.deltaTime;
        }
        else
        {
            timeInAir = 0.0f;
            hasBeenInAir = false;
        }
        
        if (Mathf.Abs(Input.GetAxis("Vertical")) > 0 && forwardSpeed < maxForwardSpeed)
        {
            if (transform.position.z > lastPosition.z)
            {
                // Modificar la velocidad hacia adelante basada en la entrada vertical
                forwardSpeed += acceleration * Time.deltaTime;
            }
        }
        else if (Mathf.Abs(Input.GetAxis("Horizontal")) != 0)
        {
            // Obtener la entrada horizontal del teclado
            float horizontalInput = Input.GetAxis("Horizontal");

            // Calcular la rotación basada en la entrada horizontal
            float rotationAmount = horizontalInput * rotationSpeed * Time.deltaTime;

            // Aplicar la reducción de velocidad solo si la rotación supera el umbral
            if (Mathf.Abs(rotationAmount) > rotationThreshold && forwardSpeed > speedThreshold)
            {
                // Reducir la velocidad proporcionalmente a la cantidad de rotación
                forwardSpeed *= 1.0f / (1.0f + Mathf.Abs(rotationAmount) * rotationReductionFactor);
            }

            // Rotar el jugador alrededor del eje Y
            transform.Rotate(Vector3.up, rotationAmount);
        }
        
        // Bloquear input vertical si transform.position.z < lastPosition.z
        if (transform.position.z < lastPosition.z)
        {
            forwardSpeed -= acceleration * Time.deltaTime;
            if (forwardSpeed <= 0)
            {
                forwardSpeed = 0;
                transform.Rotate(Vector3.up, 180f);
                forwardSpeed = 1;
            }
            lastPosition = transform.position;
            // Salir del método Update para bloquear cualquier otro movimiento vertical
            return;
        }

        // Guardar la posición actual del jugador
        lastPosition = transform.position;
    }

    private void FixedUpdate()
    {
        // Si el jugador está en el suelo, aplicar la velocidad hacia adelante
        if (isGrounded)
        {
            // Obtener la dirección hacia donde está mirando el jugador
            Vector3 forwardDirection = transform.forward;

            // Crear un vector de velocidad con la velocidad hacia adelante
            Vector3 newVelocity = forwardDirection * forwardSpeed;

            // Mantener la velocidad en el eje Y (vertical)
            newVelocity.y = rb.velocity.y;

            // Asignar la nueva velocidad al rigidbody
            rb.velocity = newVelocity;

            groundRotation = transform.rotation;

            // Modificar la velocidad inicial de las partículas
            var mainModule = sistemaParticulas.main;
            mainModule.startSpeed = 0.5f * Mathf.Abs(forwardSpeed); // 0.5 es el factor de escala para convertir la velocidad del personaje en la velocidad de las partículas
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Verificar si el personaje colisiona con un objeto etiquetado como "Ground"
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;

            hasBeenInAir = timeInAir >= 0.75f;

            if (hasBeenInAir && !sistemaParticulas.isPlaying)
            {
                // Asignar la rotación almacenada del suelo al sistema de partículas
                sistemaParticulas.transform.rotation = groundRotation;
                // Actualizar la posición del sistema de partículas
                sistemaParticulas.transform.position = transform.position;
                sistemaParticulas.Play();
            }
        }

        // Verificar si el personaje colisiona con un objeto etiquetado como "Obstaculo"
        if (collision.gameObject.CompareTag("Obstaculo"))
        {
            // Teletransportar el objeto a las coordenadas especificadas
            transform.position = new Vector3(0, 1000, -880);

            // Anular la velocidad estableciéndola a cero
            rb.velocity = Vector3.zero;
            forwardSpeed = 10;

            // Realizar un raycast para posicionar al jugador en el suelo
            PerformRaycast();
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        // Verificar si el personaje deja de colisionar con un objeto etiquetado como "Ground"
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }

    private void PerformRaycast()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit))
        {
            if (hit.collider.CompareTag("Ground"))
            {
                // Posicionar al jugador en el suelo
                transform.position = hit.point + Vector3.up * 0.5f; // Ajusta la altura del personaje según sea necesario
                isGrounded = true;
            }
        }
    }
}