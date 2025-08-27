using System.Collections.Generic;
using FindPath;
using UnityEngine;

namespace Managers
{
    public class SoundManager : MonoBehaviour, IBaseManager
    {
        private Transform _root;
        private AudioSource _bgmAudioSource;
        private readonly List<AudioSource> _seAudioSources = new();

        private const int DefaultSECount = 10;
        
        public void InitManager()
        {
            CreateRoot();
            CreateAudioSource();
        }

        private void CreateRoot()
        {
            // root 생성
            if (_root == null)
            {
                _root = new GameObject("SoundManager").transform;
                _root.SetParent(transform);
            }
        }

        private void CreateAudioSource()
        {
            if (_bgmAudioSource == null)
            {
                var bgmSourceObj = new GameObject("AudioSource_BGM");
                bgmSourceObj.transform.SetParent(_root);
                _bgmAudioSource = bgmSourceObj.AddComponent<AudioSource>();
                
                _bgmAudioSource.loop = true;
                _bgmAudioSource.playOnAwake = false;
            }

            if (_seAudioSources.Count == 0)
            {
                for (var i = 0; i < DefaultSECount; i++)
                {
                    CreateSeAudioSource();
                }
            }
        }

        private void CreateSeAudioSource()
        {
            var seSourceObj = new GameObject("AudioSource_SE");
            seSourceObj.transform.SetParent(_root);
            _seAudioSources.Add(seSourceObj.AddComponent<AudioSource>());
        }

        public void PlaySound(SoundType soundType, AudioClip clip)
        {
            if (soundType == SoundType.BGM)
            {
                _bgmAudioSource.Stop();
                _bgmAudioSource.clip = clip;
                _bgmAudioSource.Play();
            }
            else
            {
                var seAudioSource = _seAudioSources.Find(se => se.isPlaying == false);
                if (seAudioSource == null)
                {
                    CreateSeAudioSource();
                    seAudioSource = _seAudioSources[^1];
                }
                seAudioSource.Stop();
                seAudioSource.PlayOneShot(clip);
            }
        }
    }
}