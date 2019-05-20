using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Win32;
using mshtml;
using SHDocVw;

namespace IEPlugin
{
    [
        ComVisible(true),
        ClassInterface(ClassInterfaceType.None),
        Guid("C42D40F0-BEBF-418D-8EA1-18D99AC2AB17")
    ]
    public class BHOPlugin : IObjectWithSite
    {
        private InternetExplorer ieInstance;
        private const string BHORegistryKey = "Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Browser Helper Objects";

        private string assemblyLocation;
        private Manifest manifest;
        public BHOPlugin()
        {
            
            assemblyLocation = this.GetType().Assembly.Location;
            var manifestLocation = Path.Combine(Path.GetDirectoryName(this.assemblyLocation), "content", "manifest.json");
            manifest = new Manifest(manifestLocation);
        }


        [ComRegisterFunction]
        public static void RegisterBHO(Type t)
        {
            RegistryKey key = Registry.LocalMachine.OpenSubKey(BHORegistryKey, true);
            if(key == null)
            {
                key = Registry.LocalMachine.CreateSubKey(BHORegistryKey);
            }

            string bhoKeyStr = t.GUID.ToString("B");
            RegistryKey bhoKey = key.OpenSubKey(bhoKeyStr, true);
            if(bhoKey == null)
            {
                bhoKey = key.CreateSubKey(bhoKeyStr);
            }

            bhoKey.SetValue("NoExplorer", 1);
            key.Close();
            bhoKey.Close();
        }

        [ComUnregisterFunction]
        public static void UnregisterBHO(Type t)
        {
            RegistryKey key = Registry.LocalMachine.OpenSubKey(BHORegistryKey, true);
            string guidString = t.GUID.ToString("B");
            if (key != null)
            {
                key.DeleteSubKey(guidString, false);
            }
        }



        public void SetSite([In, MarshalAs(UnmanagedType.IUnknown)] object pUnkSite)
        {
            
            if (pUnkSite != null)
            {
                ieInstance = (InternetExplorer)pUnkSite;
                ieInstance.DocumentComplete += IeInstance_DocumentComplete;
            }
            else
            {
                ieInstance.DocumentComplete -= IeInstance_DocumentComplete;
                ieInstance = null;
            }
        }

        private void IeInstance_DocumentComplete(object pDisp, ref object URL)
        {
            string url = URL as string;

            if (string.IsNullOrEmpty(url)
                || url.Equals("about:blank", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            bool isOriginMatched = false;
            foreach(var origin in manifest.AllowedOrigins)
            {   
                if (Regex.IsMatch(url, origin))
                {
                    isOriginMatched = true;
                    break;
                }
            }

            if (!isOriginMatched)
            {
                return;
            }


            InternetExplorer explorer = pDisp as InternetExplorer;

            HTMLDocument document = explorer.Document;
            HTMLHeadElement head = document.all.tags("head").item(null, 0);

            foreach(var stylesheet in manifest.StyleSheets)
            {
                IHTMLStyleElement styleObject = (IHTMLStyleElement)document.createElement("style");
                styleObject.type = "text/css";
                styleObject.styleSheet.cssText = stylesheet;
                head.appendChild((IHTMLDOMNode)styleObject);
            }

            foreach(var script in manifest.Scripts)
            {
                IHTMLScriptElement scriptObject = (IHTMLScriptElement)document.createElement("script");
                scriptObject.type = "text/javascript";
                scriptObject.text = script;
                head.appendChild((IHTMLDOMNode)scriptObject);

            }

            foreach(var html in manifest.Htmls)
            {
                document.body.insertAdjacentHTML("beforeEnd", html);
            }
        }

        public void GetSite(ref Guid riid, [MarshalAs(UnmanagedType.IUnknown)] out IntPtr ppvSite)
        {
            IntPtr punk = Marshal.GetIUnknownForObject(ieInstance);
            int hr = Marshal.QueryInterface(punk, ref riid, out ppvSite);
            Marshal.ThrowExceptionForHR(hr);
            Marshal.Release(punk);
        }
    }
}
