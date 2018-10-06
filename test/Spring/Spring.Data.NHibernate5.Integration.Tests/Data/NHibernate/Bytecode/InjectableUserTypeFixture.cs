using NHibernate.Bytecode;

using NUnit.Framework;

using Spring.Context;
using Spring.Context.Support;
using Spring.Objects.Factory.Config;
using Spring.Objects.Factory.Support;

namespace Spring.Data.NHibernate.Bytecode
{
    [TestFixture, Explicit("requires database")]
    public class InjectableUserTypeFixture : AbstractInjectableUserTypeFixture
    {
        private IConfigurableListableObjectFactory objectFactory;
        private static readonly IObjectDefinitionFactory objectDefinitionFactory = new DefaultObjectDefinitionFactory();

        protected override void InitializeServiceLocator()
        {
            IConfigurableApplicationContext context = new StaticApplicationContext();
            objectFactory = context.ObjectFactory;

            Register<IDelimiter, ParenDelimiter>(objectFactory);
            RegisterPrototype<InjectableStringUserType>(objectFactory);
        }

        protected override IBytecodeProvider GetBytecodeProvider()
        {
            return new BytecodeProvider(objectFactory);
        }

        private static void Register<TSerivice, TImplementation>(IConfigurableListableObjectFactory confObjFactory)
        {
            ObjectDefinitionBuilder odb = ObjectDefinitionBuilder.RootObjectDefinition(objectDefinitionFactory, typeof(TImplementation)).
                SetAutowireMode(AutoWiringMode.Constructor);
            confObjFactory.RegisterObjectDefinition(typeof (TSerivice).FullName, odb.ObjectDefinition);
        }

        private static void RegisterPrototype<TImplementation>(IConfigurableListableObjectFactory confObjFactory)
		{
            ObjectDefinitionBuilder odb = ObjectDefinitionBuilder.RootObjectDefinition(objectDefinitionFactory, typeof(TImplementation))
				.SetSingleton(false).SetAutowireMode(AutoWiringMode.AutoDetect);
			confObjFactory.RegisterObjectDefinition(typeof(TImplementation).FullName, odb.ObjectDefinition);
		}
    }
}
