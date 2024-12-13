using UnityEngine;
using System.Collections;
using System.Linq;

// Create an enum for the different states of the trooper
public enum trooperOrders{
    Patrol,
    Cluster,
    Advance,
    Guard,
    Retreat
}

public enum teams{
    Red,
    Blue
}

public class trooperBehavior : MonoBehaviour
{

    public trooperOrders order;
    private trooperOrders previousOrder;

    public int health = 2;
    public float sightRange = 5.0f;
    public float movementSpeed = 2.0f;
    public float kinematicArrivalThreshold = 0.1f;
    public float rotationSpeed = 4.0f;

    public teams team;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        previousOrder = trooperOrders.Advance;
    }

    // Update is called once per frame
    void Update()
    {
        if (order == trooperOrders.Cluster){
            Cluster();
        }
        if (order == trooperOrders.Patrol){
            Patrol();
        }

        if (order == trooperOrders.Advance){
            Advance();
        }
        if (order == trooperOrders.Retreat){
            Retreat();
        }
        Attack();

        //if (shouldRetreat()){
       //     if (order != trooperOrders.Retreat){
        //        previousOrder = order;
         //   }
        //    
        //    order = trooperOrders.Retreat;
        //} else {
        //    order = previousOrder;
        //}

        
    }

    #region Patrol
    private bool movingToRandom = false;
    private bool initialized = false;  // Track initialization status
    private Vector3 startingPoint;
    private Vector3 randomPoint;

    private void Patrol(){
        // Initialize startingPoint once
        if (!initialized) {
            startingPoint = transform.position;
            randomPoint = calculateRandomPoint(sightRange);
            movingToRandom = true;
            initialized = true;  // Mark as initialized
        }

        // Choose a random point within sight of the player
        if (!movingToRandom) {
            moveTowards(startingPoint);
            if (Vector3.Distance(transform.position, startingPoint) < kinematicArrivalThreshold) {
                movingToRandom = true;
                startingPoint = transform.position;
                randomPoint = calculateRandomPoint(sightRange);
            }
        } else {
            moveTowards(randomPoint);
            if (Vector3.Distance(transform.position, randomPoint) < kinematicArrivalThreshold) {
                movingToRandom = false;
            }
        }
    }

    private Vector3 calculateRandomPoint(float range){
        // Calculate a random point within the range, relative to the player
        float randomX = Random.Range(-range, range);
        float randomY = Random.Range(-range, range);
        Vector3 point = new Vector3(randomX, randomY, 0) + transform.position;
        return point;
    }
    #endregion

    #region Retreat
    private void Retreat(){
        // Move backwards. Calculate backwards based on rotation and position
        Vector3 backwardsPoint = transform.position - transform.up * movementSpeed * Time.deltaTime;
        moveTowards(backwardsPoint);

    }

    private bool shouldRetreat(){
        // Count the number of nearby allied and enemy troops. If enemy > allied, retreat
        int alliedTroopers = GameObject.FindGameObjectsWithTag("trooper")
            .Where(trooper => trooper.GetComponent<trooperBehavior>().getTeam() == team && Vector3.Distance(transform.position, trooper.transform.position) < sightRange*2)
            .Count();
        int enemyTroopers = GameObject.FindGameObjectsWithTag("trooper")
            .Where(trooper => trooper.GetComponent<trooperBehavior>().getTeam() != team && Vector3.Distance(transform.position, trooper.transform.position) < sightRange*2)
            .Count();
        return enemyTroopers > alliedTroopers;
    }
    #endregion
    private void moveTowards(Vector3 point){

        // Calculate the direction towards the point
        Vector3 towardsPoint = point - transform.position;
        towardsPoint = towardsPoint.normalized;

        // Calculate the angle required to face the target on the x-y plane
        float targetAngle = Mathf.Atan2(towardsPoint.y, towardsPoint.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(0, 0, targetAngle);
        
        // Subtract 90 degrees to the target rotation
        targetRotation = Quaternion.Euler(0, 0, targetRotation.eulerAngles.z - 90);

        // Smoothly rotate towards the target only on the z-axis
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        // Move towards the target
        transform.position += towardsPoint * movementSpeed * Time.deltaTime;
    }

    private void turnTowards(Vector3 point){
        // Calculate the direction towards the point
        Vector3 towardsPoint = point - transform.position;
        towardsPoint = towardsPoint.normalized;

        // Calculate the angle required to face the target on the x-y plane
        float targetAngle = Mathf.Atan2(towardsPoint.y, towardsPoint.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(0, 0, targetAngle);
        
        // Subtract 90 degrees to the target rotation
        targetRotation = Quaternion.Euler(0, 0, targetRotation.eulerAngles.z - 90);

        // Smoothly rotate towards the target only on the z-axis
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    #region Attacking
    public float boltDelay = 0.5f;
    private float lastBoltTime = 0.0f;

    public float xOffset = 0.65f;
    public float yOffset = 0.45f;
    // This region contains targeting code and shooting code
    private bool isLinedUpForShot(GameObject target, float tolerance) {
        Vector3 targetPos = target.transform.position;
        Vector3 targetDir = targetPos - transform.position;
        float targetAngle = Mathf.Atan2(targetDir.y, targetDir.x) * Mathf.Rad2Deg;

        // Correct angle difference calculation
        float angleDiff = Mathf.Abs(Mathf.DeltaAngle(transform.rotation.eulerAngles.z + 90, targetAngle));

        return angleDiff < tolerance;
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

    private void Shoot(){
        // Summon a bolt
        SpawnBolts();
    }

    private GameObject getTarget() {
        // Get all GameObjects tagged with "Trooper" and "Player"
        GameObject[] troopers = GameObject.FindGameObjectsWithTag("trooper");
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        
        // Combine arrays of troopers and players
        GameObject[] potentialTargets = troopers.Concat(players).ToArray();

        // Filter by team and distance, ignoring troopers with the same team
        GameObject[] sortedTargets = potentialTargets
            .Where(target => {
                // Check if it's a trooper or player, and get team accordingly
                var trooperBehavior = target.GetComponent<trooperBehavior>();
                var playerBehavior = target.GetComponent<playerMovementBehaviour>();

                // Check if the component exists and if the team is different
                if (trooperBehavior != null) {
                    return getTeam() != trooperBehavior.getTeam();
                } else if (playerBehavior != null) {
                    return getTeam() != playerBehavior.getTeam();
                }
                return false;
            })
            .Where(target => Vector3.Distance(transform.position, target.transform.position) < sightRange)
            .OrderBy(target => Vector3.Distance(transform.position, target.transform.position))
            .ToArray();

        // Return the closest valid target or null if none are found
        if (sortedTargets.Length > 0) {
            return sortedTargets[0];
        }
        return null;
    }   


    private void Attack(){
        GameObject target = getTarget();
        if(target != null && isLinedUpForShot(target, sightRange)){
            Shoot();
        }
    
        if (target != null){
            turnTowards(target.transform.position);
        }

    }   

    #endregion

    #region Cluster
    private void Cluster(){
        // Sum the normalized vectors to all the allied troopers
        GameObject[] allTroopers = GameObject.FindGameObjectsWithTag("trooper").Where(trooper => trooper.GetComponent<trooperBehavior>().getTeam() == team).ToArray();
        Vector3 sum = Vector3.zero;
        foreach (GameObject trooper in allTroopers){
            sum += (trooper.transform.position - transform.position).normalized;
        }

        // Move towards the center of the cluster
        moveTowards(transform.position + sum.normalized);
    }

    #endregion Cluster

    #region Advance
    private void Advance(){
        // Get all gameObjects tagged with "goalCrystal"
        GameObject[] goalCrystals = GameObject.FindGameObjectsWithTag("goalCrystal");

        // Get the goal crystal that is opposite team's.
        GameObject[] sortedGoalCrystals = goalCrystals
            .Where(crystal => getTeam() != crystal.GetComponent<goalCrystalBehavior>().getTeam())
            .ToArray();


        GameObject enemyCrystal = null;
        if (sortedGoalCrystals.Length > 0){
            enemyCrystal = sortedGoalCrystals[0];
        }

        // Only advance if an enemy crystal is found and there are no enemy troopers
        GameObject enemyTrooper = getTarget();
        if (enemyCrystal != null && enemyTrooper == null){
            moveTowards(enemyCrystal.transform.position);
        }
            
        
    }
    #endregion
    
    #region TeamUtils
    public teams getTeam(){
        return team;
    }

    public void setOrder(trooperOrders order){
        this.order = order;
    }
    #endregion

    #region Damage
    public void TakeDamage(int damage){
        health -= damage;
        DamageFlash();
        if (health <= 0){
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other) {
        // Take damage if the trooper collides with a blaster bolt
        if (other.gameObject.tag == "blasterBolt"){
            TakeDamage(1);
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
    #endregion
}   
