// DataKeeper v1.5
// created by TezRomacH
// github.com/TezRomacH/DataKeeper

using System;
using System.Collections.Generic;

namespace DataKeeper
{
    public sealed class Data
    {
        private readonly Dictionary<string, DataInfo> data;

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
        /// Связывает выполнение действия action до или после изменения значения данных ко ключу key
        /// в зависимости от значения triggerType
        /// </summary>
        public void BindChangeField(string key, Action action, TriggerType triggerType = TriggerType.After)
        {
            if (key == null)
                return;

            DataInfo info = null;
            if (data.TryGetValue(key, out info))
            {
                info.AddBindTriggers(action, BindType.OnChange, triggerType);
                return;
            }

            info = new DataInfo { Value = null };

            info.AddBindTriggers(action, BindType.OnChange, triggerType);
            data[key] = info;
        }

        /// <summary>
        /// Связывает выполнение действия action до или после удаления значения данных ко ключу key
        /// в зависимости от значения triggerType
        /// </summary>
        public void BindRemoveField(string key, Action action, TriggerType triggerType = TriggerType.Before)
        {
            if (key == null)
                return;

            DataInfo info = null;
            if (data.TryGetValue(key, out info))
            {
                info.AddBindTriggers(action, BindType.OnRemove, triggerType);
                return;
            }

            info = new DataInfo { Value = null };

            info.AddBindTriggers(action, BindType.OnRemove, triggerType);
            data[key] = info;
        }

        /// <summary>
        /// Удаляет все связанные действия по ключу key
        /// </summary>
        /// <param name="key">Ключ связки</param>
        public void Unbind(string key)
        {
            Unbind(key, BindType.OnAll, TriggerType.Both);
        }
        /// <summary>
        /// Удаляет все связанные действия по ключу key
        /// </summary>
        /// <param name="key">Ключ связки</param>
        public void Unbind(string key, BindType bindType)
        {
            Unbind(key, bindType, TriggerType.Both);
        }

        /// <summary>
        /// Удаляет все связанные действия по ключу key
        /// </summary>
        /// <param name="key">Ключ связки</param>
        public void Unbind(string key, BindType bindType, TriggerType triggerType)
        {
            if (key == null)
                return;

            DataInfo info = null;
            if (data.TryGetValue(key, out info))
            {
                info.RemoveBindTriggers(bindType, triggerType);
            }
        }

        #endregion

        #region memory methods

        /// <summary>
        /// Записывает или изменяет значение по ключу key на value
        /// </summary>
        /// <param name="key">Ключ</param>
        /// <param name="value">Объект, которые записывается или изменяется</param>
        /// <exception cref="ArgumentNullException"></exception>
        public void Set(string key, object value)
        {
            DataInfo info = null;
            if (data.TryGetValue(key, out info))
            {
                info.TriggersBeforeChange?.InvokeAll();
                info.Value = value;
                info.TriggersAfterChange?.InvokeAll();
                return;
            }

            info = new DataInfo { Value = value };
            data[key] = info;
        }

        #region getters

        /// <summary>
        /// Получает объект из данных
        /// </summary>
        /// <param name="key">Ключ</param>
        /// <returns>Объект по ключу или исключение</returns>
        /// <exception cref="KeyNotFoundException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public object Get(string key)
        {
            DataInfo info = null;
            if (data.TryGetValue(key, out info))
                return info.Value;

            throw new KeyNotFoundException($"Key \'{key}\' can't be found!");
        }

        /// <summary>
        /// Получает объект из данных
        /// </summary>
        /// <param name="key">Ключ</param>
        /// <returns>Объект по ключу или @default</returns>
        public object Get(string key, object @default)
        {
            DataInfo info = null;
            if (key != null && data.TryGetValue(key, out info))
                return info.Value;

            return @default;
        }

