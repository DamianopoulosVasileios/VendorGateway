using System;
using System.Collections.Generic;
using System.Text;

namespace VendorGateway.Application.Dtos
{
    public class User
    {
        public int Id { get; set; }

        public string Username { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;
    }
}
