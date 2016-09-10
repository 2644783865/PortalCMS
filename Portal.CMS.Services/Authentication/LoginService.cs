﻿using Portal.CMS.Entities;
using System;
using System.Linq;
using System.Security.Cryptography;

namespace Portal.CMS.Services.Authentication
{
    public interface ILoginService
    {
        int? Login(string emailAddress, string password);

        int? SSO(int userId, string token);
    }

    public class LoginService : ILoginService
    {
        #region Dependencies

        private readonly PortalEntityModel _context;
        private readonly ITokenService _tokenService;

        public LoginService(PortalEntityModel context, ITokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        #endregion Dependencies

        public int? Login(string emailAddress, string password)
        {
            var userAccount = _context.Users.FirstOrDefault(x => x.EmailAddress.Equals(emailAddress, System.StringComparison.OrdinalIgnoreCase));

            if (userAccount == null)
                return null;

            if (!CompareSecurePassword(password, userAccount.Password))
                return null;

            return userAccount.UserId;
        }

        private static bool CompareSecurePassword(string passwordAttempt, string passwordActual)
        {
            var savedPasswordHash = passwordActual;
            var hashBytes = Convert.FromBase64String(savedPasswordHash);
            var salt = new byte[16];
            Array.Copy(hashBytes, 0, salt, 0, 16);
            using (var pbkdf2 = new Rfc2898DeriveBytes(passwordAttempt, salt, 10000))
            {
                var hash = pbkdf2.GetBytes(20);
                for (int i = 0; i < 20; i++)
                    if (hashBytes[i + 16] != hash[i])
                        return false;
                return true;
            }
        }

        public int? SSO(int userId, string token)
        {
            var tokenResult = _tokenService.RedeemSSOToken(userId, token);

            if (string.IsNullOrWhiteSpace(tokenResult))
            {
                var userAccount = _context.Users.FirstOrDefault(x => x.UserId == userId);

                if (userAccount == null)
                    return null;

                return userAccount.UserId;
            }

            return null;
        }
    }
}