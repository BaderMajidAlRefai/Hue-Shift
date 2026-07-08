using UnityEngine;

public class Mohammed : MonoBehaviour
{
    public float speed = 2f;
    private Rigidbody2D rb;
    private Vector2 movement; 

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        
    }

    void Update()
    {
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
    }

    void FixedUpdate()
    {   
       rb.linearVelocity = movement * speed;
    }
}
