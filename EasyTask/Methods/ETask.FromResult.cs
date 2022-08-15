namespace EasyTask
{
    partial struct ETask
    {
        public static ETask<T> FromResult<T>(T result)
            => new ETask<T>(result);
    }
}
