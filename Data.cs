// DataKeeper v1.3
// created by TezRomacH

using System;
using System.Collections.Generic;

public sealed class Data
{
    private Dictionary<string, object> data;
    private Dictionary<string, List<Action>> bindedActions;

    private static volatile Data instance;
    private Data()
    {
        data = new Dictionary<string, object>();
        bindedActions = new Dictionary<string, List<Action>>();
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
    /// Связывает выполнение действия action при изменении значения данных ко ключу key
    /// </summary>
    public void BindChangeField(string key, Action action)
    {
        List<Action> actions = null;
        if (bindedActions.TryGetValue(key, out actions))
        {
            actions?.Add(action);
            return;
        }

        bindedActions[key] = new List<Action> { action };
    }

    /// <summary>
    /// Связывает выполнение всех действий actions при изменении значения данных ко ключу key
    /// </summary>
    /// <param name="key"></param>
    /// <param name="actions"></param>
    public void BindChangeField(string key, IEnumerable<Action> actions)
    {
        List<Action> localActions = null;
        if (bindedActions.TryGetValue(key, out localActions))
        {
            localActions.AddRange(actions);
            return;
        }

        bindedActions[key] = new List<Action>(actions);
    }

    /// <summary>
    /// Удаляет все связанные действия по ключу key
    /// </summary>
    /// <param name="key">Ключ связки</param>
    public void Unbind(string key)
    {
        List<Action> actions = null;
        if (bindedActions.TryGetValue(key, out actions) && actions != null)
            bindedActions[key] = new List<Action>();
    }

    private void InvokeAll(string key)
    {
        List<Action> actions = null;
        if (bindedActions.TryGetValue(key, out actions))
            actions?.ForEach(action => action?.Invoke());
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
        data[key] = value;
        InvokeAll(key);
    }

    /// <summary>
    /// Получает объект из данных
    /// </summary>
    /// <param name="key">Ключ</param>
    /// <returns>Объект по ключу или null</returns>
    public object Get(string key)
    {
        object value = null;
        if (data.TryGetValue(key, out value))
            return value;

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
        object value = null;
        if (data.TryGetValue(key, out value))
        {
            return (T) value;
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
        object value = null;
        if (data.TryGetValue(key, out value))
            return (int) value;

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
        object value;
        if (data.TryGetValue(key, out value))
            return (float)value;

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
        object value = null;
        if (data.TryGetValue(key, out value))
            return (bool) value;

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
        object value = null;
        if (data.TryGetValue(key, out value))
            return value.ToString();

        return @default;
    }

    public Type GetValueType(string key)
    {
        object value = null;
        return data.TryGetValue(key, out value) ? value.GetType() : (Type) null;
    }

    public void Increase(string key, int valueToIncrease = 1)
    {
        if (Instance.GetValueType(key) == typeof(int))
        {
            var obj = Instance.GetInt(key);
            Instance.Set(key, obj + valueToIncrease);
        }
    }

    public void Increase(string key, float valueToIncrease = 1f)
    {
        if (Instance.GetValueType(key) == typeof(float))
        {
            var obj = Instance.GetFloat(key);
            Instance.Set(key, obj + valueToIncrease);
        }
    }

    public void Increase(string key, double valueToIncrease = 1.0)
    {
        if (Instance.GetValueType(key) == typeof(double))
        {
            var obj = Instance.Get<double>(key);
            Instance.Set(key, obj + valueToIncrease);
        }
    }

    public void Decrease(string key, int valueToDecrease = 1)
    {
        if (Instance.GetValueType(key) == typeof(int))
        {
            var obj = Instance.GetInt(key);
            Instance.Set(key, obj - valueToDecrease);
        }
    }

    public void Decrease(string key, float valueToDecrease = 1f)
    {
        if (Instance.GetValueType(key) == typeof(float))
        {
            var obj = Instance.GetFloat(key);
            Instance.Set(key, obj - valueToDecrease);
        }
    }

    public void Decrease(string key, double valueToDecrease = 1.0)
    {
        if (Instance.GetValueType(key) == typeof(double))
        {
            var obj = Instance.Get<double>(key);
            Instance.Set(key, obj - valueToDecrease);
        }
    }

    #endregion
}
