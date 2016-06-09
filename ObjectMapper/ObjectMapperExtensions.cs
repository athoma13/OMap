namespace ObjectMapper
{
    public static class ObjectMapperExtensions
    {
        /// <summary>
        /// Maps object to a TTarget. WARNING: If TTarget is a sub-class, use Map&lt;TTarget, TTargetBase&gt;(object source) to ensure all mappings from TTargetBase down to TTarget are applied.
        /// </summary>
        /// <typeparam name="TTarget"></typeparam>
        /// <param name="mapper"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public static TTarget Map<TTarget>(this IObjectMapper mapper, object source)
        {
           return mapper.Map<TTarget, TTarget>(source);
        }
    }
}