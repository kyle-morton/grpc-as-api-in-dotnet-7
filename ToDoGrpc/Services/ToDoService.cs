using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using ToDoGrpc.Data;
using ToDoGrpc.Models;

namespace ToDoGrpc.Services;

public class ToDoService : ToDoIt.ToDoItBase
{
    public AppDbContext _dbContext { get; }
    public ToDoService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override async Task<CreateToDoResponse> CreateToDo (CreateToDoRequest request, ServerCallContext context) 
    {
        if (request.Title == string.Empty || request.Description == string.Empty) {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Title & Description are required"));
        }

        // KM_TODO: Use automapper for this stuff...
        var todoItem = new ToDoItem {
            Title = request.Title,
            Description = request.Description
        };

        await _dbContext.AddAsync(todoItem);
        await _dbContext.SaveChangesAsync();

        return await Task.FromResult(new CreateToDoResponse {
            Id = todoItem.Id
        });
    }

}
