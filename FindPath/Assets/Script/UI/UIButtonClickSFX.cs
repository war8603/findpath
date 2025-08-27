using FindPath;
using Managers;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

[RequireComponent(typeof(Button))]
public class UIButtonClickSFX : MonoBehaviour
{
    [Inject] private readonly AssetManager _assetManager;
    [Inject] private readonly SoundManager _soundManager;
    
    private void Awake()
    {
        var button = GetComponent<Button>();
        button.onClick.AddListener(OnPlaySound);
    }

    private void OnPlaySound()
    {
        if (_assetManager == null) return;
        if (_soundManager == null) return;
        
        var clip = _assetManager.LoadAudioClip(DataConfig.ButtonClickSoundSEName);
        _soundManager.PlaySound(SoundType.SE, clip);
    }
}
