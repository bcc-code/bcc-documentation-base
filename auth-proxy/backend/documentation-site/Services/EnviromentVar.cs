namespace BccCode.DocumentationSite.Services
{
    public class EnviromentVar
    {
        public EnviromentVar(IConfiguration config)
        {
            this.config = config;
        }

        IConfiguration config;

        public string GetEnviromentVariable(string varName)
        {
            return !string.IsNullOrEmpty(Environment.GetEnvironmentVariable(varName)) ? Environment.GetEnvironmentVariable(varName)! : config[$"AppSettings:{varName}"];
        }
    }
}