        /// <summary>
        /// Получает объект из данных типа <c>T</c>
        /// </summary>
        /// <typeparam name="T">Тип возвращаемого значения</typeparam>
        /// <param name="key">Ключ</param>
        /// <returns>Объект по ключу или исключение</returns>
        /// <exception cref="KeyNotFoundException"></exception>
        /// <exception cref="InvalidCastException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public T Get<T>(string key)
        {
            DataInfo info = null;
            if (data.TryGetValue(key, out info) && info.Value != null)
            {
                return (T)info.Value;
            }

            throw new KeyNotFoundException($"Key \'{key}\' can't be found!");
        }

        /// <summary>
        /// Получает объект из данных типа <c>T</c>
        /// </summary>
        /// <typeparam name="T">Тип возвращаемого значения</typeparam>
        /// <param name="key">Ключ</param>
        /// <param name="default">Значение по умолчание. В случае, если в данных нет объекта по ключу key</param>
        /// <returns>Объект по ключу или @default</returns>
        /// <exception cref="InvalidCastException"></exception>
        public T Get<T>(string key, T @default)
        {
            DataInfo info = null;
            if (key != null && data.TryGetValue(key, out info) && info.Value != null)
            {
                return (T)info.Value;
            }

            return @default;
        }

        /// <summary>
        /// Получает целое число из данных
        /// </summary>
        /// <param name="key">Ключ</param>
        /// <returns>Объект по ключу или исключение</returns>
        /// <exception cref="KeyNotFoundException"></exception>
        /// <exception cref="InvalidCastException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public int GetInt(string key)
        {
            DataInfo info = null;
            if (data.TryGetValue(key, out info) && info.Value != null)
                return (int)info.Value;

            throw new KeyNotFoundException($"Key \'{key}\' can't be found!");
        }

        /// <summary>
        /// Получает целое число из данных
        /// </summary>
        /// <param name="key">Ключ</param>
        /// <param name="default">Значение по умолчание. В случае, если в данных нет объекта по ключу key</param>
        /// <returns>Объект по ключу или @default</returns>
        /// <exception cref="InvalidCastException"></exception>
        public int GetInt(string key, int @default)
        {
            DataInfo info = null;
            if (key != null && data.TryGetValue(key, out info) && info.Value != null)
                return (int)info.Value;

            return @default;
        }

        /// <summary>
        /// Получает число с плавающей точкой из данных
        /// </summary>
        /// <param name="key">Ключ</param>
        /// <returns>Объект по ключу или исключение</returns>
        /// <exception cref="KeyNotFoundException"></exception>
        /// <exception cref="InvalidCastException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public float GetFloat(string key)
        {
            DataInfo info = null;
            if (data.TryGetValue(key, out info) && info.Value != null)
                return (float)info.Value;

            throw new KeyNotFoundException($"Key \'{key}\' can't be found!");
        }

        /// <summary>
        /// Получает число с плавающей точкой из данных
        /// </summary>
        /// <param name="key">Ключ</param>
        /// <param name="default">Значение по умолчание. В случае, если в данных нет объекта по ключу key</param>
        /// <returns>Объект по ключу или @default</returns>
        /// <exception cref="InvalidCastException"></exception>
        public float GetFloat(string key, float @default)
        {
            DataInfo info = null;
            if (key != null && data.TryGetValue(key, out info) && info.Value != null)
                return (float)info.Value;

            return @default;
        }

        /// <summary>
        /// Получает число с плавающей точкой из данных
        /// </summary>
        /// <param name="key">Ключ</param>
        /// <returns>Объект по ключу или исключение</returns>
        /// <exception cref="KeyNotFoundException"></exception>
        /// <exception cref="InvalidCastException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public double GetDouble(string key)
        {
            DataInfo info = null;
            if (data.TryGetValue(key, out info) && info.Value != null)
                return (double)info.Value;

            throw new KeyNotFoundException($"Key \'{key}\' can't be found!");
        }

