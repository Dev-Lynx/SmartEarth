using System;
using System.Collections.Generic;
using SmartEarth.Common.Infrastructure.Models;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartEarth.Common.Infrastructure.Services.Interfaces
{
    public interface IConfigurationManager
    { 
        Configuration Configuration { get; }
        bool SaveConfiguration(Configuration configuration = null);
    }
}
