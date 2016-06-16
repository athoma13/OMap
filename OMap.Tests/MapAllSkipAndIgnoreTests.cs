using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace OMap.Tests
{
    [TestFixture]
    public class MapAllSkipAndIgnoreTests
    {
        public class Foo
        {
            public string Property1 { get; set; }
            public string Property2 { get; set; }
        }

        public class FooDto
        {
            public string Property1 { get; set; }

            private string _property2;
            public string Property2
            {
                get { return _property2; }
                set
                {
                    _isBroken = _isBroken || value == "break";
                    _property2 = value;
                }
            }

            private bool _isBroken = false;
            public bool IsBroken() { return _isBroken;}

        }

        public class FooDtoMissing
        {
            public string Property1 { get; set; }
            public string Property3 { get; set; }
        }

        [Test]
        public void MapAllShouldIgnoreWhenExplictlyMarkedAsIgnore()
        {
            var mapper = TestHelper.CreateMapper(b =>
            {
                b.CreateMap<Foo, FooDto>()
                    .MapAll()
                    .Ignore(x => x.Property2);
            });

            var foo = new Foo() {Property1 = "Hello", Property2 = "break"};
            var fooDto = mapper.Map<FooDto>(foo);
            Assert.AreEqual("Hello", fooDto.Property1);
            Assert.IsNull(fooDto.Property2);
            Assert.IsFalse(fooDto.IsBroken(), "Is broken flag should be false");
        }

        [Test]
        public void MapAllShouldCauseExceptionInMapping()
        {
            var mapper = TestHelper.CreateMapper(b =>
            {
                b.CreateMap<Foo, FooDto>()
                    .MapAll();
            });

            var foo = new Foo() { Property1 = "Hello", Property2 = "break" };
            var fooDto = mapper.Map<FooDto>(foo);
            Assert.IsTrue(fooDto.IsBroken(), "Is broken flag should be true");
        }



        [Test]
        public void MapAllShouldSkipWhenMapPropertyDefined()
        {
            var mapper = TestHelper.CreateMapper(b =>
            {
                b.CreateMap<Foo, FooDto>()
                    .MapAll()
                    .MapProperty(x => "Don't " + x.Property2, x => x.Property2);
            });

            var foo = new Foo() { Property1 = "Hello", Property2 = "break" };
            var fooDto = mapper.Map<FooDto>(foo);
            Assert.AreEqual("Hello", fooDto.Property1);
            Assert.AreEqual("Don't break", fooDto.Property2);
            Assert.IsFalse(fooDto.IsBroken(), "Is broken flag should be false");

        }

        [Test]
        [ExpectedException(typeof(MappingException))]
        public void MapAllShouldThrowWhenMissingMembers()
        {
            var mapper = TestHelper.CreateMapper(b =>
            {
                b.CreateMap<Foo, FooDtoMissing>()
                    .MapAll();
            });

            var foo = new Foo() { Property1 = "Hello", Property2 = "break" };
            var fooDto = mapper.Map<FooDto>(foo);

        }


    }
}
