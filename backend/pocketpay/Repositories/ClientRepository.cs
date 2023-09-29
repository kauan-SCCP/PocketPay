
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using pocketpay.Models;

public class ClientRepository : IClientRepository
{
    private readonly BankContext _context;

    public ClientRepository(BankContext context)
    {
        _context = context;
    }

    public async Task<UserModel> Create(string email, string password, string name, string surname, string cpf) {
        var newAccount = new AccountModel();
        newAccount.Id = new Guid();
        newAccount.Email = email;
        newAccount.Password = BCrypt.Net.BCrypt.HashPassword(password);
        newAccount.Role = "Client";
        await _context.AddAsync(newAccount);

        var newUser = new UserModel();
        newUser.Id = new Guid();
        newUser.Name = name;
        newUser.Surname = surname;
        newUser.CPF = cpf;
        newUser.Account = newAccount;
        await _context.AddAsync(newUser);

        var newWallet = new WalletModel();
        newWallet.Id = new Guid();
        newWallet.Balance = 0;
        newWallet.Account = newAccount;
        await _context.AddAsync(newWallet);
        
        await _context.SaveChangesAsync();

        return newUser;
    }

    public async Task<UserModel> GetById(Guid id)
    {
        var user = await _context.Users.FirstOrDefaultAsync( 
            user => user.Id == id
        );

        return user;
    }

    public async Task<UserModel> GetByEmail(string email)
    {
        var user = await  _context.Users
            .Include(user => user.Account)
            .FirstOrDefaultAsync(user => user.Account.Email == email);
        
        return user;
    }

    public async Task<UserModel> GetByCPF(string cpf)
    {
        var user = await _context.Users.FirstOrDefaultAsync(
            user => user.CPF == cpf
        );

        return user;
    }

    public async Task<UserModel> UpdateById(Guid id, AccountModel account, string name, string surname, string cpf)
    {
        var user = await _context.Users.FirstOrDefaultAsync(
            user => user.Id == id
        );

        if (user == null) {return null;}

        user.Account = account;
        user.Name = name;
        user.Surname = surname;
        user.CPF = cpf;

        await _context.SaveChangesAsync();

        return user;
    }

    public async Task<UserModel> Delete(Guid id)
    {
        var user = await _context.Users.FirstOrDefaultAsync( 
            user => user.Id == id
        );

        if (user == null) {return null;}

        _context.Remove(user);

        return user;
    }

}