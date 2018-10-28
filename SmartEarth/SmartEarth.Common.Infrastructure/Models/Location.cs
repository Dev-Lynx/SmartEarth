using CoordinateSharp;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SmartEarth.Common.Infrastructure.Models
{
    [Serializable]
    public class Location : BindableBase
    {
        #region Properties
        [XmlIgnore]
        [LiteDB.BsonIgnore]
        Coordinate _coordinates = new Coordinate();
        [XmlIgnore]
        [LiteDB.BsonIgnore]
        public Coordinate Coordinates
        {
            get => _coordinates;
            set
            {
                if (value != null) value.FormatOptions = Format;
                SetProperty(ref _coordinates, value);
            }
        }

        CoordinateFormatOptions Format => new CoordinateFormatOptions()
        {
            Position_First = false, Display_Trailing_Zeros = true
        };

        public string StringCoordinates
        {
            get
            {
                try { return Coordinates.ToString(Format); }
                catch { return string.Empty; } 
            }
            set
            {
                if (Coordinate.TryParse(value, out Coordinate coordinates))
                    Coordinates = coordinates;
            }
        }

        [XmlIgnore]
        [LiteDB.BsonIgnore]
        public string Latitude
        {
            get => Coordinates.Latitude.ToString(Format);
            set
            {
                if (!CoordinatePart.TryParse(value, CoordinateType.Lat, out CoordinatePart latitude)) return;
                Coordinates.Latitude = latitude;
                RaisePropertyChanged(nameof(Coordinates));  
            }
        }

        [XmlIgnore]
        [LiteDB.BsonIgnore]
        public string Longitude
        {
            get => Coordinates.Longitude.ToString(Format);
            set
            {
                if (!CoordinatePart.TryParse(value, CoordinateType.Long, out CoordinatePart longitude)) return;
                Coordinates.Longitude = longitude;
                RaisePropertyChanged(nameof(Coordinates));
            }
        }

        double _altitude; 
        public double Altitude { get => _altitude; set => SetProperty(ref _altitude, value); }

        double _range;
        public double Range { get => _range; set => SetProperty(ref _range, value); }

        DateTime _time;
        public DateTime Time { get => _time; set => SetProperty(ref _time, value); }

        Angle _tilt = new Angle();
        public Angle Tilt
        {
            get => _tilt;
            set
            {
                if (value == null) value = new Angle(0, 90, 0);
                value.Max = 90; value.Min = 0;
                SetProperty(ref _tilt, value);
            }
        }

        Angle _heading = new Angle();
        public Angle Heading
        {
            get => _heading;
            set
            {
                if (value == null) value = 0;
                value.Max = 360; value.Min = 0;
                SetProperty(ref _heading, value);
            }
        }

        string _name = string.Empty;
        public string Name { get => _name; set => SetProperty(ref _name, value); }

        string _description = string.Empty;
        public string Description { get => _description; set => SetProperty(ref _description, value); }
        #endregion

        #region Constructors
        public Location() {  }
        public Location(Location location)
        {
            Coordinates = new Coordinate(location.Coordinates.Latitude.ToDouble(), location.Coordinates.Longitude.ToDouble());
            Name = location.Name;
            Description = location.Description;
            Altitude = location.Altitude;
            Range = location.Range;
            Heading = new Angle(location.Heading);
            Tilt = new Angle(location.Tilt);
            Time = location.Time;
        }
        #endregion

        #region Methods

        #region Equity and Comparison
        static bool CompareLocations(Location a, Location b)
        {
            bool nullOne = ReferenceEquals(a, null);
            bool nullTwo = ReferenceEquals(b, null);

            if (nullOne && nullTwo) return true;
            else if (nullOne || nullTwo) return false;

            return a.Altitude == b.Altitude && a.Coordinates == b.Coordinates
                && a.Heading == b.Heading && a.Range == b.Range && a.Tilt == b.Tilt
                && a.Time == b.Time;
        }
        #endregion

        #region Overrides
        public override bool Equals(object obj)
        {
            if (!(obj is Location)) return false;
            return CompareLocations(this, (Location)obj);
        }

        public static bool operator ==(Location a, Location b) => CompareLocations(a, b);
        public static bool operator !=(Location a, Location b) => !CompareLocations(a, b);

        public override int GetHashCode()
        {
            var properties = new object[]
            {
                Coordinates, Altitude, Range, 
                Time, Tilt, Heading
            };

            unchecked
            {
                int hash = 17;
                for (int i = 0; i < properties.Length; i++)
                    if (ReferenceEquals(null, properties[i]))
                        hash = hash * 23 + properties[i].GetHashCode();
                return hash;
            }
        }
        #endregion

        public static bool TryParse(string value, out Location location)
        {
            location = new Location();
            if (!Coordinate.TryParse(value, out Coordinate c))
                return false;
            location.Coordinates = c;
            return true;
        }

        #endregion
    }
}
