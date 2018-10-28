using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartEarth.Common.Infrastructure.Resources.Interfaces
{
    public interface IShellAware
    {
        IWindow Window { get; }
        void Load(IWindow window);
    }
}
