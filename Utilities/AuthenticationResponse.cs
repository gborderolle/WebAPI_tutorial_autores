﻿namespace WebAPI_tutorial_recursos.Utilities
{
    public class AuthenticationResponse
    {
        public string Token { get; set; }
        public DateTime Expiration { get; set; }
    }
}
