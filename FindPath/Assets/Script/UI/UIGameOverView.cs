using Managers;
using UnityEngine;
using VContainer;

namespace FindPath
{
    public class UIGameOverView : UIMainView
    {
        [Inject] private readonly UIManager _uiManager;
        
        [SerializeField] private float _duration;

        private void Update()
        {
            _duration -= Time.deltaTime;
            if (_duration <= 0)
            {
                _uiManager.DestroyView(this);
            }
        }
    }
}