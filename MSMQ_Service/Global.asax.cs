using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using log4net;
using System.Configuration;

namespace MSMQ_RFService
{
    public class Global : System.Web.HttpApplication
    {
        private readonly ILog log = LogManager.GetLogger(typeof(MsmqService));
        protected void Application_Start(object sender, EventArgs e)
        {
            log4net.Config.XmlConfigurator.Configure();

            if (InitailContext._dataConfigSetting == null)
            {
                InitailContext._dataConfigSetting = System.Configuration.ConfigurationManager.GetSection("DataConfigSettings") as DataConfigSettings;
            }

            if (InitailContext._appXsd == null)
            {
                InitailContext._appXsd = new Dictionary<string, string>();
                InitailContext.time = !string.IsNullOrEmpty(ConfigurationManager.AppSettings["Time"].ToString()) ? Convert.ToInt32(ConfigurationManager.AppSettings["Time"].ToString()) : 3000;
                //InitailContext.queueTimeout = Convert.ToInt32(ConfigurationManager.AppSettings["TimeToReachQueue"].ToString());
                LoadApplicationSchema();
            }
        }

        protected void Session_Start(object sender, EventArgs e)
        {
            
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {

        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

        }

        protected void Application_Error(object sender, EventArgs e)
        {

        }

        protected void Session_End(object sender, EventArgs e)
        {

        }

        protected void Application_End(object sender, EventArgs e)
        {
            InitailContext.mq.Dispose();
            InitailContext.mq = null;
        }

        private bool LoadApplicationSchema()
        {
            try
            {
                log.Info("Inside Method");

                InitailContext._appXsd.Clear();
                foreach (AppConfigSetting setting in InitailContext._dataConfigSetting.AppConfigSettings)
                {
                    string xsdFile = System.Web.Hosting.HostingEnvironment.MapPath("~/XSD/IvrMainSchema.xsd"); ;
                    InitailContext._appXsd.Add(setting.ID, System.IO.File.ReadAllText(xsdFile));
                }
                log.InfoFormat("Read XSD is Successfull", InitailContext._appXsd);
                return true;
            }
            catch (Exception ex)
            {
                log.InfoFormat("Exception in Read XSD - {0}", ex);
                return false;
            }
        }
    }
}