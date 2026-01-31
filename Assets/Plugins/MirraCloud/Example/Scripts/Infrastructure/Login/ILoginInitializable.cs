using System.Threading.Tasks;

namespace Plugins.MirraCloud.Example.Scripts.Infrastructure.Lobby
{
    public interface ILoginInitializable
    {
        Task<bool> Initialize();
    }
}