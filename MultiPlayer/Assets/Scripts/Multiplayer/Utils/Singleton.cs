/// <summary>
/// Used when isn't necessary Awake, Start, Update, etc
/// </summary>
/// <typeparam name="T"></typeparam>
public class Singleton<T> where T : Singleton<T>, new()
{
    static object handler = new object();

    static T instance;

    public static T Instance
    {
        get
        {
            lock (handler)
            {
                if (instance == null)
                {
                    instance = new T();
                    instance.Initialize();
                }
                return instance;
            }
        }
    }

    public static void Release()
    {
        lock (handler)
        {
            if (instance != null)
                instance.OnRelease();
            instance = null;
        }
    }

    protected virtual void Initialize()
    {

    }

    protected virtual void OnRelease()
    {

    }
}