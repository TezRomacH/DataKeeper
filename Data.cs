// DataKeeper v1.05
// created by TezRomacH

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
        if (bindedActions.TryGetValue(key, out var actions))
        {
            actions?.Add(action);
            return;
        }

        bindedActions[key] = new List<Action> { action };
    }

    /// <summary>
    /// Удаляет все связанные действия по ключу key
    /// </summary>
    /// <param name="key">Ключ связки</param>
    public void Unbind(string key)
    {
        if (bindedActions.TryGetValue(key, out var actions))
            actions?.Clear();
    }

    /// <summary>
    /// Удаляет действие action по ключу key
    /// </summary>
    /// <param name="key">Ключ связки</param>
    /// <param name="action">Действие, которое нужно удалить из привязки</param>
    public void RemoveAction(string key, Action action)
    {
        if (bindedActions.TryGetValue(key, out var actions))
            actions?.Remove(action);
    }

    private void InvokeAll(string key)
    {
        if (bindedActions.TryGetValue(key, out var actions))
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
        if (data.TryGetValue(key, out var value))
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
        if (data.TryGetValue(key, out var value))
            return (T) value;

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
        if (data.TryGetValue(key, out var value))
            return (int) value;

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
        if (data.TryGetValue(key, out var value))
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
        if (data.TryGetValue(key, out var value))
            return value.ToString();

        return @default;
    }

    #endregion
}
