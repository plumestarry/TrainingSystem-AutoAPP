namespace AutoAPI.Context.Entity
{
    public class BaseEntity
    {
        public int Id { get; set; }

        public string UserName { get; set; }

        public DateTime CreateDate { get; set; }

        public DateTime UpdateDate { get; set; }
    }
}
