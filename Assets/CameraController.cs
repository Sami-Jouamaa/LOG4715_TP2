using UnityEngine;

namespace TMPro.Examples
{
    public class CameraController : MonoBehaviour
    {
        public Transform CameraTarget;
        public float FollowDistance = 10f;
        public float VerticalOffset = 2f;
        public bool MovementSmoothing = false;
        public float MovementSmoothingValue = 20f;

        private Vector3 currentVelocity;
        private float side = 1f;

        private void Start()
        {
            if (CameraTarget == null)
            {
                Debug.LogError("CameraController requires a target Transform.", this);
                enabled = false;
                return;
            }

            side = Mathf.Sign(transform.position.x - CameraTarget.position.x);
            if (side == 0f)
                side = 1f;

            transform.rotation = Quaternion.LookRotation(
                Vector3.left * side,
                Vector3.up);
        }

        private void LateUpdate()
        {
            Vector3 targetPosition = CameraTarget.position;
            Vector3 desiredPosition = new Vector3(
                targetPosition.x + side * FollowDistance,
                targetPosition.y + VerticalOffset,
                targetPosition.z);

            if (MovementSmoothing)
            {
                transform.position = Vector3.SmoothDamp(
                    transform.position,
                    desiredPosition,
                    ref currentVelocity,
                    Mathf.Max(0.01f, MovementSmoothingValue * Time.fixedDeltaTime));
            }
            else
            {
                transform.position = desiredPosition;
            }

            transform.rotation = Quaternion.LookRotation(
                Vector3.left * side,
                Vector3.up);
        }
    }
}