        /// <summary>
        /// Получает число с плавающей точкой из данных
        /// </summary>
        /// <param name="key">Ключ</param>
        /// <param name="default">Значение по умолчание. В случае, если в данных нет объекта по ключу key</param>
        /// <returns>Объект по ключу или @default</returns>
        /// <exception cref="InvalidCastException"></exception>
        public double GetDouble(string key, double @default)
        {
            DataInfo info = null;
            if (key != null && data.TryGetValue(key, out info) && info.Value != null)
                return (double)info.Value;

            return @default;
        }

        /// <summary>
        /// Получает логическое значение из данных
        /// </summary>
        /// <param name="key">Ключ</param>
        /// <returns>Объект по ключу или исключение</returns>
        /// <exception cref="InvalidCastException"></exception>
        /// <exception cref="KeyNotFoundException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public bool GetBool(string key)
        {
            DataInfo info = null;
            if (data.TryGetValue(key, out info) && info.Value != null)
                return (bool)info.Value;

            throw new KeyNotFoundException($"Key \'{key}\' can't be found!");
        }

        /// <summary>
        /// Получает логическое значение из данных
        /// </summary>
        /// <param name="key">Ключ</param>
        /// <param name="default">Значение по умолчание. В случае, если в данных нет объекта по ключу key</param>
        /// <returns>Объект по ключу или @default</returns>
        /// <exception cref="InvalidCastException"></exception>
        public bool GetBool(string key, bool @default)
        {
            DataInfo info = null;
            if (key != null && data.TryGetValue(key, out info) && info.Value != null)
                return (bool)info.Value;

            return @default;
        }

        /// <summary>
        /// Возвращает строку из данных.
        /// Безопасен. Не выбрасывает исключений
        /// </summary>
        /// <param name="key">Ключ</param>
        /// <param name="default">Значение по умолчание. В случае, если в данных нет объекта по ключу key</param>
        /// <returns>Строку, представляющий объект по ключу или @default</returns>
        public string GetString(string key, string @default = "")
        {
            DataInfo info = null;
            if (key != null && data.TryGetValue(key, out info) && info.Value != null)
                return info.Value.ToString();

            return @default;
        }

        /// <summary>
        /// Возвращает <see cref="Type"></see> объекта из данных по ключу.
        /// Безопасен. Не выбрасывает исключений
        /// </summary>
        /// <param name="key">Ключ</param>
        public Type GetValueType(string key)
        {
            DataInfo info = null;
            if (key != null && data.TryGetValue(key, out info) && info.Value != null)
                return info.Value.GetType();

            return (Type)null;
        }

        #endregion

        #region increase & decrease

        public void Increase(string key, int valueToIncrease = 1)
        {
            var type = GetValueType(key);
            if (type == null || type == typeof(int))
            {
                var obj = GetInt(key, default(int));
                Set(key, obj + valueToIncrease);
            }
        }

        public void Increase(string key, float valueToIncrease = 1f)
        {
            var type = GetValueType(key);
            if (type == null || type == typeof(float))
            {
                var obj = GetFloat(key, default(float));
                Set(key, obj + valueToIncrease);
            }
        }

        public void Increase(string key, double valueToIncrease = 1.0)
        {
            var type = GetValueType(key);
            if (type == null || type == typeof(double))
            {
                var obj = GetDouble(key, default(double));
                Set(key, obj + valueToIncrease);
            }
        }

        public void Decrease(string key, int valueToDecrease = 1)
        {
            var type = GetValueType(key);
            if (type == null || type == typeof(int))
            {
                var obj = GetInt(key, default(int));
                Set(key, obj - valueToDecrease);
            }
        }

        public void Decrease(string key, float valueToDecrease = 1f)
        {
            var type = GetValueType(key);
            if (type == null || type == typeof(float))
            {
                var obj = GetFloat(key, default(float));
                Set(key, obj - valueToDecrease);
            }
        }

        public void Decrease(string key, double valueToDecrease = 1.0)
        {
            var type = GetValueType(key);
            if (type == null || type == typeof(double))
            {
                var obj = GetDouble(key, default(double));
                Set(key, obj - valueToDecrease);
            }
        }

        #endregion

