using System;
using System.ComponentModel;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using System.Text;


namespace SiteMap.SiteMapWebPart
{
    [ToolboxItemAttribute(false)]
    public class SiteMapWebPart : WebPart
    {
        private bool ignoreSecurity;
        private string siteUrl;

        [DefaultValue(false),
        WebBrowsable(true),
        Personalizable(PersonalizationScope.Shared),
        WebDisplayName("Ignore user's permissons"),
        WebDescription("Check this to run as the farm account"),       
        Category("Site Map Configuration")]
        public bool IgnoreSecurity
        {
            get { return ignoreSecurity; }
            set { ignoreSecurity = value; }
        }

        [DefaultValue("http://sp2013"),
        WebBrowsable(true),
        Personalizable(PersonalizationScope.Shared),
        WebDisplayName("Site URL"),
        WebDescription("Displays the site map for this URL"),
        Category("Site Map Configuration")]
        public string SiteUrl
        {
            get { return siteUrl; }
            set { siteUrl = value; }
        }

        protected override void CreateChildControls()
        {
            
        }

        protected override void RenderChildren(HtmlTextWriter writer)
        {
            base.RenderChildren(writer);

            if (!string.IsNullOrEmpty(siteUrl))
            {
                if (ignoreSecurity)
                {
                    SPSecurity.RunWithElevatedPrivileges(delegate() {           //without this- access denied
                    using (SPSite site = new SPSite(siteUrl))
                    {
                        using (SPWeb web = site.OpenWeb())
                        {
                            PrintMap(web, ref writer);                            
                        }
                        
                    }
                    });
                }
                else
                {
                    using (SPSite site = new SPSite(siteUrl))
                    {
                        using (SPWeb web = site.OpenWeb())
                        {
                            PrintUserMap(web, ref writer);                            
                            //SPWebCollection subwebs = web.GetSubwebsForCurrentUser();                      
                        }

                    }

                    //SPSite tmpsite = new SPSite(siteUrl);                    
                    //SPWeb tmpweb = tmpsite.OpenWeb();
                    //Guid siteID = tmpsite.ID;
                    //Guid webID = tmpweb.ID;
                    //tmpweb.Dispose();
                    //tmpsite.Dispose();

                    //Guid webID = SPContext.Current.Web.ID;
                    //Guid siteID = SPContext.Current.Site.ID;

                    //SPSecurity.RunWithElevatedPrivileges(delegate()
                    //{
                    //    using (SPSite site = new SPSite(siteID))
                    //    {
                    //        using (SPWeb web = site.AllWebs[webID])
                    //        {
                    //            PrintMap(web, ref writer);
                    //        }
                    //    }
                    //});
                }
            }
            else
            {
                writer.RenderBeginTag(HtmlTextWriterTag.H2);
                writer.Write("Configure the site URL to display its site map.");
                writer.RenderEndTag();
            }
        }

        private void PrintMap(SPWeb web, ref HtmlTextWriter output)
        {
            output.RenderBeginTag(HtmlTextWriterTag.Ul);

            output.RenderBeginTag(HtmlTextWriterTag.Li);

            output.Write(string.Format("{0} ({1})", web.Title, PrintAdmins(web.SiteAdministrators)));
            output.RenderEndTag();  //li

            foreach (SPWeb sub in web.Webs)
            {
                PrintMap(sub, ref output);
            }

            output.RenderEndTag();  //ul
         
        }

        private void PrintUserMap(SPWeb web, ref HtmlTextWriter output)
        {
            output.RenderBeginTag(HtmlTextWriterTag.Ul);

            output.RenderBeginTag(HtmlTextWriterTag.Li);
            output.Write(web.Title);
            output.RenderEndTag();  //li

            foreach (SPWeb sub in web.GetSubwebsForCurrentUser())
            {
                PrintUserMap(sub, ref output);
            }

            output.RenderEndTag();  //ul

        }

        private string PrintAdmins(SPUserCollection admins)
        {
            StringBuilder s = new StringBuilder();

            for (int i = 0; i < admins.Count; i++)
            {
                s.Append(string.Format("{0}", admins[i].Name));

                if (i != admins.Count - 1)
                {
                    s.Append(", ");
                }
            }            

            return s.ToString();
        }

    }
}
