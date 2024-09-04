using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Summer
{

    /// <summary>
    /// 单例模式基础类
    /// </summary>
    /// <typeparam name="T">泛型</typeparam>
    public class Singleton<T> where T : new()
    {
        private static T? instance;

        private static object lockObj = new();  // 全局唯一锁

        public static T Instance
        {
            get
            {
                //线程安全的单例对象
                if (instance != null) return instance;
                lock (lockObj)
                {
                    if (instance == null)
                    {
                        instance = new T();
                    }
                }
                return instance;

            }
        }
    }
}
