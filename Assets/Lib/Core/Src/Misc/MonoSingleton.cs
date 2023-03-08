using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

namespace CoreLib
{
#if DEBUG_SINGLETON
    public class SingletonDebug
    {
        public static int InstanceCount = 0;
        public static HashSet<string> ClassNames = new HashSet<string>();
    }
#endif
	public class MonoSingleton<T> where T:MonoSingleton<T> 
	{
		protected static T _instance;

		protected virtual void Init()
		{
            // Not all classes call base init. 
		}

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Activator.CreateInstance<T>();
                    _instance.Init();

#if DEBUG_SINGLETON
                    ++SingletonDebug.InstanceCount;
                    SingletonDebug.ClassNames.Add(_instance.ToString());
#endif
                }

                return _instance;
            }
        }

		protected virtual void Dispose()
		{
			if (_instance == this)
			{
#if DEBUG_SINGLETON
                SingletonDebug.ClassNames.Remove(_instance.ToString());
                --SingletonDebug.InstanceCount;                
#endif
                _instance = null;
			}
		}

		public virtual void Reset() 
        {
			Dispose ();
		}
	}
}
