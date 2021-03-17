using System.Threading.Tasks;

namespace Deploy
{
    public interface IDeployService
    {
        Task Deploy(int? num = null);
    }
}