        /// <summary>
        /// Возвращает количество данных в модели
        /// </summary>
        public int Count => data.Count;

        /// <summary>
        /// Уничтожает все данные в модели
        /// </summary>
        public void Clear()
        {
            data.Clear();
        }

        /// <summary>
        /// Проверяет, содержится ли в модели данные по ключу key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool ContainsKey(string key)
        {
            if (key == null)
                return false;

            return data.ContainsKey(key);
        }

        /// <summary>
        /// Позволяет получить или записать даныне в модель
        /// </summary>
        /// <param name="key"></param>
        /// <exception cref="KeyNotFoundException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public object this[string key]
        {
            get { return this.Get(key); }
            set { this.Set(key, value); }
        }

        /// <summary>
        /// Удаляет данные по ключу key
        /// </summary>
        /// <param name="key"></param>
        public void Remove(string key)
        {
            DataInfo info = null;
            if (ContainsKey(key) && data.TryGetValue(key, out info))
            {
                info.TriggersBeforeRemove?.InvokeAll();
                data.Remove(key);
                info.TriggersAfterRemove?.InvokeAll();
            }

        }

        #endregion

        private class DataInfo
        {
            public object Value { get; set; }
            public ICollection<Action> TriggersBeforeChange { get; private set; }
            public ICollection<Action> TriggersAfterChange { get; private set; }

            public ICollection<Action> TriggersBeforeRemove { get; private set; }
            public ICollection<Action> TriggersAfterRemove { get; private set; }

            public void AddBindTriggers(Action action, BindType bindType, TriggerType type)
            {
                if (bindType.BindTypeHasFlag(BindType.OnChange))
                {
                    if (type.TriggerHasFlag(TriggerType.Before))
                    {
                        if (TriggersBeforeChange == null)
                            TriggersBeforeChange = new List<Action>();

                        TriggersBeforeChange.Add(action);
                    }

                    if (type.TriggerHasFlag(TriggerType.After))
                    {
                        if (TriggersAfterChange == null)
                            TriggersAfterChange = new List<Action>();

                        TriggersAfterChange.Add(action);
                    }
                }

                if (bindType.BindTypeHasFlag(BindType.OnRemove))
                {
                    if (type.TriggerHasFlag(TriggerType.Before))
                    {
                        if (TriggersBeforeRemove == null)
                            TriggersBeforeRemove = new List<Action>();

                        TriggersBeforeRemove.Add(action);
                    }

                    if (type.TriggerHasFlag(TriggerType.After))
                    {
                        if (TriggersAfterRemove == null)
                            TriggersAfterRemove = new List<Action>();

                        TriggersAfterRemove.Add(action);
                    }
                }
            }

            public void RemoveBindTriggers(BindType bindType, TriggerType type)
            {
                if (bindType.BindTypeHasFlag(BindType.OnChange))
                {
                    if (type.TriggerHasFlag(TriggerType.Before))
                        TriggersBeforeChange = null;

                    if (type.TriggerHasFlag(TriggerType.After))
                        TriggersAfterChange = null;
                }

                if (bindType.BindTypeHasFlag(BindType.OnRemove))
                {
                    if (type.TriggerHasFlag(TriggerType.Before))
                        TriggersBeforeRemove = null;

                    if (type.TriggerHasFlag(TriggerType.After))
                        TriggersAfterRemove = null;
                }
            }
        }
    }

    #region enums

    [Flags]
    public enum TriggerType
    {
        Before = 1,
        After = 2,
        Both = Before | After
    }

    [Flags]
    public enum BindType
    {
        OnChange = 1,
        OnRemove = 2,
        OnAll = OnChange | OnRemove
    }

    #endregion

    internal static class Extentions
    {
        public static bool TriggerHasFlag(this TriggerType triggerType, TriggerType flag)
        {
            // Faster than native Enum.HasFlag
            return (triggerType & flag) == flag;
        }
        public static bool BindTypeHasFlag(this BindType bindType, BindType flag)
        {
            return (bindType & flag) == flag;
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
