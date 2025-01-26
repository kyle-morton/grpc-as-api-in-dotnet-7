using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
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

    /*
    * This class is an implementation of the grpc contract we create in the proto file (see protos folder)
    * Each method here is an override of the methods there w/ actual logic to connect it up to a DB.
    */
    
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

    public override async Task<ReadToDoResponse> ReadToDo(ReadToDoRequest request, ServerCallContext context)
    {
        if (request.Id <= 0) {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Resource Id must be greater than 0"));            
        }

        var todoItem = await _dbContext.ToDoItems.FirstOrDefaultAsync(t => t.Id == request.Id);

        if (todoItem == null) {
            throw new RpcException(new Status(StatusCode.NotFound, "No resource found with given ID"));
        }

        return await Task.FromResult(new ReadToDoResponse{
            Id = todoItem.Id,
            Description = todoItem.Description,
            Title = todoItem.Title,
            ToDoStatus = todoItem.ToDoStatus.ToString()
        });
    }

}
