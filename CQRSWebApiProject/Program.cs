using MediatR;
using AutoMapper;
using CQRSWebApiProject.Business.MapProfiles;
using CQRSWebApiProject.DAL.Concrete.EntityFramework.Context;
using Microsoft.EntityFrameworkCore;
using FluentValidation.AspNetCore;
using CQRSWebApiProject.Business.Validators.General;
using Kanbersky.Customer.Business.Extensions;
using CQRSWebApiProject.DAL.Concrete.EntityFramework.GenericRepository;
using Common.Messaging;
using System;
using CQRSWebApiProject.DAL.Concrete.Redis.Concrete;
using CQRSWebApiProject.DAL.Concrete.Redis.Abstract;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMediatR(AppDomain.CurrentDomain.GetAssemblies());



builder.Services.AddMvc(options =>
{
    options.Filters.Add(new ResponseValidator());
});

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(opt =>
    {
        opt.SuppressModelStateInvalidFilter = true;
    })
    .AddFluentValidation();

//builder.Services.AddDbContext<WriteDbContext>(options =>
//    options.UseInMemoryDatabase("InMem"));

//builder.Services.AddDbContext<ReadDbContext>(options =>
//        options.UseInMemoryDatabase("InMem"));

builder.Services.AddDbContext<ReadDbContext>(opt =>
      opt.UseSqlServer(builder.Configuration.GetConnectionString("PlatformsConn")));

builder.Services.AddDbContext<WriteDbContext>(opt =>
      opt.UseSqlServer(builder.Configuration.GetConnectionString("PlatformsConn")));


//A�a��daki 2 �ekilde de ekleme yap�labilir. 
builder.Services.AddSingleton(
    MessageQueueFactory.CreateProvider());// 
//builder.Services.AddSingleton<IMessageQueueProvider>(sp =>
//    MessageQueueProviderFactory.CreateProvider(sp));// 
//builder.Services.AddSingleton<ICacheService, CacheService>();

#region service aboneli�i yakla��m�
// clean code yakla��m� a�a��daki yap�y� Extensions klas�r� alt�na ald�m. bu sayede sadace a�a��daki iki sat�r kod ile baya i�lem halletmi� olduk...
//Fluentvalidation k�t�phanesi ile gelen istekleri kolay ve kod kalabal��� olmadan validation yapmam�za yarayan k�t�phanemiz var
//apimizi bu servislere de abone ediyoruz ve daha fazla servis i�ini de orada halledebiliyoruz.
//Core ve Customer servislerindeki startup.cs i�erisindeki kar���kl��� bu yap� ile par�alay�p daha temiz bir hale getirebiliriz....
//builder.Services.AddMediatR(Assembly.GetExecutingAssembly());
//builder.Services.AddSingleton<IValidator<CreateCustomerRequest>, CreateCustomerRequestValidator>();
//builder.Services.AddSingleton<IValidator<UpdateCustomerRequest>, UpdateCustomerRequestValidator>();
builder.Services.RegisterHandlers();
builder.Services.RegisterValidators();
builder.Services.RedisConfiguration();
#endregion


var mappingConfig = new MapperConfiguration(mc =>
{
    mc.AddProfile(new AutoMappingProfiels());
});
IMapper mapper = mappingConfig.CreateMapper();
builder.Services.AddSingleton(mapper);
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

var app = builder.Build();





// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
