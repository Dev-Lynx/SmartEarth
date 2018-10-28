using Prism.Mvvm;
using SmartEarth.Common.Infrastructure.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartEarth.Common.Infrastructure.Models
{
    public class ScheduleTask : BindableBase, IPresentable
    {
        #region Properties

        #region Statics
        const int MaxDescription = 50;
        #endregion

        public Guid Signature { get; set; }

        string _name = string.Empty;
        public string Name { get => _name; set => SetProperty(ref _name, value); }

        public bool DescriptionHidden => Description.Length > MaxDescription;

        public string MiniDescription
        {
            get
            {
                try { return Description.Substring(0, MaxDescription) + "...."; }
                catch { return string.Empty; }
            }
        }

        string _description = string.Empty;
        public string Description
        {
            get => _description;
            set
            {
                SetProperty(ref _description, value);
                RaisePropertyChanged(nameof(MiniDescription));
            }
        }

        ColorBox _color = null;
        public ColorBox Color { get => _color; set => SetProperty(ref _color, value); }

        DateTime _due;
        public DateTime Due { get => _due; set => SetProperty(ref _due, value); }
        #endregion

        #region Constructors
        public ScheduleTask() { }
        public ScheduleTask(Guid signature, string name, string description, ColorBox color, DateTime due)
        {
            Signature = signature;
            Name = name;
            Description = description;
            Color = color;
            Due = due;
        }

        public ScheduleTask(IPresentable presentable)
        {
            Signature = presentable.Signature;
            Name = presentable.Name;
            Description = presentable.Description;
            Color = presentable.Color;
            Due = presentable.Due;
        }
        #endregion

        #region Methods

        #region IPresentable Implemantation
        public IPresentable Replicate() => new ScheduleTask(this);
        public IPresentable ReplicateDue(DateTime due) => new ScheduleTask(this) { Due = due };
        #endregion

        #endregion
    }
}
