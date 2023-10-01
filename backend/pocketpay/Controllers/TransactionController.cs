﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using pocketpay.DTOs.Transaction;
using pocketpay.Models;
using System.Security.Cryptography.Xml;

namespace pocketpay.Controllers;

[ApiController]
[Route("api/v1/transaction")]
public class TransactionController : ControllerBase
{
    private readonly IAccountRepository _accountRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly ITransactionRepository _transactionRepository;
    public TransactionController(IAccountRepository accountRepository, IWalletRepository walletRepository, ITransactionRepository transactionRepository)
    {
        this._accountRepository = accountRepository;
        this._walletRepository = walletRepository;
        this._transactionRepository = transactionRepository;
    }

    [HttpPost("")]
    [Authorize]

    public async Task<IActionResult> CreateTransaction(TransactionRegisterRequest request)
    {
        if (request.email_receiver == null || request.value <= 0)
        {
            return BadRequest();
        }
        
        if (User.Identity == null || User.Identity.Name == null)
        {
            return Forbid();
        }

        var sender = await _accountRepository.FindByEmail(User.Identity.Name);
        if (sender == null)
        {
            return Forbid();
        }

        var senderWallet = await _walletRepository.FindByAccount(sender);
        if (senderWallet == null)
        {
            return BadRequest();
        }

        if (senderWallet.Balance < request.value)
        {
            return Forbid();
        }

        var receiver = await _accountRepository.FindByEmail(request.email_receiver);
        if (receiver == null)
        {
            return NotFound();
        }

        var newTransaction = _transactionRepository.Create(sender, receiver, request.value); //registro minha transação
        await _walletRepository.Withdraw(senderWallet.Id, request.value); // pego qual é a minha carteira
        
        var walletReceiver = await _walletRepository.FindByAccount(receiver); // pego a carteira do destinatario
        if (walletReceiver == null)
        {
            return NotFound();
        }
        
        await _walletRepository.Deposit(walletReceiver.Id, request.value); // realizo o deposito

        return Ok();
    }

    [HttpGet("")]
    [Authorize]
    public async Task<IActionResult> GetUserTransaction() 
    {
        if (User.Identity == null || User.Identity.Name == null)
        {
            return Forbid();
        }

        var account = await _accountRepository.FindByEmail(User.Identity.Name);

        if (account == null)
        {
            return BadRequest();
        }

        var AllTransaction = await _transactionRepository.FindByAccount(account);
        var responseBory = new List<TransactionResponse>();

        foreach (TransactionModel T in AllTransaction)
        {
            var transaction = new TransactionResponse()
            {
                receiverEmail = T.To.Email,
                timeStamp = T.TimeStamp,
                senderEmail = T.From.Email,
                value = T.Value
            };

            responseBory.Add(transaction);
        }
        return Ok(responseBory);
    }
        


}
