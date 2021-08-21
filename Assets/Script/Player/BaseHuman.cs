using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseHuman : MonoBehaviour
{
    public float speed = 1.2f;
    public string desc = "";
    private Vector3 targetPosition;
    private Animator animator;
    protected bool isMoving = false;
    internal bool isAttacking = false;
    internal float attackTime = float.MinValue;

    // attack management
    public void Attack()
    {
        isAttacking = true;
        attackTime = Time.time;
        animator.SetBool("isAttacking", true);
    }

    public void AttackUpdate()
    {
        if (!isAttacking) return;
        if (Time.time - attackTime < 1.2f) return;
        isAttacking = false;
        animator.SetBool("isAttacking", false);
    }

    // move management
    public void MoveTo(Vector3 pos)
    {
        targetPosition = pos;
        isMoving = true;
        animator.SetBool("isMoving", true);
    }

    public void MoveUpdate()
    {
        if (isMoving == false) return;
        Vector3 pos = transform.position;
        transform.position = Vector3.MoveTowards(pos, targetPosition, speed * Time.deltaTime);
        transform.LookAt(targetPosition);
        if (Vector3.Distance(pos, targetPosition) < 0.02f)
        {
            isMoving = false;
            animator.SetBool("isMoving", false);
        }
    }

    protected void Start()
    {
        animator = GetComponent<Animator>();
    }

    protected void Update()
    {
        MoveUpdate();
        AttackUpdate();
    }
}
