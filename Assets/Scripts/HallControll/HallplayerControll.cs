using UnityEngine;
using UnityEngine.InputSystem;

public class HallplayerControll : MonoBehaviour
{
    private InputSystem_Actions input;
    public float moveSpeed = 5f;
    private Camera mainCamera;
    private Vector2 moveInput;
    private Vector3 targetPosition;
    private bool movingToClick = false;

    private SpriteRenderer spriteRenderer;
    
    public bool IsMovingToClick => movingToClick;

    private void Awake()
    {
        input = new InputSystem_Actions ();
        spriteRenderer = GetComponent<SpriteRenderer>();
        mainCamera = Camera.main;
    }

    private void OnEnable()
    {
        input.Player.Enable();
        input.Player.Move.performed += OnMovePerformed;
        input.Player.Move.canceled += OnMoveCanceled;
        input.Player.Click.performed += OnClickPerformed;
    }


    private void OnDisable()
    {
        input.Player.Disable();
        input.Player.Move.performed -= OnMovePerformed;
        input.Player.Move.canceled -= OnMoveCanceled;
        input.Player.Click.performed -= OnClickPerformed;
        
    }
    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
        movingToClick = false; 
    }
    
    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        moveInput = Vector2.zero;
    }
    
    private void OnClickPerformed(InputAction.CallbackContext context)
    {
        Vector2 mouseScreenPos = input.Player.Point.ReadValue<Vector2>();
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(mouseScreenPos);
        worldPos.z = transform.position.z;
        worldPos.y = transform.position.y;

        targetPosition = worldPos;
        movingToClick = true;
        
        if (targetPosition.x < transform.position.x)
            spriteRenderer.flipX = false;
        else if (targetPosition.x > transform.position.x)
            spriteRenderer.flipX = true;
    }
    
    private void Update()
    {
        if (Mathf.Abs(moveInput.x) > 0.01f)
        {
            movingToClick = false;
            transform.Translate(Vector3.right * moveInput.x * moveSpeed * Time.deltaTime);

            if (moveInput.x < 0) spriteRenderer.flipX = false;
            else if (moveInput.x > 0) spriteRenderer.flipX = true;
        }
        else if (movingToClick)
        {
            Vector3 newPosition = transform.position;
            newPosition.x = Mathf.MoveTowards(transform.position.x, targetPosition.x, moveSpeed * Time.deltaTime);
            transform.position = newPosition;
            
            if (targetPosition.x > transform.position.x) spriteRenderer.flipX = true;
            else if (targetPosition.x < transform.position.x) spriteRenderer.flipX = false;

            if (Vector3.Distance(transform.position, targetPosition) < 0.05f)
                movingToClick = false;
        }
    }
}
