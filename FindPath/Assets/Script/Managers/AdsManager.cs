using Cysharp.Threading.Tasks;
using FindPath;
using GoogleMobileAds.Api;
using UnityEngine;
using UnityEngine.Events;

namespace Managers
{
    public class AdsManager : MonoBehaviour, IBaseManager
    {
        // 배너 광고
        private string _bannerAdUnitId = "ca-app-pub-3940256099942544/6300978111";
        private BannerView _bannerView;

        // 보상형 광고
#if UNITY_ANDROID
        private const string RewardedAdUnitId = "ca-app-pub-3940256099942544/5224354917"; // Rewarded (Android)
#elif UNITY_IOS
        private const string RewardedAdUnitId = "ca-app-pub-3940256099942544/1712485313"; // Rewarded (iOS 테스트용)
#endif
        private RewardedAd _rewardedAd;
        private bool _isShowing;
        
        private UniTaskCompletionSource _taskCompletionSource;

        private SafeAreaFitter _safeAreaFitter;
        private SafeAreaFitter Fitter => _safeAreaFitter ??= ResolveFitter();

        private SafeAreaFitter ResolveFitter()
        {
            var go = GameObject.FindGameObjectWithTag(TagNames.MainCanvas);
            if (!go) return null;
            return go.GetComponent<SafeAreaFitter>() 
                   ?? go.GetComponentInChildren<SafeAreaFitter>(true);
        }
        
        private void ApplyBannerBottomInset()
        {
            var h = _bannerView.GetHeightInPixels();   // float
            var bannerPx = Mathf.Clamp(Mathf.CeilToInt(h), 0, Screen.height);
            Fitter?.SetReservedInsets(0, 0, 0, bannerPx);
        }
        
        public void InitManager()
        {
            MobileAds.RaiseAdEventsOnUnityMainThread = true;
            RegistSafeAreaFitter();
            Init().Forget();
        }

        private void RegistSafeAreaFitter()
        {
            
        }

        private async UniTask Init()
        {
            _taskCompletionSource = new UniTaskCompletionSource();
           
            // Google Mobile Ads 초기화
            MobileAds.Initialize((InitializationStatus initStatus) =>
            {
                Debug.Log($"MobileAds 초기화 완료");
                
                // 배너 광고 로드
                LoadBannerAd();

                // 보상형 광고 로드
                LoadRewardedAd();
                
                _taskCompletionSource.TrySetResult();
            });
            
            await _taskCompletionSource.Task;
        }

#region BannerView
        private void LoadBannerAd()
        {
            // 배너 뷰 생성
            CreateBannerView();

            if (_bannerView == null) return;
            
            _bannerView.OnBannerAdLoaded += () =>
            {
                ApplyBannerBottomInset();
            };
            
            var adRequest = new AdRequest();
            _bannerView.LoadAd(adRequest);
        }
        
        /// <summary>
        /// 배너 뷰 생성
        /// </summary>
        private void CreateBannerView() 
        {
            if (_bannerView != null)
            {
                // 기존 광고 제거
                DestroyBannerView();
            }

            var adaptiveSize = AdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(AdSize.FullWidth);
            _bannerView = new BannerView(_bannerAdUnitId, adaptiveSize, AdPosition.Bottom);
        }
        
        /// <summary>
        /// 배너 뷰 광고 제거
        /// </summary>
        private void DestroyBannerView()
        {
            if (_bannerView != null)
            {
                Debug.Log("배너 광고 제거.");
                _bannerView.Destroy();
                _bannerView = null;
            }
        }
#endregion

#region RewardedAd
        public void ShowRewardedAd(UnityAction onsuccessCallback, UnityAction onFailedCallback)
        {
            if (_isShowing) return;

            if (_rewardedAd == null || !_rewardedAd.CanShowAd()) 
            {
                onFailedCallback?.Invoke();
                LoadRewardedAd();
                return;
            }

            _isShowing = true;
            var earned = false;

            _rewardedAd.OnAdFullScreenContentFailed += (AdError error) =>
            {
                _isShowing = false;
                onFailedCallback?.Invoke();
                LoadRewardedAd();
            };

            _rewardedAd.OnAdFullScreenContentClosed += () =>
            {
                // 닫힌 뒤 재로딩 (가장 안전)
                _rewardedAd?.Destroy();
                _rewardedAd = null;
                LoadRewardedAd();

                _isShowing = false;
                if (earned)
                {
                    onsuccessCallback?.Invoke();
                }
                else
                {
                    onFailedCallback?.Invoke();
                }
            };

            try
            {
                _rewardedAd.Show(reward => 
                {
                    earned = true;
                    Debug.Log($"[Ads] Earned: {reward?.Type} {reward?.Amount}");
                });
            } 
            catch (System.Exception e)
            {
                Debug.LogError($"[Ads] Show failed: {e}");
                _isShowing = false;
                onFailedCallback?.Invoke();
                LoadRewardedAd();
            }
        }

        private void LoadRewardedAd()
        {
            _rewardedAd?.Destroy();
            _rewardedAd = null;

            Debug.Log("[Ads] Loading rewarded...");
            var req = new AdRequest();

            RewardedAd.Load(RewardedAdUnitId, req, (ad, error) =>
            {
                if (error != null || ad == null) 
                {
                    Debug.LogError($"[Ads] Load failed: {error}");
                    return;
                }
                _rewardedAd = ad;
                Debug.Log($"[Ads] Loaded: {ad.GetResponseInfo()}");
            });
        }
#endregion

    }
}
