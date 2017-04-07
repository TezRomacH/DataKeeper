// DataKeeper v1.7
// created by TezRomacH
// https://github.com/TezRomacH/DataKeeper

using System;
using System.Collections.Generic;

namespace DataKeeper
{
    public sealed partial class Data
    {
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

        /// <summary>
        /// Перечислимая коллекция ключей в модели
        /// </summary>
        public IReadOnlyCollection<string> Keys
        {
            get
            {
                var set = new SortedSet<string>(data.Keys);
                set.ExceptWith(removedKeys);
                return (IReadOnlyCollection<string>) set;
            }
        }

        #region binding methods

        /// <summary>
        /// Связывает выполнение действия action до или после изменения значения данных ко ключу key
        /// в зависимости от значения triggerType
        /// </summary>
        public void BindChangeField(string key, Action action, TriggerType triggerType = TriggerType.After)
        {
            BindField(key, action, BindType.OnChange, triggerType);
        }

        /// <summary>
        /// Связывает выполнение действия action до или после удаления значения данных ко ключу key
        /// в зависимости от значения triggerType
        /// </summary>
        public void BindRemoveField(string key, Action action, TriggerType triggerType = TriggerType.Before)
        {
            BindField(key, action, BindType.OnRemove, triggerType);
        }

        /// <summary>
        /// Позволяет связать ключ key с некоторым действием <see cref="Action"/>.
        /// Смотрите также <seealso cref="BindChangeField"/> и <seealso cref="BindRemoveField"/>
        /// </summary>
        public Action this[string key, BindType bindType, TriggerType triggerType]
        {
            set { BindField(key, value, bindType, triggerType); }
        }

        private void BindField(string key, Action action, BindType bindType, TriggerType triggerType)
        {
            if (key == null)
                return;

            DataInfo info = null;
            if (data.TryGetValue(key, out info))
            {
                info.AddBindTriggers(action, bindType, triggerType);
                return;
            }

            info = new DataInfo { Value = null };

            info.AddBindTriggers(action, bindType, triggerType);
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
                if (IsKeyRemoved(key) && !info.HasAnyTrigger())
                {
                    data.Remove(key);
                    removedKeys.Remove(key);
                }
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
            if (key == null) return;

            DataInfo info = null;
            if (data.TryGetValue(key, out info))
            {
                info.TriggersBeforeChange?.InvokeAll();
                info.Value = value;
                removedKeys.Remove(key);
                info.TriggersAfterChange?.InvokeAll();
                return;
            }

            info = new DataInfo { Value = value };
            data[key] = info;
        }

        /// <summary>
        /// Удаляет данные по ключу key
        /// </summary>
        /// <param name="key"></param>
        public void Remove(string key)
        {
            DataInfo info = null;
            if (key != null && !IsKeyRemoved(key) && data.TryGetValue(key, out info))
            {
                if (!info.HasAnyTrigger())
                {
                    data.Remove(key);
                    return;
                }

                info.TriggersBeforeRemove?.InvokeAll();
                info.Value = null;
                removedKeys.Add(key);
                info.TriggersAfterRemove?.InvokeAll();
            }
        }

        /// <summary>
        /// Позволяет получить или записать данные в модель
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
        /// Позволяет получить данные из модели.
        /// В случае, если данных нет, то вернется второй параметр
        /// </summary>
        /// <param name="key"></param>
        public object this[string key, object @default]
        {
            get { return this.Get<object>(key, @default); }
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
            if (!IsKeyRemoved(key) && data.TryGetValue(key, out info))
                return info.Value;

            throw new KeyNotFoundException($"Key \'{key}\' can't be found!");
        }

        /// <summary>
        /// Получает объект из данных типа <see cref="T"/>
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
            if (!IsKeyRemoved(key) && data.TryGetValue(key, out info))
            {
                return (T)info.Value;
            }

            throw new KeyNotFoundException($"Key \'{key}\' can't be found!");
        }

