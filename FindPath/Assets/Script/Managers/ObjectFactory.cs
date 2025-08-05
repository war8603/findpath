using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace FindPath
{
    public class ObjectFactory
    {
        [Inject] private readonly IObjectResolver _objectResolver;
        
        public GameObject LoadGameObject (string name, Transform parent = null)
        {
            var baseObj = Resources.Load<GameObject>(ObjectNames.ObjectPath + name);
            var obj = Object.Instantiate(baseObj, parent);
            obj.name = name;
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.identity;
            obj.transform.localScale = Vector3.one;
            _objectResolver.InjectGameObject(obj);
            return obj;
        }

        public GameObject LoadCellGameObject(string name, Transform parent = null)
        {
            var baseObj = Resources.Load<GameObject>(ObjectNames.CellPath + name);
            var obj = Object.Instantiate(baseObj, parent);
            obj.name = name;
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.identity;
            obj.transform.localScale = Vector3.one;
            _objectResolver.InjectGameObject(obj);
            return obj;
        }

        public T LoadResource<T>(string path, string name) where T : Object
        {
            return Resources.Load<T>(path + name);
        }

        public GameObject LoadUIObject(string name, Transform parent = null)
        {
            var baseObj = Resources.Load<GameObject>(ObjectNames.UIPath + name);
            var obj = Object.Instantiate(baseObj, parent);
            obj.name = name;
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.identity;
            obj.transform.localScale = Vector3.one;
            _objectResolver.InjectGameObject(obj);
            return obj;
        }
    }
}