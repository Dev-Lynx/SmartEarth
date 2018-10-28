using SmartEarth.Common.Infrastructure.Models;
using SmartEarth.Common.Infrastructure.Resources.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartEarth.Common.Infrastructure.Services.Interfaces
{
    public interface IWindowService
    {
        List<IWindow> Windows { get; }
        Task<IWindow> CreateWindow<T>(WindowCreationSettings settings) where T : class;
        IWindow CreateWindow(WindowCreationSettings settings);
        void DestroyWindow(IWindow window);
    }
}
