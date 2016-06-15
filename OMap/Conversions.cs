using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMap
{
    public interface IConversion
    {
        bool CanConvert(Type source, Type target);
        IConverter Create(Type source, Type target);
    }

    public interface IConverter
    {
    }

    public interface IConverter<TSource, TTarget> : IConverter
    {
        TTarget Convert(TSource source);
    }

    public class GenericConversion<TSource, TTarget> : IConversion
    {
        private readonly Func<TSource, TTarget> _func;

        public GenericConversion(Func<TSource, TTarget> func)
        {
            _func = func;
        }

        public bool CanConvert(Type source, Type target)
        {
            return source == typeof(TSource) && target == typeof(TTarget);
        }

        public IConverter Create(Type source, Type target)
        {
            return new GenericConverter<TSource, TTarget>(_func);
        }
    }

    public class GenericConverter<TSource, TTarget> : IConverter<TSource, TTarget>
    {
        private readonly Func<TSource, TTarget> _func;

        public GenericConverter(Func<TSource, TTarget> func)
        {
            _func = func;
        }

        public TTarget Convert(TSource source)
        {
            return _func(source);
        }
    }

    public class ConvertibleConversion : IConversion
    {
        private readonly List<Tuple<Type, Type, IConverter>> _converters = new List<Tuple<Type, Type, IConverter>>();

        public static ConvertibleConversion CreateDefault()
        {
            var result = new ConvertibleConversion();

            //Lossless conversions
            result.AddConverter<int, long>(x => x);
            result.AddConverter<int, decimal>(x => x);
            result.AddConverter<int, double>(x => x);
            result.AddConverter<int, float>(x => x);

            result.AddConverter<short, int>(x => x);
            result.AddConverter<short, long>(x => x);
            result.AddConverter<short, decimal>(x => x);

            result.AddConverter<byte, short>(x => x);
            result.AddConverter<byte, int>(x => x);
            result.AddConverter<byte, long>(x => x);
            result.AddConverter<byte, decimal>(x => x);

            result.AddConverter<bool, int>(x => x ? 1 : 0);
            result.AddConverter<bool, short>(x => x ? (short)1 : (short)0);
            result.AddConverter<bool, byte>(x => x ? (byte)1 : (byte)0);

            result.AddConverter<decimal, double>(x => (double)x);
            result.AddConverter<decimal, float>(x => (float)x);


            result.AddConverter<DateTime, DateTimeOffset>(x => x);

            result.AddConverter<DateTimeOffset, DateTime>(x => x.DateTime);

            return result;
        }


        public void AddConverter<TSource, TTarget>(Func<TSource, TTarget> func)
        {
            _converters.Add(Tuple.Create(typeof(TSource), typeof(TTarget), (IConverter)new GenericConverter<TSource, TTarget>(func)));
        }

        public bool CanConvert(Type source, Type target)
        {
            return _converters.Any(x => x.Item1 == source && x.Item2 == target);
        }

        public IConverter Create(Type source, Type target)
        {
            var row = _converters.FirstOrDefault(x => x.Item1 == source && x.Item2 == target);
            if (row == null) throw new InvalidOperationException(string.Format("Converter for {0}->{1} does not exist.", source.Name, target.Name));
            return row.Item3;
        }
    }

    public class NullableConversion : IConversion
    {
        public bool CanConvert(Type source, Type target)
        {
            Type tmp;
            return CanConvert(source, target, out tmp);
        }

        private bool CanConvert(Type source, Type target, out Type nullableArg)
        {
            nullableArg = null;
            if (source.IsGenericType && source.GetGenericTypeDefinition() == typeof(Nullable<>) && source.GetGenericArguments()[0] == target)
            {
                nullableArg = target;
                return true;
            }
            if (target.IsGenericType && target.GetGenericTypeDefinition() == typeof(Nullable<>) && target.GetGenericArguments()[0] == source)
            {
                nullableArg = source;
                return true;
            }
            return false;
        }

        public IConverter Create(Type source, Type target)
        {
            Type nullableArg;
            var canConvert = CanConvert(source, target, out nullableArg);
            if (!canConvert) throw new InvalidOperationException(string.Format("Cannot convert from {0}->{1}", source.Name, target.Name));
            var result = (IConverter)Activator.CreateInstance(typeof(NullableConverter<>).MakeGenericType(nullableArg));
            return result;
        }
    }

    public class NullableConverter<TValue> : IConverter<TValue, TValue?>, IConverter<TValue?, TValue>
        where TValue : struct
    {
        public TValue Convert(TValue? source)
        {
            return source ?? default(TValue);
        }

        public TValue? Convert(TValue source)
        {
            return source;
        }
    }


}
