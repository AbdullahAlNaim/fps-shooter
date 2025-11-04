using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private CharacterController controller;

    public float speed = 12f;
    public float gravity = -9.81f * 2;
    public float jumpHieght = 3f;

    public Transform groundCheck;
    public float groundDistance = 0.8f;
    public LayerMask groundMask;

    Vector3 velocity;

    bool isGrounded;
    bool isMoving;

    private Vector3 lastPosition = new Vector3(0f, 0f, 0f);

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }


    void Update()
    {
        // Ground check
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        // Create moving vector
        Vector3 move = transform.right * x + transform.forward * z;

        // Actually move the player
        controller.Move(move * speed * Time.deltaTime);

        // Check if the player can jump
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            // Actually jump
            velocity.y = Mathf.Sqrt(jumpHieght * -2f * gravity);
            Debug.Log("JUMPING");
        }

        // Falling down
        velocity.y += gravity * Time.deltaTime;

        // Executing the jump
        controller.Move(velocity * Time.deltaTime);

        if (lastPosition != gameObject.transform.position && isGrounded == true)
        {
            isMoving = true;
        }
        else
        {
            isMoving = false;
        }

        lastPosition = gameObject.transform.position;

    }
}
