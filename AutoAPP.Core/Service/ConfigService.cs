using AutoAPP.Core.Service.Client;
using AutoAPP.Core.Service.Interface;
using AutoShared.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoAPP.Core.Service
{
    public class ConfigService(HttpRestClient client) : BaseService<ConfigDto>(client, "Config"), IConfigService
    {
    }
}
