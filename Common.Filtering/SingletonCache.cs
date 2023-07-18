using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Common.Filtering
{
    sealed class SingletonCache
    {
        private static readonly object padlock = new object();
        private readonly static Dictionary<Type, PropertyInfo[]>? _typeProps = null;

        SingletonCache()
        {
        }

        public static SingletonCache TypeProperties
        {
            get
            {
                if (instance == null)
                {
                    lock (padlock)
                    {
                        if (instance == null)
                        {
                            instance = new SingletonCache();
                            
                        }
                    }
                }

                return instance;
            }
        }

        public static GetTypeProperties(Type type)
        {

        }
    }
}
