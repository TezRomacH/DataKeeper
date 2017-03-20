// DataKeeper v1.4
// created by TezRomacH

using System;
using System.Collections.Generic;

namespace DataKeeper
{
    [Flags]
    public enum TriggerKind
    {
        Before = 1,
        After = 2
    }

    public sealed class Data
    {
        private class DataInfo
        {
            public object Value { get; set; }
            public List<Action> TriggersBefore { get; set; }
            public List<Action> TriggersAfter { get; set; }

            public void AddTriggers(Action action, TriggerKind kind)
            {
                if (kind.TriggerHasFlag(TriggerKind.Before))
                {
                    if (TriggersBefore == null)
                        TriggersBefore = new List<Action>();

                    TriggersBefore.Add(action);
                }

                if (kind.TriggerHasFlag(TriggerKind.After))
                {
                    if (TriggersAfter == null)
                        TriggersAfter = new List<Action>();

                    TriggersAfter.Add(action);
                }
            }

            public void RemoveTriggers(TriggerKind kind)
            {
                if (kind.TriggerHasFlag(TriggerKind.Before))
                    TriggersBefore = null;

                if (kind.TriggerHasFlag(TriggerKind.After))
                    TriggersAfter = null;
            }
        }

        private Dictionary<string, DataInfo> data;

        private static volatile Data instance;

        private Data()
        {
            data = new Dictionary<string, DataInfo>();
        }

        private static object syncRoot = new object();

