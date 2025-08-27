using Cysharp.Threading.Tasks;
using GoogleMobileAds.Api;
using UnityEngine;
using UnityEngine.Events;

namespace Managers
{
    public class AdsManager : IBaseManager
    {
        // 배너 광고
        private string _bannerAdUnitId = "ca-app-pub-3940256099942544/6300978111";
        private BannerView _bannerView;

        // 보상형 광고
        private string _rewardedAdUnitId = "ca-app-pub-3940256099942544/1033173712";
        private RewardedAd _rewardedAd;
        
        private UniTaskCompletionSource _taskCompletionSource;

        public void InitManager()
        {
            Init().Forget();
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
            if (_bannerView == null)
            {
                // 배너 뷰 생성
                CreateBannerView();
            }

            if (_bannerView == null) return;
            
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
            MobileAds.RaiseAdEventsOnUnityMainThread = true;
            if (_rewardedAd != null && _rewardedAd.CanShowAd())
            {
                _rewardedAd.Show((Reward reward) =>
                {
                    if (reward == null)
                    {
                        Debug.LogError("[AdsManager] ShowAd Reward is null");
                        onFailedCallback?.Invoke();
                    }
                    else
                    {
                        Debug.Log($"[AdsManager] ShowAd Reward = {reward.Type} {reward.Amount}");
                        onsuccessCallback?.Invoke();
                    }
                    
                    LoadRewardedAd();
                });
            }
            else
            {
                onFailedCallback?.Invoke();
                LoadRewardedAd();
            }
        }

        private void LoadRewardedAd()
        {
            // Clean up the old ad before loading a new one.
            if (_rewardedAd != null)
            {
                _rewardedAd.Destroy();
                _rewardedAd = null;
            }

            Debug.Log("Loading the rewarded ad.");

            // create our request used to load the ad.
            var adRequest = new AdRequest();

            // send the request to load the ad.
            RewardedAd.Load(_rewardedAdUnitId, adRequest,
                (RewardedAd ad, LoadAdError error) =>
                {
                    // if error is not null, the load request failed.
                    if (error != null || ad == null)
                    {
                        Debug.LogError("Rewarded ad failed to load an ad " +
                                       "with error : " + error);
                        return;
                    }

                    Debug.Log("Rewarded ad loaded with response : "
                              + ad.GetResponseInfo());

                    _rewardedAd = ad;
                });
        }
#endregion

    }
}
