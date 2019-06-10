namespace MappingTest.Dtos
{
    public class AddressDto
    {
        public AddressDto(string name)
        {
            this.Name = name;
        }
        public string Name { get; set; }
    }
}