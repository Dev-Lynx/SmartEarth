﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartEarth.Common.Infrastructure.Resources.Interfaces
{
    public interface INavigationTab
    {
        string View { get; set; }
        string Region { get; set; }
    }
}
