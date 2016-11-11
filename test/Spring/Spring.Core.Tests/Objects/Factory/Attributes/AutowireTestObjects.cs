using System.Collections.Generic;

using Spring.Objects.Factory.Attributes.ByType;

namespace Spring.Objects.Factory.Attributes.ByType
{
    public interface IFoo
    {
        string Say();
    }

    public class HelloFoo : IFoo
    {
        public string Say()
        {
            return "hello";
        }
    }

    public class CiaoFoo : IFoo
    {
        public string Say()
        {
            return "ciao";
        }
    }

    public class AutowireTestFieldNormal
    {
        [Autowired]
        public IFoo hello;
    }

    public class AutowireTestPropertyNormal
    {
        [Autowired]
        public IFoo Hello { get; set; }
    }

    public class AutowireTestMethodNormal
    {
        public IFoo hello;

        [Autowired]
        private void Prepare(IFoo hello)
        {
            this.hello = hello;
        }
    }

    public class AutowireTestConstructorNormal
    {
        public IFoo hello;

        [Autowired]
        public AutowireTestConstructorNormal(IFoo hello)
        {
            this.hello = hello;
        }
    }

    public class AutowireTestFieldNotRequired
    {
        [Autowired(Required = false)]
        public IFoo hello;
    }

    public class AutowireTestPropertyNotRequired
    {
        [Autowired(Required = false)]
        public IFoo Hello { get; set; }
    }

    public class AutowireTestMethodNotRequired
    {
        public IFoo hello;

        [Autowired(Required = false)]
        private void Prepare(IFoo hello)
        {
            this.hello = hello;
        }
    }

}

namespace Spring.Objects.Factory.Attributes.ByQualifier
{
    public class AutowireTestFieldNormal
    {
        [Autowired]
        [Qualifier("ciao")]
        public IFoo ciao;
    }

    public class AutowireTestPropertyNormal
    {
        [Autowired]
        [Qualifier("ciao")]
        public IFoo Ciao { get; set; }
    }

    public class AutowireTestMethodNormal
    {
        public IFoo ciao;

        [Autowired]
        private void Prepare([Qualifier("ciao")] IFoo ciao)
        {
            this.ciao = ciao;
        }
    }

    public class AutowireTestConstructorNormal
    {
        public IFoo ciao;

        [Autowired]
        public AutowireTestConstructorNormal([Qualifier("ciao")] IFoo ciao)
        {
            this.ciao = ciao;
        }
    }
}

namespace Spring.Objects.Factory.Attributes.ByQualifierAttribute
{
    public class DialectAttribute : QualifierAttribute
    {
        private string _language = "";

        public string Language { get { return _language; } set { _language = value; } }
    }

    public class AutowireTestFieldNormal
    {
        [Autowired]
        [Dialect(Language = "Italian")]
        public IFoo ciao;
    }

    public class AutowireTestPropertyNormal
    {
        [Autowired]
        [Dialect(Language = "Italian")]
        public IFoo Ciao { get; set; }
    }

    public class AutowireTestMethodNormal
    {
        public IFoo ciao;

        [Autowired]
        private void Prepare([Dialect(Language = "Italian")] IFoo ciao)
        {
            this.ciao = ciao;
        }
    }

    public class AutowireTestConstructorNormal
    {
        public IFoo ciao;

        [Autowired]
        public AutowireTestConstructorNormal([Dialect(Language = "Italian")] IFoo ciao)
        {
            this.ciao = ciao;
        }
    }
}

namespace Spring.Objects.Factory.Attributes.ByValue
{
    public class AutowireTestFieldNormal
    {
        [Value("@(CiaoFoo)")]
        public IFoo ciao;
    }

    public class AutowireTestPropertyNormal
    {
        [Value("@(CiaoFoo)")]
        public IFoo Ciao { get; set; }
    }

    public class AutowireTestMethodNormal
    {
        public IFoo ciao;

        [Autowired]
        private void Prepare([Value("@(CiaoFoo)")] IFoo ciao)
        {
            this.ciao = ciao;
        }
    }

    public class AutowireTestConstructorNormal
    {
        public IFoo ciao;

        [Autowired]
        public AutowireTestConstructorNormal([Value("@(CiaoFoo)")] IFoo ciao)
        {
            this.ciao = ciao;
        }
    }

    public class AutowireTestPropertyPlaceHolder
    {
        [Value("${greeting}")]
        public string greeting;
    }

}

namespace Spring.Objects.Factory.Attributes.Collections
{
    public class AutowireTestList
    {
        [Autowired]
        public IList<IFoo> foos;
    }
    
    public class AutowireTestSet
    {
        [Autowired]
        public Spring.Collections.Generic.ISet<IFoo> foos;        
    }

    public class AutowireTestDictionary
    {
        [Autowired]
        public IDictionary<string, IFoo> foos;
    }

    public class AutowireTestDictionaryFail
    {
        [Autowired]
        public IDictionary<IFoo, IFoo> foos;
    }

    public class AutowireTestQualifier
    {
        [Autowired]
        [Qualifier("ciao")]
        public IList<IFoo> foos;        
    }


}