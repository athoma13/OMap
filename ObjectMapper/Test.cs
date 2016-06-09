using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        public class FooO
        {
            public int Property1 { get; set; }
            public Foo Foo { get; set; }
        }
        public class FooC
        {
            public int Property1 { get; set; }
            public Foo[] Foos { get; set; }
        }

        public class Bar
        {
            public int Property3 { get; set; }
        }
        public class BarX : Bar
        {
            public int Property4 { get; set; }
        }

        public class BarO
        {
            public int Property1 { get; set; }
            public Bar Bar { get; set; }
        }

        public class BarC
        {
            public int Property1 { get; set; }
            public Bar[] BarArray { get; set; }
            public List<Bar> BarList { get; set; }
            public List<Bar> BarListNoSetter { get; private set; }
            public List<Bar> BarListNotEmpty { get; set; }
            public List<Bar> BarListNoSetterNotEmpty { get; private set; }
            public BarC()
            {
                BarListNoSetter = new List<Bar>();
                BarListNotEmpty = new List<Bar>() { new Bar() };
                BarListNoSetterNotEmpty = new List<Bar>() { new Bar() };
            }
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

        [Test]
        public void ShouldMapComposedObject()
        {
            var foo = new FooO() { Property1 = 18, Foo = new Foo() { Property1 = 7 } };

            var mapper = CreateMapper(new ResolverMock(), builder =>
            {
                builder.CreateMap<Foo, Bar>()
                    .MapProperty(x => x.Property1, x => x.Property3);

                builder.CreateMap<FooO, BarO>()
                    .MapProperty(x => x.Property1, x => x.Property1)
                    .MapObject(x => x.Foo, x => x.Bar);
            });

            var bar = mapper.Map<BarO>(foo);
            Assert.AreEqual(foo.Property1, bar.Property1);
            Assert.AreEqual(foo.Foo.Property1, bar.Bar.Property3);
        }

        [Test]
        public void ShouldMapInheritedComposedObject()
        {
            var foo = new FooO() { Property1 = 18, Foo = new FooX() { Property1 = 7, Property2 = 6 } };

            var mapper = CreateMapper(new ResolverMock(), builder =>
            {
                builder.CreateMap<Foo, Bar>()
                    .MapProperty(x => x.Property1, x => x.Property3);

                builder.CreateMap<FooX, BarX>()
                    .MapProperty(x => x.Property2, x => x.Property4);

                builder.CreateMap<FooO, BarO>()
                    .MapProperty(x => x.Property1, x => x.Property1)
                    .MapObject(x => x.Foo, x => x.Bar);
            });

            var bar = mapper.Map<BarO>(foo);
            Assert.AreEqual(foo.Property1, bar.Property1);
            Assert.AreEqual(foo.Foo.Property1, bar.Bar.Property3);
            Assert.IsInstanceOf<BarX>(bar.Bar);
            Assert.AreEqual(((FooX)foo.Foo).Property2, ((BarX)bar.Bar).Property4);
        }

        [Test]
        public void ShouldMapComposedObjectExistingObject()
        {
            var foo = new FooO() { Property1 = 18, Foo = new Foo() { Property1 = 7 } };

            var mapper = CreateMapper(new ResolverMock(), builder =>
            {
                builder.CreateMap<Foo, Bar>()
                    .MapProperty(x => x.Property1, x => x.Property3);

                builder.CreateMap<FooO, BarO>()
                    .MapProperty(x => x.Property1, x => x.Property1)
                    .MapObject(x => x.Foo, x => x.Bar);
            });

            var bar = new Bar();
            var barO = new BarO {Bar = bar };
            mapper.Map(foo, barO);
            Assert.AreEqual(foo.Property1, barO.Property1);
            Assert.AreEqual(foo.Foo.Property1, barO.Bar.Property3);
            Assert.AreSame(bar, barO.Bar, "Should not have created a new instance of Bar");
        }

        [Test]
        public void ShouldMapTargetArray()
        {
            var fooC = new FooC() {Property1 = 18, Foos = new Foo[] {new Foo() {Property1 = 1}, new Foo() {Property1 = 1}}};

            var mapper = CreateMapper(new ResolverMock(), builder =>
            {
                builder.CreateMap<Foo, Bar>()
                    .MapProperty(x => x.Property1, x => x.Property3);

                builder.CreateMap<FooC, BarC>()
                    .MapProperty(x => x.Property1, x => x.Property1)
                    .MapCollection(x => x.Foos, x => x.BarArray);
            });

            var barC = mapper.Map<BarC>(fooC);
            Assert.AreEqual(fooC.Property1, barC.Property1);
            Assert.AreEqual(fooC.Foos.Length, barC.BarArray.Length);
            Assert.AreEqual(fooC.Foos[0].Property1, barC.BarArray[0].Property3);
            Assert.AreEqual(fooC.Foos[1].Property1, barC.BarArray[1].Property3);
        }

        [Test]
        public void ShouldMapTargetArrayWithInheritedElements()
        {
            var fooC = new FooC() { Property1 = 18, Foos = new Foo[] { new Foo() { Property1 = 65 }, new FooX() { Property1 = 79, Property2 = 871} } };

            var mapper = CreateMapper(new ResolverMock(), builder =>
            {
                builder.CreateMap<Foo, Bar>()
                    .MapProperty(x => x.Property1, x => x.Property3);

                builder.CreateMap<FooX, BarX>()
                    .MapProperty(x => x.Property2, x => x.Property4);

                builder.CreateMap<FooC, BarC>()
                    .MapProperty(x => x.Property1, x => x.Property1)
                    .MapCollection(x => x.Foos, x => x.BarArray);
            });

            var barC = mapper.Map<BarC>(fooC);
            Assert.AreEqual(fooC.Property1, barC.Property1);
            Assert.AreEqual(fooC.Foos.Length, barC.BarArray.Length);
            Assert.AreEqual(fooC.Foos[0].Property1, barC.BarArray[0].Property3);
            Assert.AreEqual(fooC.Foos[1].Property1, barC.BarArray[1].Property3);
            Assert.IsInstanceOf<BarX>(barC.BarArray[1]);
            Assert.AreEqual(((FooX)fooC.Foos[1]).Property2, ((BarX)barC.BarArray[1]).Property4);
        }

        [Test]
        public void ShouldMapTargetList()
        {
            var fooC = new FooC() { Property1 = 18, Foos = new Foo[] { new Foo() { Property1 = 1 }, new Foo() { Property1 = 1 } } };

            var mapper = CreateMapper(new ResolverMock(), builder =>
            {
                builder.CreateMap<Foo, Bar>()
                    .MapProperty(x => x.Property1, x => x.Property3);

                builder.CreateMap<FooC, BarC>()
                    .MapProperty(x => x.Property1, x => x.Property1)
                    .MapCollection(x => x.Foos, x => x.BarList);
            });

            var barC = mapper.Map<BarC>(fooC);
            Assert.AreEqual(fooC.Property1, barC.Property1);
            Assert.AreEqual(fooC.Foos.Length, barC.BarList.Count);
            Assert.AreEqual(fooC.Foos[0].Property1, barC.BarList[0].Property3);
            Assert.AreEqual(fooC.Foos[1].Property1, barC.BarList[1].Property3);
        }

        [Test]
        public void ShouldMapTargetListNoSetter()
        {
            var fooC = new FooC() { Property1 = 18, Foos = new Foo[] { new Foo() { Property1 = 1 }, new Foo() { Property1 = 1 } } };

            var mapper = CreateMapper(new ResolverMock(), builder =>
            {
                builder.CreateMap<Foo, Bar>()
                    .MapProperty(x => x.Property1, x => x.Property3);

                builder.CreateMap<FooC, BarC>()
                    .MapProperty(x => x.Property1, x => x.Property1)
                    .MapCollection(x => x.Foos, x => x.BarListNoSetter);
            });

            var barC = mapper.Map<BarC>(fooC);
            Assert.AreEqual(fooC.Property1, barC.Property1);
            Assert.AreEqual(fooC.Foos.Length, barC.BarListNoSetter.Count);
            Assert.AreEqual(fooC.Foos[0].Property1, barC.BarListNoSetter[0].Property3);
            Assert.AreEqual(fooC.Foos[1].Property1, barC.BarListNoSetter[1].Property3);
        }

        [Test]
        public void ShouldMapTargetListNotEmpty()
        {
            var fooC = new FooC() { Property1 = 18, Foos = new Foo[] { new Foo() { Property1 = 1 }, new Foo() { Property1 = 1 } } };

            var mapper = CreateMapper(new ResolverMock(), builder =>
            {
                builder.CreateMap<Foo, Bar>()
                    .MapProperty(x => x.Property1, x => x.Property3);

                builder.CreateMap<FooC, BarC>()
                    .MapProperty(x => x.Property1, x => x.Property1)
                    .MapCollection(x => x.Foos, x => x.BarListNotEmpty);
            });

            var barC = mapper.Map<BarC>(fooC);
            Assert.AreEqual(fooC.Property1, barC.Property1);
            Assert.AreEqual(fooC.Foos.Length, barC.BarListNotEmpty.Count);
            Assert.AreEqual(fooC.Foos[0].Property1, barC.BarListNotEmpty[0].Property3);
            Assert.AreEqual(fooC.Foos[1].Property1, barC.BarListNotEmpty[1].Property3);
        }

        [Test]
        public void ShouldMapTargetListNoSetterNotEmpty()
        {
            var fooC = new FooC() { Property1 = 18, Foos = new Foo[] { new Foo() { Property1 = 1 }, new Foo() { Property1 = 1 } } };

            var mapper = CreateMapper(new ResolverMock(), builder =>
            {
                builder.CreateMap<Foo, Bar>()
                    .MapProperty(x => x.Property1, x => x.Property3);

                builder.CreateMap<FooC, BarC>()
                    .MapProperty(x => x.Property1, x => x.Property1)
                    .MapCollection(x => x.Foos, x => x.BarListNoSetterNotEmpty);
            });

            var barC = mapper.Map<BarC>(fooC);
            Assert.AreEqual(fooC.Property1, barC.Property1);
            Assert.AreEqual(fooC.Foos.Length, barC.BarListNoSetterNotEmpty.Count);
            Assert.AreEqual(fooC.Foos[0].Property1, barC.BarListNoSetterNotEmpty[0].Property3);
            Assert.AreEqual(fooC.Foos[1].Property1, barC.BarListNoSetterNotEmpty[1].Property3);
        }



        private static IObjectMapper CreateMapper(IDependencyResolver resolver, Action<ConfigurationBuilder> builder)
        {
            var mappingProvider = new MappingConfigurationProvider(builder);
            var mapper = new ObjectObjectMapper(mappingProvider, resolver);
            return mapper;
        }
    }
}