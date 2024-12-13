using UnityEngine;


public class playerMovementBehaviour : MonoBehaviour
{
    private Rigidbody2D rb;
    public int health = 2;
    public teams team;
    public float forceMultiplier = 10.0f;

    public float boltDelay = 0.5f;
    private float lastBoltTime = 0.0f;

    public float xOffset = 0.65f;
    public float yOffset = 0.45f;

    private Vector3 startingPoint;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        startingPoint = transform.position;
        
    }

    // Update is called once per frame
    void Update()
    {
        faceTowardsMouse();
        getWASDInput();
        // If no key is pressed, apply friction
        if (!Input.anyKey){
            applyFriction();
        }

        // If left click is pressed, spawn a bolt
        if (Input.GetMouseButtonDown(0)){
            SpawnBolts();
        }
    }

    private void faceTowardsMouse(){
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = mousePos - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle-90, Vector3.forward);
    }

    private void getWASDInput(){
        if(Input.GetKey(KeyCode.W)){
            rb.AddForce(transform.up * forceMultiplier);
        }
        if(Input.GetKey(KeyCode.S)){
            rb.AddForce(transform.up * -forceMultiplier);
        }
        if(Input.GetKey(KeyCode.A)){
            rb.AddForce(transform.right * -forceMultiplier);
        }
        if(Input.GetKey(KeyCode.D)){
            rb.AddForce(transform.right * forceMultiplier);
        }

        // Normalize the velocity
        if(rb.linearVelocity.magnitude > forceMultiplier){
            rb.linearVelocity = rb.linearVelocity.normalized * forceMultiplier;
        }
    }

    private void applyFriction(){
        rb.linearVelocity *= 0.75f;
    }

    private void SpawnBolts(){
        Vector3 rotatedOffset = transform.up * xOffset + transform.right * yOffset;

        if(Time.time > lastBoltTime + boltDelay){
            GameObject bolt = GameObject.Instantiate(Resources.Load("Sprites/Blaster Bolt")) as GameObject;
            bolt.transform.position = transform.position + rotatedOffset;
            lastBoltTime = Time.time;

            bolt.transform.rotation = transform.rotation;
        }
    }

    public teams getTeam(){
        return team;
    }

    private void OnTriggerEnter2D(Collider2D collision){
        if (collision.gameObject.tag == "blasterBolt"){
            TakeDamage(1);
        }
    }

    public void TakeDamage(int damage){
        health -= damage;
        DamageFlash();
        if (health <= 0){
            Respawn();
        }
    }

    private Color originalColor;
    private void DamageFlash(){
        // Flash the sprite red
        originalColor = GetComponent<SpriteRenderer>().color;
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.color = Color.red;
        Invoke("ResetColor", 0.1f);
    }

    private void ResetColor(){
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.color = originalColor;
    }

    private void Respawn(){
        transform.position = startingPoint;
        health = 2;
    }

}
