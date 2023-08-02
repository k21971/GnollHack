﻿using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xamarin.Forms;
using System.Runtime.InteropServices;
using GnollHackX;
using Android;
using Java.IO;
using System.IO;
using Xamarin.Google.Android.Play.Core.AssetPacks;
using Android.Content.PM;
using System.Runtime.Remoting.Contexts;
using Xamarin.Google.Android.Play.Core.Review;
using System.Threading.Tasks;
using Xamarin.Google.Android.Play.Core.Review.Model;
using Xamarin.Google.Android.Play.Core.Tasks;
using System.Threading;
using Xamarin.Google.Android.Play.Core.Review.Testing;
using Android.Util;

[assembly: Dependency(typeof(GnollHackX.Droid.PlatformService))]
namespace GnollHackX.Droid
{
    public class PlatformService : IPlatformService
    {
        public string GetVersionString()
        {
            var context = Android.App.Application.Context;
            if (Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.Tiramisu)
            {
                return context.PackageManager.GetPackageInfo(context.PackageName, PackageManager.PackageInfoFlags.Of(0L)).VersionName;
            }
            else
            {
#pragma warning disable CS0618 // Type or member is obsolete
                return context.PackageManager.GetPackageInfo(context.PackageName, 0).VersionName;
#pragma warning restore CS0618 // Type or member is obsolete            }
            }
        }

        public ulong GetDeviceMemoryInBytes()
        {
            try
            {
                var activityManager = Android.App.Application.Context.GetSystemService(Activity.ActivityService) as ActivityManager;
                var memoryInfo = new ActivityManager.MemoryInfo();
                activityManager.GetMemoryInfo(memoryInfo);

                long totalRam = memoryInfo.TotalMem;

                return (ulong)totalRam;
            }
            catch
            {
                return 0;
            }
        }

        public ulong GetDeviceFreeDiskSpaceInBytes()
        {
            try
            {
                //Using StatFS
                var path = new StatFs(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData));
                long blockSize = path.BlockSizeLong;
                long avaliableBlocks = path.AvailableBlocksLong;
                long freeSpace = blockSize * avaliableBlocks;
                return (ulong)freeSpace;
            }
            catch
            {
                return 0;
            }
        }

