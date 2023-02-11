using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Wendigo : MonoBehaviour
{
    public NavMeshAgent agent;

    public Transform player;

    public LayerMask whatIsGround, whatIsPlayer;

    public float health;

    public Animator anim;

    // Stalking
    public bool isStalking;
    public float stalkRunAway;
    public float timeBetweenTeleport;
    public float stalkDistance;

    // Patroling
    public Vector3 walkPoint;
    bool walkPointSet;
    public float walkPointRange;

    // Attacking
    public float timeBetweenAttacks;
    bool alreadyAttacked;

    // States
    public float sightRange, attackRange;
    public bool playerInSightRange, playerInAttackRange;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        // check for sight and attack range
        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);

        if (isStalking)
        {
            // choose random spot x distance away from player and idle <teleport>
            var playerPos = player.position;
            float randomZ = Random.Range(playerPos.z + stalkDistance, playerPos.z + stalkDistance + stalkDistance);
            float randomX = Random.Range(playerPos.x + stalkDistance, playerPos.x + stalkDistance + stalkDistance);
            // set randomx/y sign, and * by randzomx/z : random(0,1) * 2 - 1
            agent.Warp(new Vector3(randomX, transform.position.y, randomZ));
            isStalking = false;
            Debug.Log("warped to: " + randomX + randomZ);

            // if player gets within stalkRange, choose a point away from player

            // run away from player
        }

        else
        {
            if (!playerInSightRange && !playerInAttackRange) Patroling();
            if (playerInSightRange && !playerInAttackRange) ChasePlayer();
            if (playerInAttackRange && playerInSightRange) AttackPlayer();
        }

        
    }

    private void Patroling()
    {
        setAnim("run");

        if (!walkPointSet) SearchWalkPoint();

        if (walkPointSet)
            agent.SetDestination(walkPoint);

        Vector3 distanceToWalkPoint = transform.position - walkPoint;

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
            // attack code here

            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }

    private void ResetAttack()
    {
        alreadyAttacked = false;
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
