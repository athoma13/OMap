using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Cryptography.X509Certificates;
using NUnit.Framework;

namespace OMap.Tests
{
    public class ResolverMock : IDependencyResolver
    {
        private readonly Dictionary<Tuple<Type, string>, Delegate> _dependencies = new Dictionary<Tuple<Type, string>, Delegate>();

        public void Add<T>(Func<T> dependency, string name = null)
        {
            _dependencies[Tuple.Create(typeof(T), name)] = dependency;
        }

        public object Resolve(Type type, string name = null)
        {
            Delegate tmp;
            if (!_dependencies.TryGetValue(Tuple.Create(type, name), out tmp)) throw new InvalidOperationException("Dependency not registered");
            return tmp.DynamicInvoke();
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
        public class FooAll
        {
            public int Property1 { get; set; }
            public string Property2 { get; set; }
            public bool Property3 { get; set; }
            public DateTime Property4 { get; set; }
            public Foo[] Foos { get; set; }
            public Foo FooSingle { get; set; }
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
            public IEnumerable<Bar> BarEnumerable { get; set; }
            public IList<Bar> BarIList { get; set; }
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
        public class BarAll
        {
            public int Property1 { get; set; }
            public string Property2 { get; set; }
            public bool Property3 { get; set; }
            public DateTime Property4 { get; set; }
            public Bar[] Foos { get; set; }
            public Bar FooSingle { get; set; }
        }
        public class Dependency1
        {
            private static readonly Random _random = new Random();

            public int RandomProperty { get; private set; }
            public int GlobalProperty1 { get; set; }

