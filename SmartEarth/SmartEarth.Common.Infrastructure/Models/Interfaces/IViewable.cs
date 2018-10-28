using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SmartEarth.Common.Infrastructure.Models.Interfaces
{
    public interface IViewable : IPresentable
    {
        bool Expanded { get; set; }
        SmartEarthImage Image { get; }

        Task Load(bool crop = false, Int32Rect rect = default(Int32Rect));
        Task UnloadSource();
        Task UnloadThumb();
        Task Reset();
    }
}
