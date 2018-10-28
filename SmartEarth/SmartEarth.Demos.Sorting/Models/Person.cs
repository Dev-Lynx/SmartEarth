using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartEarth.Demos.Sorting
{
    public class Person : BindableBase
    {
        #region Properties
        int _id = 0;
        public int Id { get => _id; set => SetProperty(ref _id, value); }

        string _name;
        public string Name { get => _name; set => SetProperty(ref _name, value); }

        double _worth;
        public double Worth { get => _worth; set => SetProperty(ref _worth, value); }
        #endregion

    }
}
