using Socigy.OpenSource.DB.Attributes;

namespace UnitTest.DB
{
    [Table("test_table")]
    public partial class Tester
    {
        [PrimaryKey]
        public Guid Id { get; set; }
    }
}
