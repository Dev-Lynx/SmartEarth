using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartEarth.Common.Infrastructure.Models.Interfaces
{
    public interface IPresentable
    {
        Guid Signature { get; }
        DateTime Due { get; }
        string Name { get; }
        bool DescriptionHidden { get; }
        string MiniDescription { get; }
        string Description { get; }
        ColorBox Color { get; }
        

        IPresentable Replicate();
        IPresentable ReplicateDue(DateTime newDue);
    }
}