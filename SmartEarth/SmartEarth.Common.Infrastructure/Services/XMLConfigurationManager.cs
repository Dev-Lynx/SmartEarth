using Prism.Logging;
using SmartEarth.Common.Infrastructure.Models;
using SmartEarth.Common.Infrastructure.Extensions;
using SmartEarth.Common.Infrastructure.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Prism.Mvvm;

namespace SmartEarth.Common.Infrastructure.Services
{
    public class XMLConfigurationManager : BindableBase, IConfigurationManager
    {
        #region Properties
        public Configuration Configuration { get; private set; }

        #region Services
        ILoggerFacade Logger { get; }
        #endregion

        #endregion

        #region Constructors
        public XMLConfigurationManager(ILoggerFacade logger)
        {
            Logger = logger;
            LoadConfiguration();
        }

        #endregion

        #region Methods

        #region IConfigurationManager Implementation
        public bool SaveConfiguration(Configuration configuration = null)
        {
            try
            {
                using (var stream = new StreamWriter(Core.CONFIGURAION_PATH))
                    new XmlSerializer(typeof(Configuration)).Serialize(stream, configuration);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error("An error occured while attempting to save configuration. \n {0}", ex);
                return false;
            }
        }
        #endregion

        void LoadConfiguration()
        {
            Configuration configuration = null;
            try
            {
                using (var stream = new FileStream(Core.CONFIGURAION_PATH, FileMode.Open, FileAccess.Read))
                    configuration = (Configuration)new XmlSerializer(typeof(Configuration)).Deserialize(stream);

                Configuration = new Configuration(configuration);
            }
            catch (Exception ex)
            {
                Logger.Error("An error occured while attempting to load configuration...\n{0}", ex);
                Configuration = new Configuration(Configuration.DefaultConfiguration);
            }
        }

        #endregion
    }
}
