namespace MotionInterpolation
{
    public class Singleton<T> where T : class, new()
    {
        private static readonly object syncLock = new object();
        private static T instance;

        protected Singleton()
        {
        }
        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncLock)
                    {
                        if (instance == null)
                        {
                            instance = new T();
                        }
                    }
                }
                return instance;
            }
        }
    }
}
