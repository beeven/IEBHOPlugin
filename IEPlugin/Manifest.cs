using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.IO;

namespace IEPlugin
{
    public class Manifest
    {
        public Manifest() { }
        public Manifest(string manifestJsonLocation)
        {
            if(!File.Exists(manifestJsonLocation))
            {
                return;
            }
            var manifestJson = File.ReadAllText(manifestJsonLocation);
            var basePath = Path.GetDirectoryName(manifestJsonLocation);
            var manifestRoot = JObject.Parse(manifestJson);
            AllowedOrigins = manifestRoot["allowed_origins"].Values<string>().ToArray();

            var scriptFiles = manifestRoot["scripts"].Values<string>().ToArray();
            List<string> scripts = new List<string>();
            foreach(var f in scriptFiles)
            {
                var location = Path.Combine(basePath, f);
                if (File.Exists(location))
                {
                    scripts.Add(File.ReadAllText(location));
                }
            }
            Scripts = scripts.ToArray();

            var styleSheetFiles = manifestRoot["stylesheets"].Values<string>().ToArray();
            List<string> css = new List<string>();
            foreach(var f in styleSheetFiles)
            {
                var location = Path.Combine(basePath, f);

                
                if (File.Exists(location))
                {
                    css.Add(File.ReadAllText(location));
                }
            }
            StyleSheets = css.ToArray();

            var htmlFiles = manifestRoot["htmls"].Values<string>().ToArray();
            List<string> htmls = new List<string>();
            foreach(var f in htmlFiles)
            {
                var location = Path.Combine(basePath, f);

                if (File.Exists(location))
                {
                    htmls.Add(File.ReadAllText(location));
                }
            }
            Htmls = htmls.ToArray();
        }

        public string[] AllowedOrigins { get; set; }
        public string[] Scripts { get; set; }
        public string[] Htmls { get; set; }
        public string[] StyleSheets { get; set; }
    }
}
