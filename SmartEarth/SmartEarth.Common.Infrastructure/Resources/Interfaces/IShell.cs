using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SmartEarth.Common.Infrastructure.Resources.Interfaces
{
    public interface IShell : IWindow
    {
        event EventHandler StateChanged;
    }
}
