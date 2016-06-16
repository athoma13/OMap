using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using NUnit.Framework;

namespace OMap.Tests
{
    [TestFixture]
    public class ConverterTests
    {
        class GenericFoo<T>
        {
            public T Prop { get; set; }
        }
        class GenericBar<T>
        {
            public T Prop { get; set; }
        }

        [Test]
        public void ShouldMapWithGenericConverter()
        {
            ShouldAutoMap(1, true, x => x == 1);
            ShouldAutoMap(1, "10", x => x.ToString() + "0");
            ShouldAutoMap(true, "True", x => x.ToString());
        }

        [Test]
        public void ShouldMapDefaultConverters()
        {
            ShouldAutoMap(10, 10L, ConvertibleConversion.CreateDefault());
            ShouldAutoMap(10, 10M, ConvertibleConversion.CreateDefault());
            ShouldAutoMap(10, (double)10, ConvertibleConversion.CreateDefault());
            ShouldAutoMap(10, (float)10, ConvertibleConversion.CreateDefault());

            ShouldAutoMap((short)30, 30, ConvertibleConversion.CreateDefault());
            ShouldAutoMap((short)30, 30L, ConvertibleConversion.CreateDefault());
            ShouldAutoMap((short)30, 30M, ConvertibleConversion.CreateDefault());

            ShouldAutoMap((byte)30, (short)30, ConvertibleConversion.CreateDefault());
            ShouldAutoMap((byte)30, 30, ConvertibleConversion.CreateDefault());
            ShouldAutoMap((byte)30, 30L, ConvertibleConversion.CreateDefault());
            ShouldAutoMap((byte)30, 30M, ConvertibleConversion.CreateDefault());

            ShouldAutoMap(true, 1, ConvertibleConversion.CreateDefault());
            ShouldAutoMap(false, 0, ConvertibleConversion.CreateDefault());
            ShouldAutoMap(true, (short)1, ConvertibleConversion.CreateDefault());
            ShouldAutoMap(false, (short)0, ConvertibleConversion.CreateDefault());
            ShouldAutoMap(true, (byte)1, ConvertibleConversion.CreateDefault());
            ShouldAutoMap(false, (byte)0, ConvertibleConversion.CreateDefault());

            ShouldAutoMap(12.5M, 12.5F, ConvertibleConversion.CreateDefault());
            ShouldAutoMap(12.5M, 12.5, ConvertibleConversion.CreateDefault());

            var now = DateTimeOffset.Now;
            ShouldAutoMap(now.DateTime, now, ConvertibleConversion.CreateDefault());
            ShouldAutoMap(now, now.DateTime, ConvertibleConversion.CreateDefault());
        }

        [Test]
        public void ShouldMapNullableConverters()
        {
            ShouldAutoMap((int?)10, 10, new NullableConversion());
            ShouldAutoMap((int?)null, 0, new NullableConversion());
            ShouldAutoMap(10, (int?)10, new NullableConversion());
            ShouldAutoMap(0, (int?)0, new NullableConversion());
        }

        private void ShouldAutoMap<TSource, TTarget>(TSource input, TTarget output, params IConversion[] conversions)
        {
            var mapper = CreateMapAllMapper<TSource, TTarget>(b => b.AddConversion(conversions));
            AssertConvertMap(input, output, mapper);
        }
        private void ShouldAutoMap<TSource, TTarget>(TSource input, TTarget output, Func<TSource, TTarget> converter)
        {
            var mapper = CreateMapAllMapper<TSource, TTarget>(b => b.AddConversion(converter));
            AssertConvertMap(input, output, mapper);
        }

        private void AssertConvertMap<TSource, TTarget>(TSource input, TTarget output, IObjectMapper mapper)
        {
            var foo = new GenericFoo<TSource>() { Prop = input };
            var bar = mapper.Map<GenericBar<TTarget>>(foo);
            Assert.AreEqual(output, bar.Prop);
        }



        private static IObjectMapper CreateMapper(IDependencyResolver resolver, Action<ConfigurationBuilder> builder)
        {
            var mappingProvider = new MappingConfigurationProvider(builder);
            var mapper = new ObjectMapper(mappingProvider, resolver);
            return mapper;
        }
        private IObjectMapper CreateMapAllMapper<TSource, TTarget>(Action<ConfigurationBuilder> builder)
        {
            var mapper = CreateMapper(new ResolverMock(), b =>
            {
                builder(b);
                b.CreateMap<GenericFoo<TSource>, GenericBar<TTarget>>().MapAll();
            });
            return mapper;
        }


    }
}
