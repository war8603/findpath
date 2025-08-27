using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Managers
{
    public class AssetManager : IBaseManager
    {
        private readonly Dictionary<string, AsyncOperationHandle<GameObject>> _objectHandles = new();
        private readonly Dictionary<string, AsyncOperationHandle<ScriptableObject>> _scriptableObjectHandles = new();
        private readonly Dictionary<string, AsyncOperationHandle<AudioClip>> _audioClipHandles = new();
        
        public void InitManager()
        {
            foreach (var objKey in _objectHandles.Keys)
            {
                _objectHandles[objKey].Release();
            }
            _objectHandles.Clear();
        }

        public async UniTask<GameObject> LoadGameObjectAsync(string name, Transform parent = null)
        {
            if (!_objectHandles.TryGetValue(name, out var handle))
            {
                handle = Addressables.LoadAssetAsync<GameObject>(name);
                _objectHandles.Add(name, handle);
            }

            var baseObj = await LoadAssetAsync<GameObject>(handle);
            var newObj = Object.Instantiate(baseObj, parent);
            newObj.name = name;
            return newObj;
        }

        public GameObject LoadGameObject(string name, Transform parent = null)
        {
            if (!_objectHandles.TryGetValue(name, out var handle))
            {
                handle = Addressables.LoadAssetAsync<GameObject>(name);
                _objectHandles.Add(name, handle);
            }

            var baseObj = LoadAsset<GameObject>(handle);
            var newObj = Object.Instantiate(baseObj, parent);
            newObj.name = name;
            return newObj;
        }

        public T LoadScriptableObject<T>(string name) where T : ScriptableObject
        {
            if (_scriptableObjectHandles.TryGetValue(name, out var handle))
            {
                return LoadAsset<T>(handle);
            }

            handle = Addressables.LoadAssetAsync<ScriptableObject>(name);
            _scriptableObjectHandles.Add(name, handle);
            return LoadAsset<T>(handle);
        }

        private T LoadAsset<T>(AsyncOperationHandle handle)
        {
            handle.WaitForCompletion();
            return (T)handle.Result;
        }

        public AudioClip LoadAudioClip(string name)
        {
            if (_audioClipHandles.TryGetValue(name, out var handle))
            {
                return LoadAsset<AudioClip>(handle);
            }

            handle = Addressables.LoadAssetAsync<AudioClip>(name);
            _audioClipHandles.Add(name, handle);
            return LoadAsset<AudioClip>(handle);
        }

        private async UniTask<T> LoadAssetAsync<T>(AsyncOperationHandle handle)
        {
            await handle;
            if (handle.IsDone && handle.IsValid())
            {
                return (T)handle.Result;
            }
            
            Debug.LogError($"[LoadAssetGameObject] OnFail : {handle}");
            return default;
        }
    }
}