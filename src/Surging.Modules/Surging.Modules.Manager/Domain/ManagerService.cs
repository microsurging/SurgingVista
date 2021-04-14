using System.Collections.Generic;
using System.Threading.Tasks;
using Surging.Core.CPlatform.Utilities;
using Surging.Core.ProxyGenerator;
using Surging.IModuleServices.Common;
using Surging.IModuleServices.Common.Models;
using Surging.IModuleServices.Manager; 

namespace Surging.Modules.Manager.Domain
{
    public class ManagerService : ProxyServiceBase, IManagerService
    {
        private readonly IServiceProxyProvider _serviceProxyProvider;
        public ManagerService(IServiceProxyProvider serviceProxyProvider)
        {
            _serviceProxyProvider = serviceProxyProvider;
        }

        public async Task<string> SayHello(string name)
        {
            //GetService<IUserService>("User").GetUserId("fanly");
            Dictionary<string, object> model = new Dictionary<string, object>();
            model.Add("name", name);
            string path = "api/hello/say"; 

            string result =await _serviceProxyProvider.Invoke<string>(model, path, null);
            return result;
        }

        public async Task<string> Say(string name)
        {
            var d= await GetService<IUserService>("User").GetUser(new UserModel { Name=name, Age=1,Sex=Sex.Man });
            return await Task.FromResult($"{name}: say hello");
        }

        public Task<bool> Test(string test, string ToList, string DomainID)
        {
            return Task.FromResult(true);
        }
    }
}
