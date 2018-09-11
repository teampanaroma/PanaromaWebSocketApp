namespace Panaroma.OKC.Integration.Library
{
    public class Bank
    {
        public string Name { get; set; }
        public int Id { get; set; }

        public Bank(int id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}