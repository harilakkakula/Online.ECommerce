using System;

namespace OrderCreation.Business.Dto
{
    public class OrderCreationDto
    {
        public Guid UserId { get; set; }
        public string Product { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
