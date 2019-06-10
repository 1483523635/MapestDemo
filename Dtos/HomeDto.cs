using System.Security.Cryptography.X509Certificates;

namespace MappingTest.Dtos
{
    public class HomeDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public OwnerDto Owner { get; set; }
        public string OwnerName { get; set; }
        public AddressDto Address { get; set; }
        public string AddressName { get; set; }
        
        public string Ignore { get; set; }
    }
}