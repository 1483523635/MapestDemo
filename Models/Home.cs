using Mapster;

namespace MappingTest.Models
{
    public class Home
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public Address Address { get; set; }
        public Owner Owner { get; set; }
        
        [AdaptIgnore]
        public string Ignore { get; set; }
    }
}