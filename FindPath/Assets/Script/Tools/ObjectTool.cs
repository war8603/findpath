using Unity.VisualScripting;
using UnityEngine;

namespace Extensions
{
    public static class ObjectTool
    {
        public static Transform InitParent(this Transform transform, Transform parent)
        {
            transform.SetParent(parent, worldPositionStays: false);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
            return transform;
        }
        
        public static void InitParent(this GameObject gameObject, Transform parent)
        {
            gameObject.transform.SetParent(parent);
            gameObject.transform.localPosition = Vector3.zero;
            gameObject.transform.localRotation = Quaternion.identity;
            gameObject.transform.localScale = Vector3.one;
        }
        
        public static void InitParent(this RectTransform rectTransform, Transform parent)
        {
            rectTransform.SetParent(parent);
            rectTransform.localPosition = Vector3.zero;
            rectTransform.localRotation = Quaternion.identity;
            rectTransform.localScale = Vector3.one;
        }

        public static RectTransform AddWithInitFullSizeRectTransform(this Transform transform)
        {
            var rectTransform = transform.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;           
            rectTransform.anchorMax = Vector2.one;            
            rectTransform.anchoredPosition = Vector2.zero;    
            rectTransform.sizeDelta = Vector2.zero;           
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            return rectTransform;
        }
    }
}