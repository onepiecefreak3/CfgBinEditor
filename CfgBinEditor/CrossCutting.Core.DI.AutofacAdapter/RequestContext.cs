using Autofac;
using Autofac.Core;
using Autofac.Core.Resolving.Pipeline;
using CrossCutting.Core.Contract.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossCutting.Core.DI.AutofacAdapter
{
    public class RequestContext : IRequestContext
    {
        private readonly ResolveRequestContext _context;

        public RequestContext(ResolveRequestContext context)
        {
            _context = context;
        }

        public void ChangeParameters(Dictionary<string, object> parameter)
        {
            _context.ChangeParameters(parameter.Select(p => new NamedParameter(p.Key, p.Value)));
        }
    }
}
