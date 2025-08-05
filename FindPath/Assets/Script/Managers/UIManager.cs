using System.Collections.Generic;
using Extensions;
using FindPath;
using UnityEngine;
using VContainer;

namespace Managers
{
    public class UIManager : MonoBehaviour, IBaseManager
    {
        [Inject] private readonly ObjectFactory _objectFactory;
        
        private Transform _mainTransform;
        private Transform _systemTransform;

        private readonly List<UIMainView> _mainViews = new();
        private readonly List<UISystemView> _systemViews = new();
        
        public void InitManager()
        {
            // Canvas 찾기
            var canvasObj = GameObject.FindGameObjectWithTag(TagNames.MainCanvas);
            if (canvasObj == null)
            {
                Debug.LogError("Canvas not found");
                return;
            }
            
            // RectTransform 추가 및 설정 초기화
            transform.InitParent(canvasObj.transform).AddWithInitFullSizeRectTransform();
            
            // 메인 뷰 루트 생성
            var mainRootObj = new GameObject("Main Root");
            _mainTransform = mainRootObj.transform.InitParent(transform).AddWithInitFullSizeRectTransform();

            // 시스템뷰 루트 생성
            var systemRootObj = new GameObject("System Root");
            _systemTransform = systemRootObj.transform.InitParent(transform).AddWithInitFullSizeRectTransform();
            
            // UI 삭제
            foreach (var mainView in _mainViews)
            {
                Destroy(mainView.gameObject);
            }
            _mainViews.Clear();

            foreach (var systemView in _systemViews)
            {
                Destroy(systemView.gameObject);
            }
            _systemViews.Clear();
        }

        public T CreateView<T>(string viewName = "") where T : MonoBehaviour
        {
            var prefabName = string.IsNullOrEmpty(viewName) ? typeof(T).Name : viewName;
            var obj = _objectFactory.LoadUIObject(prefabName);
            if (!obj.TryGetComponent<UIView>(out var uiView))
            {
                Debug.LogError("UIView not found");
                Destroy(obj);
                return null;
            }

            switch (uiView)
            {
                case UIMainView uiMainView:
                    uiMainView.transform.InitParent(_mainTransform);
                    _mainViews.Add(uiMainView);
                    break;
                case UISystemView uiSystemView:
                    uiSystemView.transform.InitParent(_systemTransform);
                    _systemViews.Add(uiSystemView);
                    break;
            }
            
            return uiView as T;
        }

        public void DestroyView(UIView uiView)
        {
            switch (uiView)
            {
                case UIMainView uiMainView:
                    _mainViews.Remove(uiMainView);
                    break;
                case UISystemView uiSystemView:
                    _systemViews.Remove(uiSystemView);
                    break;
            }
            Destroy(uiView.gameObject);
        }
    }
}