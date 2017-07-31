﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharedResourcesLibrary.AccountResources;
using SharedResourcesLibrary.TransactionResources;

namespace DataAccessLibrary
{
    public interface IDataAccess
    {
        User FindUser(String login, String password);
        User FindUserById(int id);
        User FindUserByLogin(String login);
        User FindUserByToken(String TokenKey);
        bool CheckForUser(String username);
        void AddUser(String username, String password, String firstName, String lastName);
        void AddUser(User user);
        List<User> GetAllUsers();
        void AddTransaction(Transaction trans);
        List<Transaction> GetTransactionsForUser(int userID);
        bool ConfirmTransaction(Transaction trans);
        bool ResetTokenForUser(int userId);
    }
}