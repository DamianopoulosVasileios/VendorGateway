using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;
using VendorGateway.Application.Dtos;
using VendorGateway.Application.Interfaces;

namespace VendorGateway.Application.Services.Authorization
{
    public class PasswordHasherService : IPasswordHasherService
    {
        private readonly PasswordHasher<User> passwordHasher = new();

        public string Hash(string password)
        {
            return passwordHasher.HashPassword(new User(), password);
        }

        public bool Verify(string password, string passwordHash)
        {
            var result = passwordHasher.VerifyHashedPassword(
                new User(),
                passwordHash,
                password);

            return result != PasswordVerificationResult.Failed;
        }
    }
}
