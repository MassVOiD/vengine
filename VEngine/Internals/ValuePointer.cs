namespace VEngine
{
    public class ValuePointer<T>
    {
        public T R;
        private bool BeenModified = false;

        public ValuePointer(T i)
        {
            R = i;
        }

        public static dynamic Add(dynamic a, dynamic b)
        {
            return a + b;
        }

        public static dynamic Div(dynamic a, dynamic b)
        {
            return a / b;
        }

        public static dynamic Mul(dynamic a, dynamic b)
        {
            return a * b;
        }

        public static implicit operator T(ValuePointer<T> d)
        {
            return d.R;
        }

        public static implicit operator ValuePointer<T>(T d)
        {
            return new ValuePointer<T>(d);
        }

        public static dynamic Sub(dynamic a, dynamic b)
        {
            return a - b;
        }

        public dynamic Add(dynamic a)
        {
            return R + a;
        }

        public void ClearModifiedFlag()
        {
            BeenModified = false;
        }

        public dynamic Div(dynamic a)
        {
            return R / a;
        }

        public bool HasBeenModified()
        {
            return BeenModified;
        }

        public void MarkAsModified()
        {
            BeenModified = true;
        }

        public dynamic Mul(dynamic a)
        {
            return R * a;
        }

        public dynamic Sub(dynamic a)
        {
            return R - a;
        }
    }
}