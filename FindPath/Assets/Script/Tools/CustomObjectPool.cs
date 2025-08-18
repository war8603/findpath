using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Extensions;
using FindPath;
using Managers;
using UnityEngine;
using VContainer;

public class CustomObjectPool : MonoBehaviour, IBaseManager
{
    [Inject] private readonly ObjectFactory _objectFactory;
    
    private Dictionary<string, Transform> _objectRoot;
    private Dictionary<string, Dictionary<GameObject, bool>> _objects;

    public void InitManager()
    {
        _objectRoot = new Dictionary<string, Transform>();
        _objects = new Dictionary<string, Dictionary<GameObject, bool>>();
    }

    public GameObject GetGameObject(string objName, Transform root = null)
    {
        var objects = GetObjects(objName);
        if (objects.Count > 0)
        {
            var objectKey = objects.Where(x => x.Value == false).Select(x => x.Key).FirstOrDefault();
            if (objectKey != null)
            {
                objects[objectKey] = true;
                if (root != null)
                {
                    objectKey.InitParent(root);    
                }

                objectKey.SetActive(true);
                return objectKey;
            }
        }

        var newObject = _objectFactory.LoadGameObject(objName, GetRoot(objName));
        objects.Add(newObject, true);
        if (root != null)
        {
            newObject.InitParent(root);        
        }
        return newObject;
    }

    private Transform GetRoot(string objName)
    {
        if (!_objectRoot.TryGetValue(objName, out var root))
        {
            _objectRoot.Add(objName, root = new GameObject(objName + "Root").transform);
        }
        
        return root;
    }

    private Dictionary<GameObject, bool> GetObjects(string objName)
    {
        if (!_objects.TryGetValue(objName, out var objects))
        {
            objects = new Dictionary<GameObject, bool>();
            _objects.Add(objName, objects);
        }
        return objects;
    }

    public void ReleaseGameObject(GameObject obj)
    {
        var objName = obj.name;
        if (!_objects.TryGetValue(objName, out var objDictionary)) return;
        if (!objDictionary.ContainsKey(obj)) return;
        
        _objects[objName][obj] = false;
        var root = GetRoot(objName);
        obj.InitParent(root);
        obj.SetActive(false);
    }
}
