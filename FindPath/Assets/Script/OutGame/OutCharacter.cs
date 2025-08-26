using Managers;
using UnityEngine;
using VContainer;

namespace FindPath
{
    public class OutCharacter : MonoBehaviour
    {
        private static readonly int Run = Animator.StringToHash("run");
        [Inject] private readonly OutGameManager _outGameManager;

        [SerializeField] private float _walkSpeed;
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private BoxCollider2D _collider;
        [SerializeField] private Animator _animator;

        private Vector2 _direction = new Vector2(1f, 0);

        private void Awake()
        {
            ChangeDirection();
            _animator.SetTrigger(Run);
        }
        
        public void Update()
        {
            DoWalk();
        }

        private void DoWalk()
        {
            var nextPosition = transform.position + (Vector3)_direction * _walkSpeed * Time.deltaTime;
            if (_outGameManager.IsWalkable(nextPosition, _collider.bounds.size))
            {
                transform.position = nextPosition;
            }
            else
            {
                ChangeDirection();
            }
        }

        private void ChangeDirection()
        {
            _direction *= -1;
            _spriteRenderer.flipX = !(_direction.x > 0);
        }
    }
}