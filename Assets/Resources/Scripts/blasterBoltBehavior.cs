using UnityEngine;

public class blasterBoltBehavior : MonoBehaviour
{
    public float movementSpeed = 15.0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Forever move forward until collision
        transform.Translate(Vector2.up * movementSpeed * Time.deltaTime);

        // Destroy after 8 seconds
        Destroy(gameObject, 8.0f);

    }

    private void OnTriggerEnter2D(Collider2D collision){
        Destroy(gameObject);
    }
}