        /// <summary>
        /// Получает объект из данных типа <see cref="T"/>
        /// </summary>
        /// <typeparam name="T">Тип возвращаемого значения</typeparam>
        /// <param name="key">Ключ</param>
        /// <param name="default">Значение по умолчание. В случае, если в данных нет объекта по ключу key</param>
        /// <returns>Объект по ключу или @default</returns>
        public T Get<T>(string key, T @default)
        {
            DataInfo info = null;
            if (key != null && !IsKeyRemoved(key) && data.TryGetValue(key, out info))
            {
                try
                {
                    T result = (T)info.Value;
                    return result;
                }
                catch
                {
                    return @default;
                }
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
            if (!IsKeyRemoved(key) && data.TryGetValue(key, out info) && info.Value != null)
                return (int)info.Value;

            throw new KeyNotFoundException($"Key \'{key}\' can't be found!");
        }

        /// <summary>
        /// Получает целое число из данных
        /// </summary>
        /// <param name="key">Ключ</param>
        /// <param name="default">Значение по умолчание. В случае, если в данных нет объекта по ключу key</param>
        /// <returns>Объект по ключу или @default</returns>
        public int GetInt(string key, int @default)
        {
            DataInfo info = null;
            if (key != null && !IsKeyRemoved(key) && data.TryGetValue(key, out info) && info.Value != null)
            {
                try
                {
                    int result = (int)info.Value;
                    return result;
                }
                catch
                {
                    return @default;
                }
            }

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
            if (!IsKeyRemoved(key) && data.TryGetValue(key, out info) && info.Value != null)
                return (double)info.Value;

            throw new KeyNotFoundException($"Key \'{key}\' can't be found!");
        }

        /// <summary>
        /// Получает число с плавающей точкой из данных
        /// </summary>
        /// <param name="key">Ключ</param>
        /// <param name="default">Значение по умолчание. В случае, если в данных нет объекта по ключу key</param>
        /// <returns>Объект по ключу или @default</returns>
        public double GetDouble(string key, double @default)
        {
            DataInfo info = null;
            if (key != null && !IsKeyRemoved(key) && data.TryGetValue(key, out info) && info.Value != null)
            {
                try
                {
                    double result = (double)info.Value;
                    return result;
                }
                catch
                {
                    return @default;
                }
            }

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
            if (!IsKeyRemoved(key) && data.TryGetValue(key, out info) && info.Value != null)
                return (bool)info.Value;

            throw new KeyNotFoundException($"Key \'{key}\' can't be found!");
        }

        /// <summary>
        /// Получает логическое значение из данных
        /// </summary>
        /// <param name="key">Ключ</param>
        /// <param name="default">Значение по умолчание. В случае, если в данных нет объекта по ключу key</param>
        /// <returns>Объект по ключу или @default</returns>
        public bool GetBool(string key, bool @default)
        {
            DataInfo info = null;
            if (key != null && !IsKeyRemoved(key) && data.TryGetValue(key, out info) && info.Value != null)
            {
                try
                {
                    bool result = (bool)info.Value;
                    return result;
                }
                catch
                {
                    return @default;
                }
            }

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
            if (key != null && !IsKeyRemoved(key) && data.TryGetValue(key, out info) && info.Value != null)
                return info.Value.ToString();

            return @default;
        }

        /// <summary>
        /// Возвращает <see cref="Type"></see> объекта из данных по ключу.
        /// Безопасен. Не выбрасывает исключений
        /// </summary>
        /// <param name="key">Ключ</param>
        public Type GetType(string key)
        {
            DataInfo info = null;
            if (key != null && !IsKeyRemoved(key) && data.TryGetValue(key, out info) && info.Value != null)
                return info.Value.GetType();

            return (Type)null;
        }

        #endregion

        #region increase & decrease

        public void Increase(string key, int valueToIncrease = 1)
        {
            var type = GetType(key);
            if (type == null || type == typeof(int))
            {
                var obj = GetInt(key, default(int));
                Set(key, obj + valueToIncrease);
            }
        }

        public void Increase(string key, float valueToIncrease = 1f)
        {
            var type = GetType(key);
            if (type == null || type == typeof(float))
            {
                var obj = Get<float>(key, default(float));
                Set(key, obj + valueToIncrease);
            }
        }

        public void Increase(string key, double valueToIncrease = 1.0)
        {
            var type = GetType(key);
            if (type == null || type == typeof(double))
            {
                var obj = GetDouble(key, default(double));
                Set(key, obj + valueToIncrease);
            }
        }

        public void Decrease(string key, int valueToDecrease = 1)
        {
            var type = GetType(key);
            if (type == null || type == typeof(int))
            {
                var obj = GetInt(key, default(int));
                Set(key, obj - valueToDecrease);
            }
        }

        public void Decrease(string key, float valueToDecrease = 1f)
        {
            var type = GetType(key);
            if (type == null || type == typeof(float))
            {
                var obj = Get<float>(key, default(float));
                Set(key, obj - valueToDecrease);
            }
        }

        public void Decrease(string key, double valueToDecrease = 1.0)
        {
            var type = GetType(key);
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
        public int Count
        {
            get { return data.Count - removedKeys.Count; }
        }

        /// <summary>
        /// Уничтожает все данные в модели
        /// </summary>
        public void Clear()
        {
            data.Clear();
            removedKeys.Clear();
        }

        /// <summary>
        /// Уничтожает все данные в модели по ключу.
        /// Эквивалентно вызову <seealso cref="Remove(string)"/> + <seealso cref="Unbind(string)"/>
        /// </summary>
        public void Clear(string key)
        {
            if (key == null) return;
            data.Remove(key);
            removedKeys.Remove(key);
        }

        /// <summary>
        /// Проверяет, содержится ли в модели данные по ключу key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool ContainsKey(string key)
        {
            return key != null && !IsKeyRemoved(key) && data.ContainsKey(key);
        }

        #endregion
    }
}
