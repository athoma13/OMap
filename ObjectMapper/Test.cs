using System;
using System.Runtime.InteropServices.ComTypes;
using NUnit.Framework;

namespace ObjectMapper
{
    public class ResolverMock : IDependencyResolver
    {
        public object Resolve(Type type, string name = null)
        {
            return null;
        }
    }


    [TestFixture]
    public class ObjectMapperTests
    {
        public class Foo
        {
            public int Property1 { get; set; }
        }
        public class FooX : Foo
        {
            public int Property2 { get; set; }
        }


        public class Bar
        {
            public int Property3 { get; set; }
        }

        public class BarX : Bar
        {
            public int Property4 { get; set; }
        }


        [Test]
        public void ShouldMapProperty()
        {
            var foo = new Foo() { Property1 = 18 };
            var mapper = CreateMapper(new ResolverMock(), builder =>
            {
                builder.CreateMap<Foo, Bar>()
                    .MapProperty(x => x.Property1, x => x.Property3);
            });

            var bar = mapper.Map<Bar>(foo);
            Assert.AreEqual(foo.Property1, bar.Property3);
        }

        [Test]
        public void ShouldMapPropertyExistingObject()
        {
            var foo = new Foo() { Property1 = 18 };
            var mapper = CreateMapper(new ResolverMock(), builder =>
            {
                builder.CreateMap<Foo, Bar>()
                    .MapProperty(x => x.Property1, x => x.Property3);
            });

            var bar = new Bar();
            mapper.Map(foo, bar);
            Assert.AreEqual(foo.Property1, bar.Property3);
        }

        [Test]
        public void ShouldMapPropertyWithFunction()
        {
            var foo = new Foo() { Property1 = 18 };
            var mapper = CreateMapper(new ResolverMock(), builder =>
            {
                builder.CreateMap<Foo, Bar>()
                    .MapProperty(x => x.Property1 * 5, x => x.Property3);
            });

            var bar = mapper.Map<Bar>(foo);
            Assert.AreEqual(foo.Property1 * 5, bar.Property3);
        }

        [Test]
        public void ShouldMapPropertyWithFunctionExistingObject()
        {
            var foo = new Foo() { Property1 = 18 };
            var mapper = CreateMapper(new ResolverMock(), builder =>
            {
                builder.CreateMap<Foo, Bar>()
                    .MapProperty(x => x.Property1 * 5, x => x.Property3);
            });


            var bar = new Bar();
            mapper.Map(foo, bar);
            Assert.AreEqual(foo.Property1 * 5, bar.Property3);
        }

        [Test]
        public void ShouldMapInheritedProperty()
        {
            var foo = new FooX() { Property1 = 18, Property2 = 7};
            var mapper = CreateMapper(new ResolverMock(), builder =>
            {
                builder.CreateMap<Foo, Bar>()
                    .MapProperty(x => x.Property1, x => x.Property3);

                builder.CreateMap<FooX, BarX>()
                    .MapProperty(x => x.Property2, x => x.Property4);
            });

            var bar = mapper.Map<Bar>(foo);
            Assert.IsInstanceOf<BarX>(bar, "Even if you have requested 'Bar', the mapper found a more specific type and will return that instead.");
            var barX = (BarX)bar;

            Assert.AreEqual(foo.Property1, barX.Property3);
            Assert.AreEqual(foo.Property2, barX.Property4);
        }

        [Test]
        public void ShouldMapInheritedPropertyExistingObject()
        {
            var foo = new FooX() { Property1 = 18, Property2 = 7 };
            var mapper = CreateMapper(new ResolverMock(), builder =>
            {
                builder.CreateMap<Foo, Bar>()
                    .MapProperty(x => x.Property1, x => x.Property3);

                builder.CreateMap<FooX, BarX>()
                    .MapProperty(x => x.Property2, x => x.Property4);
            });

            var bar = new Bar();
            mapper.Map(foo, bar);

            Assert.AreEqual(foo.Property1, bar.Property3);
        }


        private static IMapper CreateMapper(IDependencyResolver resolver, Action<ConfigurationBuilder> builder)
        {
            var mappingProvider = new MappingConfigurationProvider(builder);
            var mapper = new ObjectMapper(mappingProvider, resolver);
            return mapper;
        }
    }
}