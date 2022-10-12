
namespace API.Entities
{
    public class Role
    {
        public Guid ID { get; set; }
        public string Name { get; set; }
        public List<UserRole> UserRoles { get; set; }
    }
}
