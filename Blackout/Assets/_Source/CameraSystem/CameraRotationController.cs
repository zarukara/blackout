using PlayerSystem;
using Unity.Cinemachine;
using UnityEngine;

namespace CameraSystem
{
    public class CameraRotationController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CinemachineFollow cinemachineFollow;

        [Header("Rotation")]
        [SerializeField] private float rotationStep = 90f;
        [SerializeField] private float rotationSpeed = 5f;

        private PlayerInputReader inputReader;

        private Vector3 originalOffset;
        private Vector3 targetOffset;

        public void Initialize(PlayerInputReader inputReader)
        {
            this.inputReader = inputReader;

            originalOffset = cinemachineFollow.FollowOffset;
            targetOffset = originalOffset;

            this.inputReader.CameraLeftPressed += RotateLeft;
            this.inputReader.CameraRightPressed += RotateRight;
        }

        private void Update()
        {
            cinemachineFollow.FollowOffset = Vector3.Lerp(
                cinemachineFollow.FollowOffset,
                targetOffset,
                rotationSpeed * Time.deltaTime
            );
        }

        private void OnDestroy()
        {
            if (inputReader != null)
            {
                inputReader.CameraLeftPressed -= RotateLeft;
                inputReader.CameraRightPressed -= RotateRight;
            }

            if (cinemachineFollow != null)
            {
                cinemachineFollow.FollowOffset = originalOffset;
            }
        }

        private void RotateLeft()
        {
            RotateOffset(-rotationStep);
        }

        private void RotateRight()
        {
            RotateOffset(rotationStep);
        }

        private void RotateOffset(float angle)
        {
            Quaternion rotation = Quaternion.Euler(0f, angle, 0f);
            targetOffset = rotation * targetOffset;
        }
    }
}