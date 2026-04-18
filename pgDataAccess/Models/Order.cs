namespace pgDataAccess.Models;

public class Order
{
    public int Id { get; set; }

    public int ClientId { get; set; }

    public int PickupPointId { get; set; }

    public DateTime OrderDate { get; set; }

    public DateTime DeliveryDate { get; set; }

    public string Status { get; set; } = string.Empty;

    public string PickupCode { get; set; } = string.Empty;

    public User? Client { get; set; }

    public PickupPoint? PickupPoint { get; set; }

    public ICollection<OrderItem> Items { get; } = new List<OrderItem>();
}
