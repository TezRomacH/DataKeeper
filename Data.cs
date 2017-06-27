// DataKeeper: version 1.8
// Created by TezRomacH (https://github.com/TezRomacH)
// https://github.com/TezRomacH/DataKeeper

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DataKeeper
{
    [DebuggerDisplay("Count = {" + nameof(Count) + "}")]
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
        /// Keys collection
        /// </summary>
        public IReadOnlyCollection<string> Keys
        {
            get
            {
                var set = new SortedSet<string>(data.Keys);
                set.ExceptWith(removedKeys);
                return set as IReadOnlyCollection<string>;
            }
        }

        #region BINDINGS

        /// <summary>
        /// Связывает выполнение действия action до или после изменения значения данных ко ключу key
        /// в зависимости от значения triggerType
        /// </summary>
        public void BindUpdateField(string key, Action action, TriggerType triggerType = TriggerType.After)
        {
            BindField(key, action, BindType.OnUpdate, triggerType);
        }

        /// <summary>
        /// Связывает выполнение действия action до или после удаления значения данных ко ключу key
        /// в зависимости от значения triggerType
        /// </summary>
        public void BindRemoveField(string key, Action action, TriggerType triggerType = TriggerType.Before)
        {
            BindField(key, action, BindType.OnRemove, triggerType);
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

        private void InvokeRemoveTriggers(IEnumerable<Action> actions)
        {
            if (actions == null)
                return;

            foreach (var action in actions)
            {
                try
                {
                    AccessOldValue = true;
                    action?.Invoke();
                }
                catch (Microsoft.CSharp.RuntimeBinder.RuntimeBinderException)
                {
                    // TODO: binder ID
                    throw new DataKeeperTypeMismatch("Type mismatch on binder \"{}\"");
                }
                finally
                {
                    AccessOldValue = false;
                }
            }
        }

        private void InvokeUpdateTriggers(IEnumerable<Action> actions)
        {
            if (actions == null)
                return;

            foreach (var action in actions)
            {
                try
                {
                    AccessBothValues = true;
                    action?.Invoke();
                }
                catch (Microsoft.CSharp.RuntimeBinder.RuntimeBinderException)
                {
                    // TODO: binder ID
                    throw new DataKeeperTypeMismatch("Type mismatch on binder \"{}\"");
                }
                finally
                {
                    AccessBothValues = false;
                }
            }
        }

        #endregion

        #region CONSTRAINTS

        public void BindConstraints(string key, Constraint constraint)
        {
            if (key == null)
                return;

            DataInfo info = null;
            if (data.TryGetValue(key, out info))
            {
                info.AddConstraint(constraint);
                return;
            }

            info = new DataInfo { Value = null };

            info.AddConstraint(constraint);
            data[key] = info;
        }

        public void UpdateConstraint(string constraintId, ConstraintProperties properties)
        {
            Constraint constraint = FindById(constraintId);

            if (constraint != null)
            {
                constraint.Properties = properties;
            }
        }

        public void SetConstraintActivity(string constraintId, ActivityStatus status)
        {
            FindById(constraintId)?.Properties.SetStatus(status);
        }

        #endregion

        #region DATA MANIPULATION

        /// <summary>
        /// Записывает или изменяет значение по ключу key на value
        /// </summary>
        /// <param name="key">Ключ</param>
        /// <param name="value">Объект, которые записывается или изменяется</param>
        public void Set(string key, object value)
        {
            if (key == null)
                return;

            DataInfo info = null;
            if (data.TryGetValue(key, out info))
            {
                OldValue = info.Value;
                NewValue = value;

                if (info.Constraints != null)
                {
                    foreach (var constraint in info.Constraints
                        .Where(c => c.Properties.Status == ActivityStatus.Active))
                    {
                        bool validation = true;
                        try
                        {
                            AccessOldValue = true;
                            validation = constraint.Validate(value);
                        }
                        finally
                        {
                            AccessOldValue = false;
                        }

                        if (!validation)
                        {
                            throw new ConstraintException(constraint.Id,
                                constraint.Properties.ErrorMessage ?? $"Error on constarint {constraint.Id}");
                        }
                    }
                }

                InvokeUpdateTriggers(info.TriggersBeforeUpdate);
                info.Value = value;
                removedKeys.Remove(key); // удаляем из удаленных ключей
                InvokeUpdateTriggers(info.TriggersAfterUpdate);
                return;
            }

            info = new DataInfo { Value = value };
            data[key] = info;
        }

        public bool TrySet(string key, object value, out DataKeeperException ex)
        {
            ex = null;
            try
            {
                Set(key, value);
                return true;
            }
            catch (DataKeeperException e)
            {
                ex = e;
            }

            return false;
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

                OldValue = info.Value;
                InvokeRemoveTriggers(info.TriggersBeforeRemove);
                info.Value = null;
                removedKeys.Add(key);
                InvokeRemoveTriggers(info.TriggersAfterRemove);
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
        public object this[string key, object @default] => Get(key, @default);

        #region OLDVALUE & NEWVALUE

        private bool AccessOldValue { get; set; } = false;
        private bool AccessNewValue { get; set; } = false;

        private bool AccessBothValues
        {
            get { return AccessOldValue && AccessNewValue; }
            set
            {
                AccessOldValue = value;
                AccessNewValue = value;
            }
        }

        private dynamic _oldValue;

        public dynamic OldValue
        {
            get
            {
                if (AccessOldValue)
                    return _oldValue;

                throw new NotOnTriggerException($"Trying to get {nameof(OldValue)} not in a trigger body!");
            }
            private set { _oldValue = value; }
        }

        private dynamic _newValue;

        public dynamic NewValue
        {
            get
            {
                if (AccessNewValue)
                    return _newValue;

                throw new NotOnTriggerException(
                    $"Trying to get {nameof(NewValue)} either not in a trigger body or in a remove binder!");
            }
            private set { _newValue = value; }
        }

        #endregion

        #region GETTERS

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

        #region INCREASE & DECREASE

        public void Increase<T>(string key, T valueToIncrease)
        {
            try
            {
#if LAZY_INCREASE
                if (!ContainsKey(key))
                {
                    Set(key, valueToIncrease);
                    return;
                }
#endif
                object obj = Get(key);
                Set(key, obj.__Add<T>(valueToIncrease));
            }
            catch (Exception e) when (!(e is DataKeeperException))
            {
            }
        }

        public void Decrease<T>(string key, T valueToDecrease)
        {
            try
            {
                object obj = Get(key);
                Set(key, obj.__Substract<T>(valueToDecrease));
            }
            catch (Exception e) when (!(e is DataKeeperException))
            {
            }
        }

        #endregion

        /// <summary>
        /// Возвращает количество данных в модели
        /// </summary>
        public int Count => data.Count - removedKeys.Count;

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
            if (key == null)
                return;

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

        public bool ValueIs<T>(string key, bool ignoreKeyNotFound = true)
        {
            DataInfo info = null;
            if (data.TryGetValue(key, out info))
            {
                return info.Value is T;
            }

            if (ignoreKeyNotFound)
                return false;

            throw new KeyNotFoundException($"Key \'{key}\' can't be found!");
        }

        #endregion
    }
}

