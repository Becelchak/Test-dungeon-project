namespace EventBusSystem
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    public static class EventBus
    {
        private static readonly Dictionary<Type, SubscribersList<IGlobalSubscriber>> s_Subscribers
            = new Dictionary<Type, SubscribersList<IGlobalSubscriber>>();

        private static readonly object s_Lock = new object();

        public static void Subscribe(IGlobalSubscriber subscriber)
        {
            lock (s_Lock)
            {
                List<Type> subscriberTypes = EventBusHelper.GetSubscriberTypes(subscriber);
                foreach (Type t in subscriberTypes)
                {
                    if (!s_Subscribers.ContainsKey(t))
                    {
                        s_Subscribers[t] = new SubscribersList<IGlobalSubscriber>();
                    }
                    s_Subscribers[t].Add(subscriber);
                }
            }
        }

        public static void Unsubscribe(IGlobalSubscriber subscriber)
        {
            lock (s_Lock)
            {
                List<Type> subscriberTypes = EventBusHelper.GetSubscriberTypes(subscriber);
                foreach (Type t in subscriberTypes)
                {
                    if (s_Subscribers.ContainsKey(t))
                    {
                        s_Subscribers[t].Remove(subscriber);
                    }
                }
            }
        }

        public static void RaiseEvent<TSubscriber>(Action<TSubscriber> action)
            where TSubscriber : class, IGlobalSubscriber
        {
            SubscribersList<IGlobalSubscriber> subscribers;
            lock (s_Lock)
            {
                if (!s_Subscribers.TryGetValue(typeof(TSubscriber), out subscribers))
                {
                    Debug.LogWarning($"[EventBus] Нет подписчиков для типа {typeof(TSubscriber).Name}");
                    return;
                }
            }

            Debug.Log($"[EventBus] Вызов события для {typeof(TSubscriber).Name}, подписчиков: {subscribers.List.Count}");

            subscribers.Executing = true;
            foreach (IGlobalSubscriber subscriber in subscribers.List.ToList())
            {
                try
                {
                    action.Invoke(subscriber as TSubscriber);
                }
                catch (Exception e)
                {
                    Debug.LogError($"EventBus exception: {e}");
                }
            }
            subscribers.Executing = false;
            subscribers.Cleanup();
        }
    }
}