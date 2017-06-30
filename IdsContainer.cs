using System.Collections.Generic;

namespace DataKeeper
{
    internal class Ids
    {
        private static volatile Ids _instance;
        private static object syncRoot = new object();
        
        public static Ids Container
        {
            get
            {
                if (_instance == null)
                {
                    lock (syncRoot)
                    {
                        if (_instance == null)
                            _instance = new Ids();
                    }
                }
                return _instance;
            }
        }

        private Dictionary<string, int> counter = new Dictionary<string, int>();

        internal string GenerateConstraintId()
        {
            return GenerateId("constraint");
        }

        // используется для генерации id: prefix_number
        internal string GenerateId(string prefix)
        {
            int count = 0;

            counter.TryGetValue(prefix, out count);
            counter[prefix] = count + 1;
            return prefix + "_" + count;
        }

        internal string ReturnId(string prefix)
        {
            int count = 0;
            bool idExists = counter.TryGetValue(prefix, out count);

            string result = prefix;
            if (idExists)
            {
                result = prefix + "_" + count;
            }
            counter[prefix] = count + 1;

            return result;
        }
    }
}