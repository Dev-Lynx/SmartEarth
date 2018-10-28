using Prism.Mvvm;
using SmartEarth.Common.Infrastructure.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media;
using System.Xml.Serialization;

namespace SmartEarth.Common.Infrastructure.Models
{
    public class ColorBox : BindableBase
    {
        #region Properties
        public int Id { get; set; }
        public string Name { get; set; }
        public Color Color { get; set; }
        public bool IsDark => Brightness < 60;
        public int Brightness => (int)Math.Sqrt((Color.R * Color.R * .241) + (Color.G * Color.G * .691) + (Color.B * Color.B * .068));
        
        [XmlIgnore]
        [LiteDB.BsonIgnore]
        public SolidColorBrush Brush => new SolidColorBrush(Color);
        #endregion

        #region Constructors
        public ColorBox()
        {
            //Color.GetBrigtness
            //System.Windows.Drawing
            //ControlPaint.Light(Color);
        }

        public ColorBox(string name, Color color)
        {
            Name = name;
            Color = color;
        }

        public ColorBox(string name, string color) : this(name, color.ToSolidBrush().Color) { }
        #endregion

        #region Methods
        #endregion
    }
}
