﻿using ContactsManager.Core.Domain.IdentityEntities;
using Hotel_Core.RabbitMQ;
using Hotel_Core.ServiceContracts;
using Hotel_Core.Services;
using Hotel_Infrastructure.DbContext;
using Hotel_Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Serialization;
using RawRabbit.Configuration;
using RawRabbit;
using RawRabbit.DependencyInjection.ServiceCollection;
using RawRabbit.Instantiation;
using Repositories;
using RepositoryContracts;
using ServiceContracts;
using Services;
using Microsoft.Extensions.DependencyInjection;
using RawRabbit.DependencyInjection;
using IDependencyResolver = RawRabbit.DependencyInjection.IDependencyResolver;


namespace Hotel_UI
{
 public static class ConfigureServicesExtension
 {
  public static IServiceCollection ConfigureServices(this IServiceCollection services, IConfiguration configuration)
  {
   services.AddControllers();
   
   //add services into IoC container
   services.AddScoped<IApplicationDbContext, ApplicationDbContext>();
   
   services.AddScoped<IUnitOfWork, UnitOfWork>();
   
   //services.Configure<RabbitMqOptions>(configuration.GetSection("RabbitMQ"));
   //services.AddTransient<RabbitMqProducer>();
   
   services.AddScoped<IBookingsRepository, BookingsRepository>();
   services.AddScoped<IBookingsGetterService, BookingsGetterService>();
   services.AddScoped<IBookingsDeleterService, BookingsDeleterService>();
   services.AddScoped<DeleteBookingConsumer>();
   services.AddHostedService<DeleteBookingWorker>();
   
   //services.AddSingleton<ILogger<BookingsUpdaterService>>();

   services.AddScoped<IBookingsUpdaterService, BookingsUpdaterService>();
   services.AddScoped<UpdateBookingConsumer>();
   services.AddHostedService<UpdateBookingWorker>();
   
   services.AddSingleton<RabbitMqProducer>();

   services.AddRawRabbit(new RawRabbitOptions
   {
    ClientConfiguration = configuration.GetSection("RawRabbit").Get<RawRabbit.Configuration.RawRabbitConfiguration>(),
    DependencyInjection = ioc =>
    {
     ioc.AddSingleton<IBookingsUpdaterService, BookingsUpdaterService>(resolver => new BookingsUpdaterService(resolver.GetService<RabbitMqProducer>(), resolver.GetService<ILogger<BookingsUpdaterService>>()));
     ioc.AddSingleton<UpdateBookingConsumer>(resolver => { var scopeFactory = resolver.GetService<IServiceScopeFactory>(); return new UpdateBookingConsumer(scopeFactory); });
    }
   });

   services.AddScoped<ICabinsRepository, CabinsRepository>();
   services.AddScoped<ICabinsGetterService, CabinsGetterService>();
   services.AddScoped<ICabinsDeleterService, CabinsDeleterService>();
   services.AddScoped<ICabinsUpdaterService, CabinsUpdaterService>();
   services.AddScoped<ICabinsAdderService, CabinsAdderService>();
   
   services.AddScoped<ISettingRepository, SettingRepository>();
   services.AddScoped<ISettingGetterService, SettingGetterService>();
   services.AddScoped<ISettingUpdaterService, SettingUpdaterService>();

   services.AddDbContext<ApplicationDbContext>(options =>
   {
    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
   });
   
   services.AddEndpointsApiExplorer();

   services.AddApiVersioning(options =>
   {
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
   });

   services.AddResponseCompression(options =>
   {
    options.EnableForHttps = true;
   });
   
   services.AddCors(options =>
   {
    options.AddPolicy("AllowSpecificOrigin", builder =>
    {
     builder.WithOrigins("http://localhost:3090")
      .AllowAnyHeader()
      .AllowAnyMethod()
      .AllowCredentials();
    });
   });

   
   services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
     options.SerializerSettings.ContractResolver = new DefaultContractResolver
     {
      NamingStrategy = new CamelCaseNamingStrategy()
     };
    });
   
   services.AddIdentity<ApplicationUser, ApplicationRole>(options => {
     options.Password.RequiredLength = 5;
     options.Password.RequireNonAlphanumeric = false;
     options.Password.RequireUppercase = false;
     options.Password.RequireLowercase = true;
     options.Password.RequireDigit = false;
     options.Password.RequiredUniqueChars = 3; //Eg: AB12AB (unique characters are A,B,1,2)
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()

    .AddDefaultTokenProviders()

    .AddUserStore<UserStore<ApplicationUser, ApplicationRole, ApplicationDbContext, Guid>>()

    .AddRoleStore<RoleStore<ApplicationRole, ApplicationDbContext, Guid>>();
   
   services.AddHttpLogging(options =>
   {
    options.LoggingFields = Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.RequestProperties | Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.ResponsePropertiesAndHeaders;
   });

   return services;
  }
 }
}
