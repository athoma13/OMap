using System;
using System.Runtime.InteropServices.ComTypes;

namespace ObjectMapper
{
    public class Test
    {
        public class Foo
        {
            public int Prope1 { get; set; }
        }
        public class FooX : Foo
        {
            public int Prope2 { get; set; }
        }


        public class Bar
        {
            public int Prope3 { get; set; }
        }

        public class BarX : Bar
        {
            public int Prope4 { get; set; }
        }


        public void Do()
        {
            var builder = new ConfigurationBuilder();

            builder.CreateMap<Foo, Bar>()
                .WithDependencies<IConnectionPoint, IComparable>()
                .MapProperty((x, d) => x.Prope1 + 1, x => x.Prope3);

            builder.CreateMap<FooX, BarX>()
                .MapProperty(x => x.Prope2 + 10, x => x.Prope4);



            var config = builder.Build();

            var factory = new MappingFactory(new ResolverMock(), config);
            var mapper = factory.CreateMapper<Foo, Bar>();

            var foo = new FooX() { Prope1 = 18, Prope2 = 6 };
            var bar = mapper.Map(foo);




        }
    }
}