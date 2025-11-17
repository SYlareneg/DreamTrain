using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace PassengerScene
{
    public class PassengerPlayerControll : MonoBehaviour
    {
        private InputSystem_Actions input;
        private Camera mainCamera;

        public float moveSpeed = 5f;
        private Vector2 moveInput;
        private Vector3 targetPosition;
        private bool movingToClick = false;
        private SpriteRenderer spriteRenderer;

        private DoorControll currentDoor;
        private bool clickTriggered = false;
        
        public float minX;
        public float maxX;

        private void Awake()
        {
            input = new InputSystem_Actions();
            mainCamera = Camera.main;
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void OnEnable()
        {
            input.Player.Enable();
            input.Player.Move.performed += OnMovePerformed;
            input.Player.Move.canceled += OnMoveCanceled;
            input.Player.Click.performed += OnClickPerformed;
            input.Player.EnterRoom.performed += OnEnterRoomPerformed;
        }

        private void OnDisable()
        {
            input.Player.Disable();

            input.Player.Move.performed -= OnMovePerformed;
            input.Player.Move.canceled -= OnMoveCanceled;
            input.Player.Click.performed -= OnClickPerformed;
            input.Player.EnterRoom.performed -= OnEnterRoomPerformed;
        }
        
        private void OnMovePerformed(InputAction.CallbackContext ctx)
        {
            moveInput = ctx.ReadValue<Vector2>();
            movingToClick = false;
        }

        private void OnMoveCanceled(InputAction.CallbackContext ctx)
        {
            moveInput = Vector2.zero;
        }

        private void Update()
        {
            HandleMovement();

            if (clickTriggered)
            {
                clickTriggered = false;
                HandleClickAction(); 
            }
        }

        private void HandleMovement()
        {
            // Keyboard move
            if (moveInput.sqrMagnitude > 0.01f)
            {
                movingToClick = false;

                transform.Translate(Vector3.right * moveInput.x * moveSpeed * Time.deltaTime);

                // 방향 전환
                if (moveInput.x < 0) spriteRenderer.flipX = false;
                else if (moveInput.x > 0) spriteRenderer.flipX = true;

                Vector3 pos = transform.position;
                pos.x = Mathf.Clamp(pos.x, minX, maxX);
                transform.position = pos;

                return;
            }

            // Click move
            if (movingToClick)
            {
                Vector3 newPos = transform.position;
                newPos.x = Mathf.MoveTowards(transform.position.x, targetPosition.x, moveSpeed * Time.deltaTime);
                transform.position = newPos;

                // 방향 전환
                if (targetPosition.x > transform.position.x) spriteRenderer.flipX = true;
                else if (targetPosition.x < transform.position.x) spriteRenderer.flipX = false;

                // === X 좌표 Clamp ===
                Vector3 clamped = transform.position;
                clamped.x = Mathf.Clamp(clamped.x, minX, maxX);
                transform.position = clamped;

                if (Mathf.Abs(transform.position.x - targetPosition.x) < 0.05f)
                    movingToClick = false;
            }
        }


        public void OnClickPerformed(InputAction.CallbackContext ctx)
        {
            if (ctx.performed)
                clickTriggered = true;
        }

        private void HandleClickAction()
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            Vector2 mousePos = input.Player.Point.ReadValue<Vector2>();
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(mousePos);

            // === 2D Raycast로 변경 (핵심) ===
            Vector2 rayOrigin = new Vector2(worldPos.x, worldPos.y);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.zero);

            if (hit.collider != null)
            {
                Debug.Log(hit.collider.name);

                if (hit.collider.CompareTag("Unwelcomed"))
                {
                    SceneManager.LoadScene("BattleScene");
                    return;
                }
            }

            worldPos.z = transform.position.z;
            worldPos.y = transform.position.y;

            targetPosition = worldPos;
            movingToClick = true;
        }


        public void SetCurrentDoor(DoorControll door)
        {
            currentDoor = door;
        }

        public void ClearCurrentDoor(DoorControll door)
        {
            if (currentDoor == door)
                currentDoor = null;
        }

        private void OnEnterRoomPerformed(InputAction.CallbackContext context)
        {
            if (currentDoor == null)
            {
                Debug.Log("No door nearby.");
                return;
            }

            SceneManager.LoadScene(currentDoor.roomSceneName);
        }
    }
}
