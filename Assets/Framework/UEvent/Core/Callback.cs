namespace UFramework
{
    public delegate void Callback();
    public delegate void Callback<T>(T arg);
    public delegate void Callback<T1, T2>(T1 arg1, T2 arg2);
    public delegate void Callback<T1, T2, T3>(T1 arg1, T2 arg2, T3 arg3);
    public delegate void Callback<T1, T2, T3, T4>(T1 arg1, T2 arg2, T3 arg3, T4 arg4);
    public delegate void Callback<T1, T2, T3, T4, T5>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);
    public delegate void Callback<T1, T2, T3, T4, T5, T6>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6);
    public delegate object Callback_Return();
    public delegate object Callback_Return<T>(T arg);
    public delegate object Callback_Return<T1, T2>(T1 arg1, T2 arg2);
    public delegate object Callback_Return<T1, T2, T3>(T1 arg1, T2 arg2, T3 arg3);
    public delegate object Callback_Return<T1, T2, T3, T4>(T1 arg1, T2 arg2, T3 arg3, T4 arg4);
    public delegate object Callback_Return<T1, T2, T3, T4, T5>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);
    public delegate object Callback_Return<T1, T2, T3, T4, T5, T6>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6);
}
