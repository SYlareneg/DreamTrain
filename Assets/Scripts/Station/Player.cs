using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class Player : MonoBehaviour
{
    [Header("플레이어블 캐릭터")]
    [SerializeField] float speed;
    public Vector2 moveTowards;
    bool isMove;
    Rigidbody2D rb;
    SpriteRenderer spriteRenderer;
    private InputSystem_Actions input;


    void PlayerMove(Vector2 pos)
    {
        moveTowards = pos;
        isMove = true;
    }

    private void OnEnable()
    {
        input.Player.Enable();
        input.Player.Click.performed += OnClickPerformed;
    }


    private void OnDisable()
    {
        input.Player.Disable();
        input.Player.Click.performed -= OnClickPerformed;

    }

    private void OnClickPerformed(InputAction.CallbackContext context)
    {
        if (PlayerManager.Inst.isLoading) return;
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D[] hits = Physics2D.RaycastAll(mousePos, Vector2.zero);

        foreach(var hit in hits)
        {
            if (hit.collider != null)
            {
                Debug.Log("Clicked on: " + hit.collider.name);

                PlayerInteractableObject interactable = hit.collider.GetComponent<PlayerInteractableObject>();
                if (interactable != null && interactable.isInteractable == true)
                {
                    if (interactable.alreadyInteracted)
                    {
                        StartCoroutine(PlayerManager.Inst.ShowPlayerSpeech());
                        return;
                    }
                    interactable.Interact();
                    return;
                }
            }
        }

        PlayerMove(Utils.MousePos);
    }

    private void CheckMove()
    {
        if (PlayerManager.Inst.isLoading) return;
        Vector2 moveDelta = input.Player.Move.ReadValue<Vector2>();
        moveDelta *= speed * Time.fixedDeltaTime;
        if (moveDelta.magnitude > 0)
        {
            PlayerMove(rb.position + moveDelta);
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        PlayerInteractableObject interObj = collision.gameObject.GetComponent<PlayerInteractableObject>();
        if (interObj != null)
        {
            interObj.isInteractable = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        PlayerInteractableObject interObj = collision.gameObject.GetComponent<PlayerInteractableObject>();
        if (interObj != null)
        {
            interObj.isInteractable = false;
        }
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;

        spriteRenderer = GetComponent<SpriteRenderer>();

        input = new InputSystem_Actions();
    }

    void FixedUpdate()
    {
        CheckMove();

        Vector2 moveDir = moveTowards - rb.position;

        Vector2 deltaPos = moveDir.normalized * speed * Time.fixedDeltaTime;
        if (moveDir.magnitude > 0.1f)
        {
            rb.MovePosition(rb.position + deltaPos);

            if (moveDir.x > 0) spriteRenderer.flipX = true;
            else if (moveDir.x < 0) spriteRenderer.flipX = false;
        }
        else
        {
            rb.MovePosition(moveTowards);
            isMove = false;
        }
    }
}
