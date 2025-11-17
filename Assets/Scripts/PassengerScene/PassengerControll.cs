using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace PassengerScene
{
    public class PassengerControll : MonoBehaviour
    {
        [SerializeField] private GameObject bat;
        [SerializeField] private GameObject ghost;

        private bool clickTriggered = false;
        void Update()
        {
            if (!clickTriggered) return;
            clickTriggered = false;

            if (EventSystem.current.IsPointerOverGameObject())
                return;

            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider.gameObject == bat || hit.collider.gameObject == ghost)
                {
                    SceneManager.LoadScene("BattleScene");
                }
            }
        }
    
        public void OnClickPerformed(InputAction.CallbackContext ctx)
        {
            if (ctx.performed)
            {
                clickTriggered = true;
            }
        }
    }
}
