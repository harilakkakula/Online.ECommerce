using System;

namespace UserCreation.Business.Entities
{
    public class RefOrder
    {
        public Guid Id { get; private set; }
        public Guid UserId { get; private set; }
        public string Product { get; private set; } = string.Empty;
        public int Quantity { get; private set; }
        public decimal Price { get; private set; }

        public RefOrder(Guid id, Guid userId, string product, int quantity, decimal price)
        {
            Id = id;
            UserId = userId;
            Product = product;
            Quantity = quantity;
            Price = price;
        }
        public RefOrder()
        {
            
        }
    }
}
