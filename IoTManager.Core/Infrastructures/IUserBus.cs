﻿using System;
using System.Collections.Generic;
using System.Text;
using IoTManager.Model;
using IoTManager.Utility.Serializers;

namespace IoTManager.Core.Infrastructures
{
    public interface IUserBus
    {
        List<UserSerializer> GetAllUsers();
        UserSerializer GetUserById(int id);
        UserSerializer GetUserByUserName(String userName);
        String CreateNewUser(UserSerializer userSerializer);
        String UpdateUser(int id, UserSerializer userSerializer);
        String DeleteUser(int id);
        List<UserSerializer> GetUsersByUserName(String userName);
        String UpdatePassword(String userName, String password);
        List<String> GetAuthByUserId(int userId);
    }
}
