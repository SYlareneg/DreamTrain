using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class RoomPlayer : MonoBehaviour
{
    public static RoomPlayer Inst;

    [Header("플레이어블 캐릭터")]
    [SerializeField] float speed;
    public Vector2 moveTowards;
    public bool isInteractable;
    Rigidbody2D rb;
    SpriteRenderer spriteRenderer;
    private InputSystem_Actions input;
    Animator animator;
    public List<RoomClickableObject> nearbyInteractables = new List<RoomClickableObject>();
    AudioSource audioSource;

    void PlayerMove(Vector2 pos)
    {
        if(!isInteractable) return;
        moveTowards = pos;
    }

    private void OnEnable()
    {
        input.Player.Enable();
        input.Player.Click.performed += OnClickPerformed;
        input.Player.Interact.performed += OnInteractPerformed;
    }


    private void OnDisable()
    {
        input.Player.Disable();
        input.Player.Click.performed -= OnClickPerformed;
        input.Player.Interact.performed -= OnInteractPerformed;
    }

    private void OnClickPerformed(InputAction.CallbackContext context)
    {
        if(!isInteractable) return;
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D[] hits = Physics2D.RaycastAll(mousePos, Vector2.zero);

        foreach(var hit in hits)
        {
            if (hit.collider != null)
            {
                Debug.Log("Clicked on: " + hit.collider.name);

                RoomClickableObject interactable = hit.collider.GetComponent<RoomClickableObject>();
                if (interactable != null && interactable.isInteractable)
                {
                    interactable.Interact();
                    return;
                }
            }
        }

        PlayerMove(Utils.MousePos);
    }

    private void OnInteractPerformed(InputAction.CallbackContext context)
    {
        if(!isInteractable) return;
        if(nearbyInteractables.Count > 0)
        {
            nearbyInteractables.Sort((a, b) => 
                Vector2.Distance(a.transform.position, rb.position)
                .CompareTo(Vector2.Distance(b.transform.position, rb.position))
            );
            nearbyInteractables[0].Interact();
        }
    }

    private void CheckMove()
    {
        // if (PlayerManager.Inst.isLoading) return;
        if(!isInteractable) return;
        Vector2 moveDelta = input.Player.Move.ReadValue<Vector2>();
        moveDelta *= speed * Time.fixedDeltaTime;
        if (moveDelta.magnitude > 0)
        {
            PlayerMove(rb.position + moveDelta);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        RoomClickableObject interObj = collision.gameObject.GetComponent<RoomClickableObject>();
        if (interObj != null && collision.isTrigger == false)
        {
            interObj.isInteractable = true;
            nearbyInteractables.Add(interObj);
        }

        RoomTriggerObject triggerObj = collision.gameObject.GetComponent<RoomTriggerObject>();
        if(triggerObj != null && collision.isTrigger == true)
        {
            triggerObj.Trigger();
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        RoomClickableObject interObj = collision.gameObject.GetComponent<RoomClickableObject>();
        if (interObj != null && collision.isTrigger == false)
        {
            interObj.isInteractable = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        RoomClickableObject interObj = collision.gameObject.GetComponent<RoomClickableObject>();
        if (interObj != null && collision.isTrigger == false)
        {
            interObj.isInteractable = false;
            nearbyInteractables.Remove(interObj);
        }
    }

    void Awake()
    {
        Inst = this;
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;

        spriteRenderer = GetComponent<SpriteRenderer>();

        input = new InputSystem_Actions();
        animator = GetComponent<Animator>();

        audioSource = GetComponent<AudioSource>();
    }

    public void WalkSound()
    {
        audioSource.PlayOneShot(audioSource.clip);
    }

    void Start()
    {
        // moveTowards = rb.position;
    }

    Vector2 lastPos;

    void FixedUpdate()
    {
        CheckMove();

        Vector2 moveDir = moveTowards - rb.position;

        Vector2 deltaPos = moveDir.normalized * speed * Time.fixedDeltaTime;
        if (moveDir.magnitude > 0.1f)
        {
            rb.MovePosition(rb.position + deltaPos);

            if(Mathf.Abs(moveDir.normalized.x) > Mathf.Abs(moveDir.normalized.y))
            {
                animator.SetBool("MoveLeft", moveDir.x < 0 && lastPos != rb.position);
                animator.SetBool("MoveRight", moveDir.x > 0 && lastPos != rb.position);
                animator.SetBool("MoveFront", false);
                animator.SetBool("MoveBack", false);
            }
            else
            {
                animator.SetBool("MoveBack", moveDir.y > 0 && lastPos != rb.position);
                animator.SetBool("MoveFront", moveDir.y < 0 && lastPos != rb.position);
                animator.SetBool("MoveLeft", false);
                animator.SetBool("MoveRight", false);
            }
        }
        else
        {
            rb.MovePosition(moveTowards);

            animator.SetBool("MoveFront", false);
            animator.SetBool("MoveBack", false);
            animator.SetBool("MoveLeft", false);
            animator.SetBool("MoveRight", false);
        }
        lastPos = rb.position;
    }
}
