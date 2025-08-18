using UnityEngine;

namespace FindPath
{
    public class MainCameraFollower : MonoBehaviour
    {
        private Transform _targetTransform;
        
        public void SetTarget(Transform target)
        {
            _targetTransform = target;
            UpdatePositionToTarget();
        }

        public void Update()
        {
            if (_targetTransform == null) return;
            UpdatePositionToTarget();
        }

        private void UpdatePositionToTarget()
        {
            transform.position = new Vector3(_targetTransform.position.x, _targetTransform.position.y, transform.position.z);
        }

        public void InitPosition()
        {
            _targetTransform = null;
            transform.position = new Vector3(0f, 0f, transform.position.z);
        }
    }
}