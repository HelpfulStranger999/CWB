using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CWBDrone.Tools
{
    public static class DependencyInjection
    {
        public static T CreateInjected<T>(IServiceProvider services, params object[] dependencies)
            => CreateInjected<T>(typeof(T), services, dependencies);

        public static T CreateInjected<T>(Type type, IServiceProvider services, params object[] dependencies)
        {
            var constructor = type.GetConstructors().First() ?? type.GetConstructor(Type.EmptyTypes);
            var parameters = constructor.GetParameters();
            var args = new object[parameters.Count()];

            for (int i = 0; i < args.Count(); i++)
            {
                var param = parameters[i];
                if (param.ParameterType.GetInterfaces().Contains(typeof(IServiceProvider)))
                {
                    args[i] = services;
                    continue;
                }

                var dependency = dependencies.Where(o => param.Extends(o.GetType()));
                if (dependency.Count() > 0)
                {
                    args[i] = dependency.Take(1);
                    continue;
                }

                args[i] = services.GetRequiredService(param.ParameterType);
            }

            return Inject((T)constructor.Invoke(args), services, dependencies);
        }

        public static T Inject<T>(T obj, IServiceProvider services, params object[] dependencies)
        {
            var properties = obj.GetType().GetProperties();

            foreach (var property in properties)
            {
                if (property.Extends(services.GetType()))
                {
                    property.SetValue(obj, services);
                    continue;
                }

                var dependency = dependencies.Where(o => property.Extends(o.GetType()));
                if (dependency.Count() > 0)
                {
                    property.SetValue(obj, dependency.Take(1));
                    continue;
                }

                property.SetValue(obj, services.GetRequiredService(property.PropertyType));

            }

            return obj;
        }
    }
}
