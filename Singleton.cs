using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public abstract class Singleton<T> where T : Singleton<T>, new()
{
    static object _lock = new object();
    private static T _instance;

    public static T Instance
    {
        get
        {
            if(_instance == null)
            {
                lock(_lock)
                {
                    if(_instance == null)
                    {
                        _instance = new T();
                        System.Threading.Thread.MemoryBarrier();
                        _instance.InitSingleton();
                    }
                }
            }

            return _instance;
        }
    }

    public virtual void InitSingleton()
    {

    }

    public virtual void Release()
    {
        _instance = default;
    }
}