        public static Data Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new Data();
                    }
                }
                return instance;
            }
        }

        #region binding methods

        /// <summary>
        /// Связывает выполнение действия action после изменения значения данных ко ключу key
        /// в зависимости от значения triggerKind
        /// </summary>
        public void BindChangeField(string key, Action action)
        {
            BindChangeField(key, action, TriggerKind.After);
        }

        /// <summary>
        /// Связывает выполнение действия action до или после изменения значения данных ко ключу key
        /// в зависимости от значения triggerKind
        /// </summary>
        public void BindChangeField(string key, Action action, TriggerKind triggerKind)
        {
            DataInfo info = null;
            if (data.TryGetValue(key, out info))
            {
                info.AddTriggers(action, triggerKind);
                return;
            }

            info = new DataInfo { Value = null };

            info.AddTriggers(action, triggerKind);
            data[key] = info;
        }

        /// <summary>
        /// Удаляет все связанные действия по ключу key
        /// </summary>
        /// <param name="key">Ключ связки</param>
        public void Unbind(string key)
        {
            Unbind(key, TriggerKind.Before | TriggerKind.After);
        }

        /// <summary>
        /// Удаляет все связанные действия по ключу key
        /// </summary>
        /// <param name="key">Ключ связки</param>
        public void Unbind(string key, TriggerKind triggerKind)
        {
            DataInfo info = null;
            if (data.TryGetValue(key, out info))
            {
                info.RemoveTriggers(triggerKind);
            }
        }

        #endregion

        #region memory methods

        /// <summary>
        /// Записывает или изменяет значение по ключу key на value
        /// </summary>
        /// <param name="key">Ключ</param>
        /// <param name="value">Объект, которые записывается или изменяется</param>
        public void Set(string key, object value)
        {
            DataInfo info = null;
            if (data.TryGetValue(key, out info))
            {
                info.TriggersBefore?.InvokeAll();
                info.Value = value;
                info.TriggersAfter?.InvokeAll();
                return;
            }

            info = new DataInfo { Value = value };
            data[key] = info;
        }

        /// <summary>
        /// Получает объект из данных
        /// </summary>
        /// <param name="key">Ключ</param>
        /// <returns>Объект по ключу или null</returns>
        public object Get(string key)
        {
            DataInfo info = null;
            if (data.TryGetValue(key, out info))
                return info.Value;

            return null;
        }

        /// <summary>
        /// Получает объект из данных типа <c>T</c>
        /// </summary>
        /// <typeparam name="T">Тип возвращаемого значения</typeparam>
        /// <param name="key">Ключ</param>
        /// <param name="default">Значение по умолчание. В случае, если в данных нет объекта по ключу key</param>
        /// <returns>Объект по ключу или @default</returns>
        /// <exception cref="InvalidCastException"></exception>
        public T Get<T>(string key, T @default = default(T))
        {
            DataInfo info = null;
            if (data.TryGetValue(key, out info) && info.Value != null)
            {
                return (T)info.Value;
            }

            return @default;
        }

        /// <summary>
        /// Получает целое число из данных
        /// </summary>
        /// <param name="key">Ключ</param>
        /// <param name="default">Значение по умолчание. В случае, если в данных нет объекта по ключу key</param>
        /// <returns>Объект по ключу или @default</returns>
        /// <exception cref="InvalidCastException"></exception>
        public int GetInt(string key, int @default = 0)
        {
            DataInfo info = null;
            if (data.TryGetValue(key, out info) && info.Value != null)
                return (int)info.Value;

            return @default;
        }

        /// <summary>
        /// Получает число с плавающей точкой из данных
        /// </summary>
        /// <param name="key">Ключ</param>
        /// <param name="default">Значение по умолчание. В случае, если в данных нет объекта по ключу key</param>
        /// <returns>Объект по ключу или @default</returns>
        /// <exception cref="InvalidCastException"></exception>
        public float GetFloat(string key, float @default = 0f)
        {
            DataInfo info = null;
            if (data.TryGetValue(key, out info) && info.Value != null)
                return (float)info.Value;

            return @default;
        }

        /// <summary>
        /// Получает логическое значение из данных
        /// </summary>
        /// <param name="key">Ключ</param>
        /// <param name="default">Значение по умолчание. В случае, если в данных нет объекта по ключу key</param>
        /// <returns>Объект по ключу или @default</returns>
        /// <exception cref="InvalidCastException"></exception>
        public bool GetBool(string key, bool @default = false)
        {
            DataInfo info = null;
            if (data.TryGetValue(key, out info) && info.Value != null)
                return (bool)info.Value;

            return @default;
        }

        /// <summary>
        /// Возвращает строку из данных
        /// </summary>
        /// <param name="key">Ключ</param>
        /// <param name="default">Значение по умолчание. В случае, если в данных нет объекта по ключу key</param>
        /// <returns>Строку, представляющий объект по ключу или @default</returns>
        public string GetString(string key, string @default = "")
        {
            DataInfo info = null;
            if (data.TryGetValue(key, out info) && info.Value != null)
                return info.Value.ToString();

            return @default;
        }

        public Type GetValueType(string key)
        {
            DataInfo info = null;
            return data.TryGetValue(key, out info) && info.Value != null ? info.Value.GetType() : (Type)null;
        }

        public void Increase(string key, int valueToIncrease = 1)
        {
            if (GetValueType(key) == typeof(int))
            {
                var obj = GetInt(key);
                Set(key, obj + valueToIncrease);
            }
        }

        public void Increase(string key, float valueToIncrease = 1f)
        {
            if (GetValueType(key) == typeof(float))
            {
                var obj = GetFloat(key);
                Set(key, obj + valueToIncrease);
            }
        }

        public void Increase(string key, double valueToIncrease = 1.0)
        {
            if (GetValueType(key) == typeof(double))
            {
                var obj = Get<double>(key);
                Set(key, obj + valueToIncrease);
            }
        }

        public void Decrease(string key, int valueToDecrease = 1)
        {
            if (GetValueType(key) == typeof(int))
            {
                var obj = GetInt(key);
                Set(key, obj - valueToDecrease);
            }
        }

        public void Decrease(string key, float valueToDecrease = 1f)
        {
            if (GetValueType(key) == typeof(float))
            {
                var obj = GetFloat(key);
                Set(key, obj - valueToDecrease);
            }
        }

        public void Decrease(string key, double valueToDecrease = 1.0)
        {
            if (GetValueType(key) == typeof(double))
            {
                var obj = Get<double>(key);
                Set(key, obj - valueToDecrease);
            }
        }

        #endregion
    }

    internal static class Extentions
    {
        public static bool TriggerHasFlag(this TriggerKind triggerKind, TriggerKind flag)
        {
            // Faster than native Enum.HasFlag
            return (triggerKind & flag) == flag;
        }

        public static void InvokeAll(this IEnumerable<Action> actions)
        {
            if (actions == null)
                return;

            foreach (var action in actions)
            {
                action?.Invoke();
            }
        }
    }
}
