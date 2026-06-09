using UnityEngine;

namespace PlayerSystem
{
    public class PlayerRotation : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerInputReader inputReader;
        [SerializeField] private Camera mainCamera;

        [Header("Rotation")]
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private float rotationSpeed = 20f;

        private void Update()
        {
            RotateToCursor();
        }

        private void RotateToCursor()
        {
            Ray ray = mainCamera.ScreenPointToRay(inputReader.LookInput);

            if (Physics.Raycast(ray, out RaycastHit hit, 1000f, groundLayer))
            {
                Vector3 direction = hit.point - transform.position;
                direction.y = 0f;

                if (direction.sqrMagnitude <= 0.001f)
                    return;

                Quaternion targetRotation = Quaternion.LookRotation(direction);

                transform.rotation = Quaternion.Lerp(
                    transform.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime
                );
            }
        }
    }
}