            public Dependency1()
            {
                while (RandomProperty == 0)
                {
                    RandomProperty = _random.Next();
                }
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
            var foo = new FooX() { Property1 = 18, Property2 = 7 };
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
        public void ShouldNotLookForParentsWhenMappingInheritedProperties()
        {
            //NOTE: By design, when calling Map<FooX>, no mappings for Foo will be used - eventhough FooX inherits from Foo.
            //To get all mappings, ca

            var foo = new FooX() { Property1 = 18, Property2 = 7 };
            var mapper = CreateMapper(new ResolverMock(), builder =>
            {
                builder.CreateMap<Foo, Bar>()
                    .MapProperty(x => x.Property1, x => x.Property3);

                builder.CreateMap<FooX, BarX>()
                    .MapProperty(x => x.Property2, x => x.Property4);
            });

            //Not specifying TTargetBase will skip all mappings from BarX upwards (the inheritance chain).
            var barX1 = mapper.Map<BarX, BarX>(foo);
            Assert.AreEqual(0, barX1.Property3);
            Assert.AreEqual(7, barX1.Property4);

            //Specifying TTargetBase will enture all mappings from Bar downwards (the inheritance chain) are applied.
            var barX2 = mapper.Map<BarX, Bar>(foo);
            Assert.AreEqual(18, barX2.Property3);
            Assert.AreEqual(7, barX2.Property4);
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
            var barO = new BarO { Bar = bar };
            mapper.Map(foo, barO);
            Assert.AreEqual(foo.Property1, barO.Property1);
            Assert.AreEqual(foo.Foo.Property1, barO.Bar.Property3);
            Assert.AreSame(bar, barO.Bar, "Should not have created a new instance of Bar");
        }

        [Test]
        public void ShouldMapTargetArray()
        {
            AssertCollectionMap(x => x.BarArray);
        }

        [Test]
        public void ShouldMapTargetList()
        {
            AssertCollectionMap(x => x.BarList);
        }

        [Test]
        public void ShouldMapTargetIEnumerable()
        {
            AssertCollectionMap(x => x.BarEnumerable);
        }

        [Test]
        public void ShouldMapTargetIList()
        {
            AssertCollectionMap(x => x.BarIList);
        }

        [Test]
        public void ShouldMapTargetListNoSetter()
        {
            AssertCollectionMap(x => x.BarListNoSetter);
        }

        [Test]
        public void ShouldMapTargetListNotEmpty()
        {
            AssertCollectionMap(x => x.BarListNotEmpty);
        }

        [Test]
        public void ShouldMapTargetListNoSetterNotEmpty()
        {
            AssertCollectionMap(x => x.BarListNoSetterNotEmpty);
        }

        private void AssertCollectionMap(Expression<Func<BarC, IEnumerable<Bar>>> collectionExpression)
        {
            var fooC = new FooC() { Property1 = 18, Foos = new Foo[] { new Foo() { Property1 = 1 }, new Foo() { Property1 = 2 }, new FooX() { Property1 = 3, Property2 = 33}  } };

            var mapper = CreateMapper(new ResolverMock(), builder =>
            {
                builder.CreateMap<Foo, Bar>()
                    .MapProperty(x => x.Property1, x => x.Property3);

                builder.CreateMap<FooX, BarX>()
                    .MapProperty(x => x.Property2, x => x.Property4);

                builder.CreateMap<FooC, BarC>()
                    .MapProperty(x => x.Property1, x => x.Property1)
                    .MapCollection(x => x.Foos, collectionExpression);
            });

            var barC = mapper.Map<BarC>(fooC);
            var func = collectionExpression.Compile();
            var enumerable = func(barC);

            Assert.AreEqual(18, barC.Property1);
            Assert.AreEqual(3, enumerable.Count());
            Assert.AreEqual(1, enumerable.ElementAt(0).Property3);
            Assert.AreEqual(2, enumerable.ElementAt(1).Property3);
            Assert.AreEqual(3, enumerable.ElementAt(2).Property3);
            Assert.IsInstanceOf<BarX>(enumerable.ElementAt(2));
            Assert.AreEqual(33, ((BarX)enumerable.ElementAt(2)).Property4);
        }



        [Test]
        public void ShouldMapWithResolver()
        {
            var foo = new Foo() { Property1 = 891 };
            var resolver = new ResolverMock();
            resolver.Add(() => new Dependency1() { GlobalProperty1 = 5 });

            var mapper = CreateMapper(resolver, builder =>
            {
                builder.CreateMap<Foo, Bar>()
                    .WithDependencies<Dependency1>()
                    .MapProperty((x, dep) => dep.Item1.GlobalProperty1 + x.Property1, x => x.Property3);
            });

            var bar = mapper.Map<Bar>(foo);
            Assert.AreEqual(896, bar.Property3);
        }

        [Test]
        public void ShouldMapWithResolverByName()
        {
            var foo = new Foo() { Property1 = 891 };
            var resolver = new ResolverMock();
            resolver.Add(() => new Dependency1() { GlobalProperty1 = 5 }, "dependency1");

            var mapper = CreateMapper(resolver, builder =>
            {
                builder.CreateMap<Foo, Bar>()
                    .WithDependencies<Dependency1>("dependency1")
                    .MapProperty((x, dep) => dep.Item1.GlobalProperty1 + x.Property1, x => x.Property3);
            });

            var bar = mapper.Map<Bar>(foo);
            Assert.AreEqual(896, bar.Property3);
        }

        [Test]
        public void ShouldMapWithDifferentDependenciesResolvedByName()
        {
            var foo = new FooX() { Property1 = 10, Property2 = 100 };
            var resolver = new ResolverMock();
            resolver.Add(() => new Dependency1() { GlobalProperty1 = 1 }, "dependency1");
            resolver.Add(() => new Dependency1() { GlobalProperty1 = 2 }, "dependency2");

            var mapper = CreateMapper(resolver, builder =>
            {
                builder.CreateMap<Foo, Bar>()
                    .WithDependencies<Dependency1>("dependency1")
                    .MapProperty((x, dep) => dep.Item1.GlobalProperty1 + x.Property1, x => x.Property3);

                builder.CreateMap<FooX, BarX>()
                    .WithDependencies<Dependency1>("dependency2")
                    .MapProperty((x, dep) => dep.Item1.GlobalProperty1 + x.Property2, x => x.Property4);

            });

            var bar = (BarX)mapper.Map<Bar>(foo);
            Assert.AreEqual(11, bar.Property3);
            Assert.AreEqual(102, bar.Property4);
        }



        [Test]
        public void ShouldUseSameDependencyDuringMappingOperation()
        {
            //While Mapping, if requesting the same dependency on multiple maps, the same dependency should be
            //used (instead of going back to the IOC container).
            //The idea is that mapping an object is a single function (and therefore the dependencies should be resolved only once).

            var foo = new FooX();
            var resolver = new ResolverMock();
            resolver.Add(() => new Dependency1());

            var mapper = CreateMapper(resolver, builder =>
            {
                builder.CreateMap<FooX, BarX>()
                    .WithDependencies<Dependency1>()
                    .MapProperty((x, dep) => dep.Item1.RandomProperty, x => x.Property4)
                    .MapProperty((x, dep) => dep.Item1.RandomProperty, x => x.Property3);
            });

            var bar1 = mapper.Map<BarX>(foo);
            var bar2 = mapper.Map<BarX>(foo);

            Assert.AreEqual(bar1.Property3, bar1.Property4, "Those properties must be the same!");
            Assert.AreEqual(bar2.Property3, bar2.Property4, "Those properties must be the same!");
            Assert.AreNotEqual(bar1.Property3, bar2.Property3, "Those properties cannot be the same!");
            Assert.AreNotEqual(bar1.Property4, bar2.Property4, "Those properties cannot be the same!");
        }

        [Test]
        public void ShouldUseSameDependencyDuringMappingOperationOfComposedChildren()
        {
            var fooC = new FooC() { Foos = new Foo[] { new Foo(), new Foo(), new Foo() } };
            var resolver = new ResolverMock();
            resolver.Add(() => new Dependency1());

            var mapper = CreateMapper(resolver, builder =>
            {
                builder.CreateMap<Foo, Bar>()
                    .WithDependencies<Dependency1>()
                    .MapProperty((x, d) => d.Item1.RandomProperty, x => x.Property3);

                builder.CreateMap<FooC, BarC>()
                    .MapCollection(x => x.Foos, x => x.BarArray);
            });

            var barC = mapper.Map<BarC>(fooC);
            var randomNumber = barC.BarArray.Select(x => x.Property3).Distinct().ToArray();
            Assert.AreEqual(1, randomNumber.Length, "Expected all Bars to have the same Property3 because they should have used the same dependency");
            Assert.AreNotEqual(0, randomNumber[0], "Should be a random int - not equal to zero");
        }

        [Test]
        public void ShouldUseMappingFunction()
        {
            var foo = new Foo() { Property1 = 18 };
            var mapper = CreateMapper(new ResolverMock(), builder =>
            {
                builder.CreateMap<Foo, Bar>()
                    .MapFunction((src, tgt) => tgt.Property3 = src.Property1);
            });

            var bar = mapper.Map<Bar>(foo);
            Assert.AreEqual(foo.Property1, bar.Property3);
        }

        [Test]
        public void ShouldUseMappingFunctionWithDependencies()
        {
            var foo = new Foo() { Property1 = 18 };
            var resolver = new ResolverMock();
            resolver.Add(() => new Dependency1() { GlobalProperty1 = 10 });

            var mapper = CreateMapper(resolver, builder =>
            {
                builder.CreateMap<Foo, Bar>()
                    .WithDependencies<Dependency1>()
                    .MapFunction((src, tgt, dependencies) => tgt.Property3 = src.Property1 + dependencies.Item1.GlobalProperty1);
            });

            var bar = mapper.Map<Bar>(foo);
            Assert.AreEqual(28, bar.Property3);
        }

        [Test]
        public void ShouldUseMappingFunctionWithDependenciesDoNotUseDependency()
        {
            var foo = new Foo() { Property1 = 6 };
            var resolver = new ResolverMock();

            var mapper = CreateMapper(resolver, builder =>
            {
                builder.CreateMap<Foo, Bar>()
                    .WithDependencies<Dependency1>()
                    .MapFunction((src, tgt) => tgt.Property3 = src.Property1);
            });

            var bar = mapper.Map<Bar>(foo);
            Assert.AreEqual(6, bar.Property3);
        }


        [Test]
        public void ShouldUseMappingFunctionInherited()
        {
            var fooX = new FooX() { Property1 = 1, Property2 = 7 };
            var mapper = CreateMapper(new ResolverMock(), builder =>
            {
                //NOTE: This is interesting because FooBarMapping function take Foo and Bar, not FooX and BarX
                builder.CreateMap<FooX, BarX>()
                    .MapFunction(FooBarMappingFunction)
                    .MapProperty(x => x.Property2, x => x.Property4);
            });

            var barX = mapper.Map<BarX>(fooX);
            Assert.AreEqual(1, barX.Property3);
            Assert.AreEqual(7, barX.Property4);
        }

        [Test]
        public void ShouldMapAll()
        {
            var now = DateTime.Now;
            var fooAll = new FooAll() { Property1 = 59, Property2 = "Hello", Property3 = true, Property4 = now };
            var mapper = CreateMapper(new ResolverMock(), builder =>
            {
                builder.CreateMap<FooAll, BarAll>()
                    .MapAll(x => x.FooSingle, x => x.Foos);
            });

            var barAll = mapper.Map<BarAll>(fooAll);
            Assert.AreEqual(59, barAll.Property1);
            Assert.AreEqual("Hello", barAll.Property2);
            Assert.AreEqual(true, barAll.Property3);
            Assert.AreEqual(now, barAll.Property4);
        }

        [Test]
        public void ShouldMapAllExcept()
        {
            var now = DateTime.Now;
            var fooAll = new FooAll() { Property1 = 59, Property2 = "Hello", Property3 = true, Property4 = now };
            var mapper = CreateMapper(new ResolverMock(), builder =>
            {
                builder.CreateMap<FooAll, BarAll>()
                    .MapAll(x => x.FooSingle, x => x.Foos, x => x.Property1, x => x.Property2);
            });

            var barAll = mapper.Map<BarAll>(fooAll);
            Assert.AreEqual(0, barAll.Property1);
            Assert.AreEqual(null, barAll.Property2);
            Assert.AreEqual(true, barAll.Property3);
            Assert.AreEqual(now, barAll.Property4);
        }

        [Test]
        public void ShouldMapAllWithObject()
        {
            var now = DateTime.Now;
            var fooAll = new FooAll()
            {
                Property1 = 87,
                Property2 = "Hello",
                Property3 = true,
                Property4 = now,
                FooSingle = new Foo() { Property1 = 51 },
                Foos = new [] { new Foo() { Property1 = 1 }, new Foo() { Property1 = 2 } , new Foo() { Property1 = 3 } , new FooX() { Property1 = 4, Property2 = 11 } }
            };
            var mapper = CreateMapper(new ResolverMock(), builder =>
            {
                builder.CreateMap<Foo, Bar>()
                    .MapProperty(x => x.Property1, x => x.Property3);

                builder.CreateMap<FooX, BarX>()
                    .MapProperty(x => x.Property2, x => x.Property4);

                builder.CreateMap<FooAll, BarAll>()
                    .MapAll();
            });

            var barAll = mapper.Map<BarAll>(fooAll);
            Assert.AreEqual(87, barAll.Property1);
            Assert.AreEqual(now, barAll.Property4);
            Assert.AreEqual(1, barAll.Foos[0].Property3);
            Assert.AreEqual(51, barAll.FooSingle.Property3);
            Assert.AreEqual(2, barAll.Foos[1].Property3);
            Assert.AreEqual(3, barAll.Foos[2].Property3);
            Assert.AreEqual(4, barAll.Foos[3].Property3);
            Assert.AreEqual(11, ((BarX)barAll.Foos[3]).Property4);
        }




        private static void FooBarMappingFunction(Foo foo, Bar bar)
        {
            bar.Property3 = foo.Property1;
        }


        private static IObjectMapper CreateMapper(IDependencyResolver resolver, Action<ConfigurationBuilder> builder)
        {
            var mappingProvider = new MappingConfigurationProvider(builder);
            var mapper = new ObjectObjectMapper(mappingProvider, resolver);
            return mapper;
        }
    }
}