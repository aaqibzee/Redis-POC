using Redis.OM.Modeling;

namespace Test_App
{
    [Document(IndexName = "address-idx", StorageType = StorageType.Json)]
    public partial class Address
    {
        public string StreetName { get; set; }
        [Indexed]
        public string ZipCode { get; set; }
        [Indexed] 
        public string City { get; set; }
        [Indexed] 
        public string State { get; set; }
        [Indexed(CascadeDepth = 1)] 
        public Address ForwardingAddress { get; set; }
        [Indexed] 
        public GeoLoc Location { get; set; }
        [Indexed] 
        public int HouseNumber { get; set; }
    }
}
