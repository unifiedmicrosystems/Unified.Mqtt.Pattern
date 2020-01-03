using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Unified.Mqtt.Pattern.Test
{
    public class ArrayComparer<T> : IEqualityComparer<T[]>
    {
        private readonly IEqualityComparer<T> valueComparer;
        public ArrayComparer(IEqualityComparer<T> valueComparer = null)
        {
            this.valueComparer = valueComparer ?? EqualityComparer<T>.Default;
        }

        public bool Equals(T[] x, T[] y)
        {
            if (x.Length != y.Length)
                return false;

            for (var i = 0; i < x.Length; i++)
            {
                if (valueComparer.Equals(x[i], y[i]) == false)
                    return false;
            }

            return true;
        }

        public int GetHashCode([DisallowNull] T[] obj)
        {
            throw new NotImplementedException();
        }
    }
}