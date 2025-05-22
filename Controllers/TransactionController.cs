using DotNetCore.CAP;
using FinalConsistencyDemo.Data;
using FinalConsistencyDemo.Models;
using FinalConsistencyDemo.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace FinalConsistencyDemo.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TransactionController : ControllerBase
{
    private readonly CentralDbContext _central;
    private readonly ICapPublisher _cap;

    public TransactionController(CentralDbContext central, ICapPublisher cap)
    {
        _central = central;
        _cap = cap;
    }

    [HttpPost("transfer")]
    public async Task<IActionResult> Transfer(string from, string to, decimal amount)
    {
        var tranId = Guid.NewGuid().ToString("N");
        // 让 CAP 托管 EF Core 的事务
        using var tran = _central.Database.BeginTransaction(_cap);
        try
        {

       
        _central.TranLogs.Add(new TranLog
        {
            TranId = tranId,
            FromAccount = from,
            ToAccount = to,
            Amount = amount
        });

        _central.MessageQueues.Add(new MessageQueue { TranId = tranId, Account = from, Delta = -amount, Type = "DEBIT" });
        _central.MessageQueues.Add(new MessageQueue { TranId = tranId, Account = to, Delta = +amount, Type = "CREDIT" });

        await _central.SaveChangesAsync();

        // 发布两条逻辑消息到 RabbitMQ
        await _cap.PublishAsync("finance.debit", new { TranId = tranId, Account = from, Delta = -amount });
        await _cap.PublishAsync("finance.credit", new { TranId = tranId, Account = to, Delta = amount });


        await tran.CommitAsync();
        }
        catch (Exception e)
        {
            await tran.RollbackAsync();
        }
        return Ok(new { TranId = tranId });
    }
}