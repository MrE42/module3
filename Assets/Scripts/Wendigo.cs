using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.Linq;

public class Wendigo : MonoBehaviour
{
    public NavMeshAgent agent;

    public Transform player;

    public LayerMask whatIsGround, whatIsPlayer;

    public float health;

    public Animator anim;

    // Stalking
    public bool isStalking, isRunningAway, isReadyToTeleport = true;
    public float stalkRunAway;
    public float timeBetweenTeleport;
    public float stalkDistance;
    public Vector3 runTo;
    public GameObject spawnpointParent;
    public GameObject retreatpointParent;
    private Transform[] spawnpoints;
    private Transform[] retreatpoints;
    Transform optimalRetreat; // expose for dist checks later

    // Patroling
    public Vector3 walkPoint;
    bool walkPointSet;
    public float walkPointRange;

    // Attacking
    public float timeBetweenAttacks;
    bool alreadyAttacked;
    public PlayerManager playerManager;

    // States
    public float sightRange, attackRange;
    public bool playerInSightRange, playerInAttackRange, playerInRunAwayRange;


    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        spawnpoints = spawnpointParent.GetComponentsInChildren<Transform>();
        retreatpoints = retreatpointParent.GetComponentsInChildren<Transform>();

        // remove parent GO from list positioned at 0,0,0
        spawnpoints = spawnpoints.Skip(1).ToArray();
        retreatpoints = retreatpoints.Skip(1).ToArray();
    }

    private void Update()
    {
        // check for sight and attack range
        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);
        playerInRunAwayRange = Physics.CheckSphere(transform.position, stalkRunAway, whatIsPlayer);

        // stalk player before messing with them
        if (isStalking) Stalking(true);

        // set is stalking to false whne found 2/5 parts

        else
        {
            if (!playerInSightRange && !playerInAttackRange) Patroling();
            if (playerInSightRange && !playerInAttackRange) ChasePlayer();
            if (playerInAttackRange && playerInSightRange) AttackPlayer();
        }

        
    }

    private void Stalking(bool comingFromMain)
    {
        if (!isRunningAway)
        {
            setAnim("idle");

            // continuously look at player and do not accoutn for x axis
            //var rotation = Quaternion.LookRotation(player.transform.position - transform.position);
            //rotation.y = 0;
            //transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * 10f);
            transform.LookAt(player);
            transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, transform.eulerAngles.z);
        }

        if (isReadyToTeleport && !isRunningAway && comingFromMain)
        {
            // handle bools
            isReadyToTeleport = false;

            // choose random spot x distance away from player and idle <teleport>
            var playerPos = player.position;
            float randomZ = Random.Range(playerPos.z + stalkDistance, playerPos.z + stalkDistance + Random.Range(0, stalkDistance));
            float randomX = Random.Range(playerPos.x + stalkDistance, playerPos.x + stalkDistance + Random.Range(0, stalkDistance));

            // used to flip x/z in a possible negative dir
            float flipX = Random.Range(0,2) * 2 - 1;
            float flipZ = Random.Range(0,2) * 2 - 1;

            var optimalSpot = new Vector3(randomX * flipX, transform.position.y, randomZ * flipZ);

            //Debug.Log("warped to x: " + randomX);
            //Debug.Log("warped to z: " + randomZ);

            // check each spawnpoint available on map, compare to plr distance. Pick closest one to player at least 50 distance away
            Transform optimalSpawnpoint = null;
            float bestDistance = 999999;

            foreach (Transform spawnpoint in spawnpoints)
            {
                var dist = Vector3.Distance(playerPos, spawnpoint.position);

                if (optimalSpawnpoint == null)
                    optimalSpawnpoint = spawnpoint;
                
                else if (dist < bestDistance && dist > 50f )  // must stay at least 50 units away from plr
                {
                    optimalSpawnpoint = spawnpoint;
                    bestDistance = dist;
                }

                //Debug.Log("spawnpoint found at: " + spawnpoint.position + "distance: " + dist);
            }
            
            // WARP TO BEST SPAWNPOINT -> 
            agent.Warp(optimalSpawnpoint.position);

            //Debug.Log("plr pos: " + player.position);

            // reset teleport time if not running away
            Invoke(nameof(ResetTeleport), timeBetweenTeleport);
        }

        // if player gets within stalkRange, choose a point away from player
        if (playerInRunAwayRange && !isRunningAway)
        {
            CancelInvoke();  // need to cancel invoke so it doesn't tp away while running
            setAnim("run");

            // tell rest of code it is running away now
            isRunningAway = true;

            // run away from player
            transform.rotation = Quaternion.LookRotation(transform.position - player.position); // later add some variablilty?
            
            // calculate line to run towards
            runTo = transform.position + transform.forward * 40f;
            //Debug.Log("running to: " + runTo);

            // runto is point furthest from player, find run to point closest to runto var
            optimalRetreat = null;
            float bestDistance = 999999;

            foreach (Transform rtpoint in retreatpoints)
            {
                var dist = Vector3.Distance(runTo, rtpoint.position);

                if (optimalRetreat == null)
                    optimalRetreat = rtpoint;
                
                else if (dist < bestDistance)  
                {
                    optimalRetreat = rtpoint;
                    bestDistance = dist;
                }
            }

            // move ai to pos
            agent.SetDestination(optimalRetreat.position);
        }  

        // check if agent has reached mesh
        if (isRunningAway)
        {
            var x_dist = Mathf.Abs(transform.position.x - optimalRetreat.position.x);
            var z_dist = Mathf.Abs(transform.position.x - optimalRetreat.position.x);

            //Debug.Log("x: " + x_dist);
            //Debug.Log("z: " + z_dist); 

            if (x_dist < 0.05f && z_dist < 0.05f)
            {
                isRunningAway = false;

                // teleport to new location
                isReadyToTeleport = true;
            }
        }

            
    }

    private void ResetTeleport()
    {
        isReadyToTeleport = true;
    }

    private void Patroling()
    {
        Debug.Log("patroling");
        setAnim("run");

        if (!walkPointSet) SearchWalkPoint();

        if (walkPointSet)
            agent.SetDestination(walkPoint);

        Debug.Log("going to: " + walkPoint);

        Vector3 distanceToWalkPoint = transform.position - walkPoint;
        distanceToWalkPoint.y = 0;

        Debug.Log("distance is: " + distanceToWalkPoint);
        Debug.Log("magnitude is: " + distanceToWalkPoint.magnitude);

        // walkpoint reached
        if (distanceToWalkPoint.magnitude < 1f)
            walkPointSet = false;
    }

    private void SearchWalkPoint()
    {
        // calc random point in range
        float randomZ = Random.Range(-walkPointRange, walkPointRange);
        float randomX = Random.Range(-walkPointRange, walkPointRange);

        walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

        if (Physics.Raycast(walkPoint, -transform.up, 2f, whatIsGround))
            walkPointSet = true;
    }

    private void ChasePlayer()
    {
        setAnim("run");

        agent.SetDestination(player.position);
    }

    private void AttackPlayer()
    {
        setAnim("attack");

        // make sure enemy doesn't move
        agent.SetDestination(transform.position);

        //transform.LookAt(player);
        agent.SetDestination(transform.position);

        if (!alreadyAttacked)
        {
            // attack code here (actually its now in deal damage called by anim event :P)
            
            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }

    private void DealDamage()
    {
        playerManager.TakeDamage(10);
    }

    private void ResetAttack()
    {
        alreadyAttacked = false;

        // run away after dealing damage
        Stalking(false);
    }

    public void TakeDamage(int damage)
    {
        health -= damage;

        if (health <= 0) Invoke(nameof(Destroy), 0.5f);
    }

    private void DestroyEnemy()
    {
        Destroy(gameObject);
    }

    private void setAnim(string name)
    {
        anim.SetBool("attack", false);
        anim.SetBool("run", false);
        anim.SetBool("idle", false);

        anim.SetBool(name, true);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, stalkRunAway);
    }

    // idk cool quote
    //As a Hunter, the scariest sound isn�t one you know well, it�s a sound you�ve never heard before.
    //Why? Because that means something you�ve never seen before is out there with you, and heaven forbid it finds you.
}
