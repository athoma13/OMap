using NUnit.Framework;

namespace OMap.Tests
{
    [TestFixture]
    public class MapAllCircularDependencyTests
    {
        public class Foo
        {
            public int FooProperty { get; set; }
            public Bar Bar { get; set; }
        }

        public class Bar
        {
            public int BarProperty { get; set; }
            public Foo Foo { get; set; }
        }

        public class FooDto
        {
            public int FooProperty { get; set; }
            public BarDto Bar { get; set; }
        }

        public class BarDto
        {
            public int BarProperty { get; set; }
            public FooDto Foo { get; set; }
        }


        [Test]
        public void ShouldMapWhenCircularDependency1()
        {
            var mapper = TestHelper.CreateMapper(b =>
            {
                b.CreateMap<Foo, FooDto>().MapAll();
                b.CreateMap<Bar, BarDto>().MapAll();
            });

            var foo = new Foo() {FooProperty = 1, Bar = new Bar() {BarProperty = 11, Foo = new Foo() { FooProperty = 2 } }};
            var fooDto = mapper.Map<FooDto>(foo);

            Assert.AreEqual(1, fooDto.FooProperty);
            Assert.AreEqual(11, fooDto.Bar.BarProperty);
            Assert.AreEqual(2, fooDto.Bar.Foo.FooProperty);
            Assert.IsNull(fooDto.Bar.Foo.Bar);
        }

        [Test]
        public void ShouldMapWhenCircularDependency2()
        {
            var mapper = TestHelper.CreateMapper(b =>
            {
                b.CreateMap<Foo, FooDto>().MapAll();
                b.CreateMap<Bar, BarDto>().MapAll();
            });

            var bar = new Bar() { BarProperty = 1, Foo = new Foo() { FooProperty = 11, Bar = new Bar() { BarProperty = 2 } } };
            var barDto = mapper.Map<BarDto>(bar);

            Assert.AreEqual(1, barDto.BarProperty);
            Assert.AreEqual(11, barDto.Foo.FooProperty);
            Assert.AreEqual(2, barDto.Foo.Bar.BarProperty);
            Assert.IsNull(barDto.Foo.Bar.Foo);
        }


        [Test]
        public void ShouldMapWhenCircularDependency3()
        {
            var mapper = TestHelper.CreateMapper(b =>
            {
                b.CreateMap<Bar, BarDto>().MapAll();
                b.CreateMap<Foo, FooDto>().MapAll();
            });

            var foo = new Foo() { FooProperty = 1, Bar = new Bar() { BarProperty = 11, Foo = new Foo() { FooProperty = 2 } } };
            var fooDto = mapper.Map<FooDto>(foo);

            Assert.AreEqual(1, fooDto.FooProperty);
            Assert.AreEqual(11, fooDto.Bar.BarProperty);
            Assert.AreEqual(2, fooDto.Bar.Foo.FooProperty);
            Assert.IsNull(fooDto.Bar.Foo.Bar);
        }

        [Test]
        public void ShouldMapWhenCircularDependency4()
        {
            var mapper = TestHelper.CreateMapper(b =>
            {
                b.CreateMap<Bar, BarDto>().MapAll();
                b.CreateMap<Foo, FooDto>().MapAll();
            });

            var bar = new Bar() { BarProperty = 1, Foo = new Foo() { FooProperty = 11, Bar = new Bar() { BarProperty = 2 } } };
            var barDto = mapper.Map<BarDto>(bar);

            Assert.AreEqual(1, barDto.BarProperty);
            Assert.AreEqual(11, barDto.Foo.FooProperty);
            Assert.AreEqual(2, barDto.Foo.Bar.BarProperty);
            Assert.IsNull(barDto.Foo.Bar.Foo);
        }



    }
}
