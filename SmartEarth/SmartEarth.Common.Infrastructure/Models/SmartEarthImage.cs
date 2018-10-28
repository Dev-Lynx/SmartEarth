using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;

namespace SmartEarth.Common.Infrastructure.Models
{
    public class SmartEarthImage : BindableBase
    {
        #region Properties

        #region Statics
        public const int STRETCH_THUMBNAIL_WIDTH = 2000;
        public const int STRECTH_THUMBNAIL_HEIGHT = 400;
        public const int SQUARE_THUMBNAIL_WIDTH = 200;
        public const int SQUARE_THUMBNAIL_HEIGHT = 200;
        #endregion

        #region Bindables
        [XmlIgnore]
        [LiteDB.BsonIgnore]
        public bool IsLoaded { get; private set; }

        BitmapSource _thumb = null;
        [XmlIgnore]
        [LiteDB.BsonIgnore]
        public BitmapSource Thumb { get => _thumb; set => SetProperty(ref _thumb, value); }

        BitmapSource _source = null;
        [XmlIgnore]
        [LiteDB.BsonIgnore]
        public BitmapSource Source { get => _source; set => SetProperty(ref _source, value); }

        public string Path { get; set; }
        #endregion

        #endregion

        #region Constructors
        public SmartEarthImage() { }
        public SmartEarthImage(string path) { Path = path; }
        #endregion

        #region Methods
        public void Load(bool crop = false, Int32Rect rect = default(Int32Rect))
        {
            IsLoaded = true;   
            BitmapImage bitmap = new BitmapImage();           
            try
            {
                using (var stream = new FileStream(Path, FileMode.Open, FileAccess.Read))
                {
                    bitmap.BeginInit();
                    bitmap.StreamSource = stream;
                    bitmap.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                }

                if (bitmap.Width < rect.Width) rect.Width = (int)bitmap.Width;
                if (bitmap.Height < rect.Height) rect.Height = (int)bitmap.Height;

                if (crop) Thumb = new CroppedBitmap(bitmap, rect);
                else Source = bitmap;
            }
            catch(Exception ex)
            {
                Core.Log.Error("An error occured while loading an image ({0}, {1} - {2}, {3} at ({4}))\n{5}", rect.X, rect.Y, rect.Width, rect.Height, Path, ex);
                Source = null;
                //Image = 
            }
        }

        public void UnloadSource() => Source = (BitmapImage)Application.Current.Resources["ImageLoadFailed"];

        public void UnloadThumb() => Thumb = (BitmapImage)Application.Current.Resources["ImageLoadFailed"];

        public void Reset()
        {
            IsLoaded = false;

            UnloadSource();
            UnloadThumb();
        }
        #endregion
    }
}
