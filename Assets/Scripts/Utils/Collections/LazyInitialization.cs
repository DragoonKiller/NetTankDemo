using System;
using System.Collections.Generic;
using Utils;

public struct Lazy<T> where T : class
{
    T val;
    Func<T> initializer;
    public Lazy(Func<T> initializer) => (this.initializer, this.val) = (initializer, null);
    public T inst
    {
        get => val == null ? val = initializer() : val;
    }
}
