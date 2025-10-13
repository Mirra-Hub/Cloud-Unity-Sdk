namespace MirraCloud
{
    public abstract class BaseService
    {
        protected abstract string ControllerApi { get; }
        protected readonly Configuration Configuration;

        protected BaseService(Configuration configuration)
        {
            Configuration = configuration;
        }
        
        protected string GetUrl(string router)
        {
            return Configuration.CreateUrlApi(ControllerApi + router);
        }
    }
}