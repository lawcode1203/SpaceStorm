using UnityEngine;
using System.Collections;
using System.Linq;
public class goalCrystalBehavior : MonoBehaviour
{

    public teams team;
    public int maxNumberOfTroopers = 7;
    public bool isStolen = true;
    GameObject thief;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        setColor();
        
    }

    // Update is called once per frame
    void Update()
    {
        SpawnTroopers();

        if (isStolen){
            transform.position = thief.transform.position + new Vector3(0, 0, -1);
        }
        
    }

    public void setColor(){
        // Set the color of the goal crystal based on the team
        if (team == teams.Red){
            GetComponent<SpriteRenderer>().color = Color.red;
        }
        else if (team == teams.Blue){
            GetComponent<SpriteRenderer>().color = Color.blue;
        }
    }

    public teams getTeam(){
        return team;
    }

    public int getNumberOfTroopers(){
        // Get the number of troopers with the same team
        return GameObject.FindGameObjectsWithTag("trooper")
            .Where(trooper => trooper.GetComponent<trooperBehavior>().getTeam() == team)
            .Count();
    }

    private Vector3 calculateRandomPoint(float range){
        // Calculate a random point within the range, relative to the player
        float randomX = Random.Range(-range, range);
        float randomY = Random.Range(-range, range);
        Vector3 point = new Vector3(randomX, randomY, 0) + transform.position;
        return point;
    }

    private void SpawnTroopers(){
        // Spawn troopers until the maximum number is reached
        if (getNumberOfTroopers() < maxNumberOfTroopers){
            // Spawn troopers, set order to advance
            if (team == teams.Red){
                // Random position within the goal area
                // Sprite is /Sprites/RedTrooper
                Vector3 spawnPosition = calculateRandomPoint(1.5f);
                GameObject trooper = GameObject.Instantiate(Resources.Load("Sprites/RedTrooper")) as GameObject;
                trooper.transform.position = spawnPosition;
                trooper.GetComponent<trooperBehavior>().setOrder(trooperOrders.Advance);
                
            }
            else if (team == teams.Blue){
                // Random position within the goal area
                // Sprite is /Sprites/BlueTrooper
                Vector3 spawnPosition = calculateRandomPoint(1.5f);
                GameObject trooper = GameObject.Instantiate(Resources.Load("Sprites/BlueTrooper")) as GameObject;
                trooper.transform.position = spawnPosition;
                trooper.GetComponent<trooperBehavior>().setOrder(trooperOrders.Advance);


            }
        }
    }

    private void beStolen(GameObject trooper){
        isStolen = true;
        thief = trooper;
    }

    void onTriggerEnter2D(Collider2D other){
        if (other.tag == "trooper"){
            // If trooper's team != this team, be stolen by the trooper
            if (other.GetComponent<trooperBehavior>().getTeam() != team){
                beStolen(other.gameObject);
            }
        }
    }
}
