using UnityEngine;

namespace PlayerSystem
{
    public class PlayerRotation : MonoBehaviour
    {
        [Header("Rotation")]
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private float rotationSpeed = 20f;

        private PlayerInputReader inputReader;
        private Camera mainCamera;

        public void Initialize(PlayerInputReader inputReader, Camera mainCamera)
        {
            this.inputReader = inputReader;
            this.mainCamera = mainCamera;
        }

        private void Update()
        {
            if (inputReader == null || mainCamera == null)
                return;

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