        public ulong GetDeviceTotalDiskSpaceInBytes()
        {
            try
            {
                //Using StatFS
                var path = new StatFs(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData));
                long freeSpace = path.TotalBytes;
                return (ulong)freeSpace;
            }
            catch
            {
                return 0;
            }
        }

        public void CloseApplication()
        {
            RevertAnimatorDuration(true);
            MainActivity.CurrentMainActivity.Finish();
        }

        public void SetStatusBarHidden(bool ishidden)
        {

        }

        public bool GetStatusBarHidden()
        {
            return true;
        }

        public float GetAnimatorDurationScaleSetting()
        {
            var resolver = Android.App.Application.Context.ContentResolver;
            var scaleName = Android.Provider.Settings.Global.AnimatorDurationScale;
            float scale = Android.Provider.Settings.Global.GetFloat(resolver, scaleName, 1.0f);
            return scale;
        }

        public float GetTransitionAnimationScaleSetting()
        {
            var resolver = Android.App.Application.Context.ContentResolver;
            var scaleName = Android.Provider.Settings.Global.TransitionAnimationScale;
            float scale = Android.Provider.Settings.Global.GetFloat(resolver, scaleName, 1.0f);
            return scale;
        }

        public float GetWindowAnimationScaleSetting()
        {
            var resolver = Android.App.Application.Context.ContentResolver;
            var scaleName = Android.Provider.Settings.Global.WindowAnimationScale;
            float scale = Android.Provider.Settings.Global.GetFloat(resolver, scaleName, 1.0f);
            return scale;
        }

        public bool IsRemoveAnimationsOn()
        {
            var scale1 = GetAnimatorDurationScaleSetting();
            var scale2 = GetTransitionAnimationScaleSetting();
            var scale3 = GetWindowAnimationScaleSetting();

            return scale1 == 0 && scale2 == 0 && scale3 == 0;
        }

        public float GetCurrentAnimatorDurationScale()
        {
            float scale = -1.0f;
            try
            {
                var classForName = JNIEnv.FindClass("android/animation/ValueAnimator");
                var methodId = JNIEnv.GetStaticMethodID(classForName, "getDurationScale", "()F");

                scale = JNIEnv.CallStaticFloatMethod(classForName, methodId);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
            return scale;
        }

        private bool _originalSet = false;
        private float _originalAnimationDurationScale = 1.0f;
        public void OverrideAnimatorDuration()
        {
            float scale = GetAnimatorDurationScaleSetting();

            if (scale == 1.0f)
                return;

            if (!_originalSet)
            {
                _originalAnimationDurationScale = scale;
                _originalSet = true;
            }

            try
            {
                var classForName = JNIEnv.FindClass("android/animation/ValueAnimator");
                var methodId = JNIEnv.GetStaticMethodID(classForName, "setDurationScale", "(F)V");

                JNIEnv.CallStaticVoidMethod(classForName, methodId, new JValue(1f));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }
        public void RevertAnimatorDuration(bool isfinal)
        {
            if (_originalSet && _originalAnimationDurationScale != 1.0f)
            {
                try
                {
                    var classForName = JNIEnv.FindClass("android/animation/ValueAnimator");
                    var methodId = JNIEnv.GetStaticMethodID(classForName, "setDurationScale", "(F)V");

                    float usedValue = !isfinal && _originalAnimationDurationScale == 0.0f ? 0.1f : _originalAnimationDurationScale; //Make sure that the value is not set to zero upon sleep just in case, since that seems to create problems
                    JNIEnv.CallStaticVoidMethod(classForName, methodId, new JValue(usedValue));
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }

                _originalSet = false;
                _originalAnimationDurationScale = 1.0f;
            }
        }


        IReviewManager _manager;
        TaskCompletionSource<bool> _tcs;
        TaskCompletionSource<bool> _tcs2 = null;
        public readonly object LogLock = new object();
        public List<string> Log = new List<string>();
        public TaskCompletionSource<bool> Tcs2 { get { return _tcs2; } set { _tcs2 = value; } }

        public async void RequestAppReview(ContentPage page)
        {
            lock(LogLock)
            {
                Log.Clear();
                Log.Add("Starting App Review");
            }

            _tcs?.TrySetCanceled();
            _tcs = new TaskCompletionSource<bool>();
            _manager = ReviewManagerFactory.Create(MainActivity.CurrentMainActivity);

            var request = _manager.RequestReviewFlow();
            var listener = new StoreReviewTaskCompleteListener(_manager, _tcs, this, false);
            request.AddOnCompleteListener(listener);

            lock (LogLock)
            {
                Log.Add("Awaiting");
            }
            await _tcs.Task;
            if (_tcs2 != null)
                await _tcs2.Task;
            _manager.Dispose();
            request.Dispose();
            lock (LogLock)
            {
                Log.Add("Done");
            }

            string logs = "";
            lock (LogLock)
            {
                foreach (var log in Log)
                {
                    if (logs != "")
                        logs += ". ";
                    logs += log;
                }
                Log.Clear();
            }
            if (App.DebugLogMessages)
                await page.DisplayAlert("App Review Log", logs, "OK");
        }

        public string GetBaseUrl()
        {
            return "file:///android_asset/";
        }
        public string GetAssetsPath()
        {

            return "file:///android_asset/";
        }

        public string GetAbsoluteOnDemandAssetPath(string assetPack)
        {
            if (MainActivity.CurrentMainActivity == null || MainActivity.CurrentMainActivity.AssetPackManager == null)
                return null;

            var assetPackPath = MainActivity.CurrentMainActivity.AssetPackManager.GetPackLocation(assetPack);
            return assetPackPath?.AssetsPath() ?? null;
        }

        public string GetAbsoluteOnDemandAssetPath(string assetPack, string relativeAssetPath)
        {
            if (MainActivity.CurrentMainActivity == null || MainActivity.CurrentMainActivity.AssetPackManager == null)
                return null;

            string assetsFolderPath = GetAbsoluteOnDemandAssetPath(assetPack);
            if (assetsFolderPath == null)
            {
                // asset pack is not ready
                return null;
            }

            string assetPath = Path.Combine(assetsFolderPath, relativeAssetPath);
            return assetPath;
        }

        public int FetchOnDemandPack(string pack)
        {
            if (MainActivity.CurrentMainActivity == null || MainActivity.CurrentMainActivity.AssetPackManager == null)
                return 2; /* No asset pack manager */

            var location = MainActivity.CurrentMainActivity.AssetPackManager.GetPackLocation(pack);
            if (location == null)
            {
                // TODO Figure out how to use the GetPackStates.
                try
                {
                    var states = MainActivity.CurrentMainActivity.AssetPackManager.GetPackStates(new string[] { pack });
                    // TODO add OnComplete Listeners to the Task returned by Fetch.
                    MainActivity.CurrentMainActivity.AssetPackManager.Fetch(new string[] { pack });
                    return 0;  /* Success */
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                    return 1;  /* Exception occurred */
                }
            }
            else
                return -1; /* Already loaded */
        }

        public event EventHandler<AssetPackStatusEventArgs> OnDemandPackStatusNotification;

        public void InitOnDemandPackStatusNotificationEventHandler()
        {
            MainActivity.CurrentMainActivity.OnDemandPackStatus += OnDemandPackStatusNotified;
        }

        private void OnDemandPackStatusNotified(object sender, AssetPackStatusEventArgs e)
        {
            OnDemandPackStatusNotification?.Invoke(this, e);
        }
    }


    public class StoreReviewTaskCompleteListener : Java.Lang.Object, IOnCompleteListener
    {
        IReviewManager _manager;
        TaskCompletionSource<bool> _tcs;
        PlatformService _ps;
        bool _isLaunchReviewFlow = false;

        public StoreReviewTaskCompleteListener(IReviewManager manager, TaskCompletionSource<bool> tcs, PlatformService ps, bool isLaunchReviewFlow)
        {
            _manager = manager;
            _tcs = tcs;
            _ps = ps;
            _isLaunchReviewFlow = isLaunchReviewFlow;
        }

        Xamarin.Google.Android.Play.Core.Tasks.Task launchTask;

        public void OnComplete(Xamarin.Google.Android.Play.Core.Tasks.Task task)
        {
            if (_isLaunchReviewFlow)
            {

                lock (_ps.LogLock)
                {
                    _ps.Log.Add("OnComplete / _isLaunchReviewFlow / isSuccessful: " + task.IsSuccessful.ToString());
                }
                if (!task.IsSuccessful)
                {
                    _ps.Tcs2 = null;
                }
                _tcs?.TrySetResult(task.IsSuccessful);
                launchTask?.Dispose();
            }
            else
            {
                lock (_ps.LogLock)
                {
                    _ps.Log.Add("OnComplete / normal / isSuccessful: " + task.IsSuccessful.ToString());
                }
                if (task.IsSuccessful)
                {
                    //Launch review flow
                    try
                    {
                        if (_ps.Tcs2 != null)
                            _ps.Tcs2.TrySetCanceled();
                        else
                            _ps.Tcs2 = new TaskCompletionSource<bool>();
                        var reviewInfo = (ReviewInfo)task.GetResult(Java.Lang.Class.FromType(typeof(ReviewInfo)));
                        launchTask = _manager.LaunchReviewFlow(MainActivity.CurrentMainActivity, reviewInfo);
                        launchTask.AddOnCompleteListener(new StoreReviewTaskCompleteListener(_manager, _ps.Tcs2, _ps, true));
                        _tcs?.TrySetResult(true);
                        lock (_ps.LogLock)
                        {
                            _ps.Log.Add("OnComplete / normal / Finished");
                        }
                    }
                    catch (Exception ex)
                    {
                        _ps.Tcs2 = null;
                        _tcs?.TrySetResult(false);
                        System.Diagnostics.Debug.WriteLine(ex.Message);
                        lock (_ps.LogLock)
                        {
                            _ps.Log.Add("OnComplete: Exception: " + ex.Message);
                        }
                    }
                }
                else
                {
                    _ps.Tcs2 = null;
                    _tcs?.TrySetResult(false);
                }
            }
        }
    }
}