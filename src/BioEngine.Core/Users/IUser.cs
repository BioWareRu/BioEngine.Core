﻿namespace BioEngine.Core.Users
{
    public interface IUser
    {
        int Id { get; set; }
        string Name { get; set; }
        string PhotoUrl { get; set; }
        string ProfileUrl { get; set; }
    }
}