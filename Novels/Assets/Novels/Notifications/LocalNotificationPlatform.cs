using System;
using UnityEngine;
#if UNITY_ANDROID
using Unity.Notifications.Android;
#elif UNITY_IOS
using Unity.Notifications.iOS;
#endif

namespace Novels.Notifications
{
    internal sealed class LocalNotificationPlatform
    {
        private const string _channelId = "novels-reminders-v1";
#if UNITY_ANDROID
        private PermissionRequest _permissionRequest;
#elif UNITY_IOS
        private AuthorizationRequest _authorizationRequest;
#endif

        internal void Initialize()
        {
#if UNITY_ANDROID
            AndroidNotificationCenter.Initialize();
            AndroidNotificationCenter.RegisterNotificationChannel(
                new AndroidNotificationChannel
                {
                    Id = _channelId,
                    Name = "Напоминания",
                    Description = "Продолжение историй и новые публикации",
                    Importance = Importance.Default,
                });
            _permissionRequest ??= new PermissionRequest();
#elif UNITY_IOS
            _authorizationRequest ??= new AuthorizationRequest(
                AuthorizationOption.Alert | AuthorizationOption.Sound,
                false);
#endif
        }

        internal void Schedule(
            string id,
            string title,
            string body,
            DateTime fireTime,
            string payload)
        {
            if (string.IsNullOrWhiteSpace(id) || fireTime <= DateTime.Now)
                return;
#if UNITY_ANDROID
            var notification = new AndroidNotification
            {
                Title = title,
                Text = body,
                FireTime = fireTime,
                IntentData = payload,
                ShouldAutoCancel = true,
            };
            AndroidNotificationCenter.SendNotificationWithExplicitID(
                notification,
                _channelId,
                StableId(id));
#elif UNITY_IOS
            var delay = fireTime - DateTime.Now;
            if (delay < TimeSpan.FromSeconds(1))
                return;
            iOSNotificationCenter.ScheduleNotification(new iOSNotification
            {
                Identifier = id,
                Title = title,
                Body = body,
                Data = payload,
                ShowInForeground = false,
                Trigger = new iOSNotificationTimeIntervalTrigger
                {
                    TimeInterval = delay,
                    Repeats = false,
                },
            });
#endif
        }

        internal void Cancel(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return;
#if UNITY_ANDROID
            AndroidNotificationCenter.CancelNotification(StableId(id));
#elif UNITY_IOS
            iOSNotificationCenter.RemoveScheduledNotification(id);
            iOSNotificationCenter.RemoveDeliveredNotification(id);
#endif
        }

        internal string GetLaunchPayload()
        {
#if UNITY_ANDROID
            return AndroidNotificationCenter.GetLastNotificationIntent()?.Notification.IntentData;
#elif UNITY_IOS
            return iOSNotificationCenter.GetLastRespondedNotification()?.Data;
#else
            return null;
#endif
        }

        private static int StableId(string value)
        {
            unchecked
            {
                var hash = 2166136261u;
                foreach (var character in value)
                {
                    hash ^= character;
                    hash *= 16777619u;
                }
                return (int)(hash & 0x7fffffff);
            }
        }
    }
}
