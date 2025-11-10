using System;
using System.Collections.Generic;

namespace UserCreation.Business.Dto
{
    public class UserWithOrdersViewModel
    {
        public Guid UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public List<OrderViewModel> Orders { get; set; } = new();
    }

    public class OrderViewModel
    {
        public Guid Id { get; set; }
        public string Product { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
