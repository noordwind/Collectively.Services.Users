﻿using Coolector.Common.Commands;

namespace Coolector.Services.Users.Shared.Commands
{
    public class ChangeUserName : IAuthenticatedCommand
    {
        public Request Request { get; set; }
        public string UserId { get; set; }
        public string Name { get; set; }
    }
}