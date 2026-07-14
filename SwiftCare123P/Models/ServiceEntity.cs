using SQLite;

namespace SwiftCare123P.Models;

public class ServiceEntity
{
    [PrimaryKey, AutoIncrement]
    public int ServiceID { get; set; }

    public string ServiceName { get; set; } = string.Empty;
}