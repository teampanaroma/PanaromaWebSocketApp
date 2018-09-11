namespace Panaroma.OKC.Integration.Library
{
    public interface IConfiguration
    {
        T GetConfiguration<T>();
    }
}