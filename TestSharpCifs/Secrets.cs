using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestSharpCifs
{
    public class Secrets
    {
        //Singleton実装
        private static Dictionary<string, object> _instance = null;
        public static Dictionary<string, object> Instance
        {
            get
            {
                if (Secrets._instance == null)
                    Secrets.Create();

                return Secrets._instance;
            }
        }

        public static Dictionary<string, object> Create()
        {
            try
            {
                if (Secrets._instance == null)
                {
                    var result = new Dictionary<string, object>();

                    if (System.IO.File.Exists(SecretsJsonPath))
                    {
                        var bytes = System.IO.File.ReadAllBytes(SecretsJsonPath);
                        var json = Encoding.UTF8.GetString(bytes);
                        result = Xb.Type.Json.Parse(json);
                    }
                    Secrets._instance = result;
                }
                
                return Secrets._instance;
            }
            catch (Exception ex)
            {
                Xb.Util.Out(ex);
                throw;
            }
        }

        private const string SecretsJsonFileName = "secrets.json";

        private static readonly string SecretsJsonPath
            = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(),
                                     SecretsJsonFileName);

        public static bool HasSecrets => Secrets.Instance.Any();

        public static bool ContainsKey(string key)
        {
            return Secrets.Instance.ContainsKey(key);
        }

        public static string Get(string key)
        {
            if (!Secrets.Instance.ContainsKey(key))
                return string.Empty;

            return Secrets.Instance[key]?.ToString() ?? string.Empty;
        }
    }
}