namespace VEngine
{
    internal class IntHashGenerator
    {
        private static int Counter = 0;

        public static int GetNext()
        {
            return Counter++;
        }
    }
}