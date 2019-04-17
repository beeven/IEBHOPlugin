using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
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

            InternetExplorer explorer = pDisp as InternetExplorer;

            HTMLDocument document = explorer.Document;
            HTMLHeadElement head = document.all.tags("head").item(null, 0);

            IHTMLScriptElement scriptObject = (IHTMLScriptElement)document.createElement("script");
            scriptObject.type = "text/javascript";
            scriptObject.text = "\nfunction hidediv(){document.getElementById('myOwnDiv').style.visibility='hidden';}";

            head.appendChild((IHTMLDOMNode)scriptObject);
            string div = "<div id='myOwnDiv' style='position:absolute;top:0;left:0;width:100px;height:50px;z-index:9999;'>" +
                         "<div>hello world!</div>" +
                         "<a href='javascript:hidediv();'>close</a>" +
                         "</div>";
            document.body.insertAdjacentHTML("afterBegin", div);
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
