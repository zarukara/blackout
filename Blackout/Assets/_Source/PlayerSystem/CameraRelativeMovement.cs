using UnityEngine;

namespace PlayerSystem
{
    internal static class CameraRelativeMovement
    {
        public static Vector3 GetDirection(Vector2 input, Transform cameraTransform)
        {
            Vector3 forward = cameraTransform.forward;
            Vector3 right = cameraTransform.right;

            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();

            // Movement preserves analog input; dash normalizes the result at its call site.
            return right * input.x + forward * input.y;
        }
    }
}
