using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartEarth.Common.Infrastructure.Models.Interfaces
{
    public interface IWaitable
    {
        bool IsBusy { get; }
        string Status { get; }
    }
}
