using API.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.DAL.Context
{
    public class ServiceResponse
    {
        public bool Success { get; set; } = true;
        public string? Message { get; set; } = null;
        public string? Token { get; set; } = null;
        public Guid Id { get; set; }
        public string? Username { get; set; }

        public ServiceResponse()
        {

        }

        public ServiceResponse(User user, string accessToken, string message)
        {
            Id = user.ID;
            Username = user.Username;
            Token = accessToken;
            Message = message;
        }

        public ServiceResponse(string message)
        {
            Success = false;
            Message = message;
        }
    }
}
